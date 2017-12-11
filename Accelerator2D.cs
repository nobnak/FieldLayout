using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using Polyhedra2DZone.SpacePartition;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Accelerator2D : Polygon2D {
        
        [Range(10, 100)]
        [SerializeField] protected int subdivision = 10;
        [SerializeField] protected Fringe2D fringe;

        [Header("Debug")]
        [SerializeField] protected bool debugEnabled = true;
        [SerializeField] protected bool debugFringeEnabled = true;
        [SerializeField] protected bool debugGridEnabled = true;
        [SerializeField] protected Color debugColorFringeBoundary = Color.blue;
        [SerializeField] protected Color debugColorCell;
        [Range(-1f,1f)]
        [SerializeField] protected float debugColorShift;
        
        protected UniformGrid2D<Cell> grid;
        protected new Validator validator = new Validator();
        protected GLFigure fig;

        #region Unity
        protected override void OnEnable() {
            base.OnEnable();
            
            grid = new UniformGrid2D<Cell>();
            fig = new GLFigure();

            fig.glmat.ZOffset = 1f;
            fringe.Init(this);

            validator.Reset();
            validator.Validation += () => {
                base.validator.CheckValidation();
                fringe.Generate();
                GenerateGrid();
            };
            base.validator.Invalidated += () => validator.Invalidate();
        }
        protected void OnRenderObject() {
            if (!debugEnabled || !this.IsActiveAndEnabledAlsoInEditMode()
                || !validator.CheckValidation())
                return;

            if (debugFringeEnabled) {
                fig.glmat.ZOffset = 0f;
                fringe.OnRenderObject(fig);
            }
            fig.glmat.ZOffset = 1f;

            if (debugGridEnabled) {
                var modelview = Camera.current.worldToCameraMatrix * layer.LayerToWorld;
                var bounds = fringe.Bounds;
                var shape = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
                fig.DrawQuad(modelview * shape, debugColorFringeBoundary);

                float h, s, v;
                Color.RGBToHSV(debugColorCell, out h, out s, out v);
                foreach (var cell in grid) {
                    var m = modelview * cell.Model;
                    var offset = 0f;
                    switch (cell.side) {
                        case WhichSideEnum.Inside:
                            offset = 0f;
                            break;
                        case WhichSideEnum.Unknown:
                            offset = 1f;
                            break;
                        case WhichSideEnum.Outside:
                            offset = 2f;
                            break;
                    }

                    var cellType = h + debugColorShift * offset;
                    cellType -= Mathf.Floor(cellType);

                    var c = Color.HSVToRGB(cellType, s, v);
                    c.a = debugColorCell.a;
                    fig.FillQuad(m, 0.5f * c);
                    fig.DrawQuad(m, c);
                }
            }
        }
        protected override void OnDisable() {
            if (fig != null) {
                fig.Dispose();
                fig = null;
            }
            base.OnDisable();
        }
        #endregion

        public override WhichSideEnum Side(Vector2 p) {
            validator.CheckValidation();
            
            var cell = grid[p];
            var side = cell.side;
            if (side == WhichSideEnum.Unknown)
                side = base.Side(p);

            return side;
        }

        protected virtual void GenerateGrid() {
            subdivision = Mathf.Max(3, subdivision);
            grid.Subdivision = subdivision;

            var bounds = fringe.Bounds;
            var innerCellCount = subdivision - 2;
            var size = bounds.size * (1.01f * subdivision / innerCellCount);
            var min = bounds.center - 0.5f * size;
            var cellSize = size / subdivision;
            grid.Init(min, cellSize);

            for (var y = 0; y < subdivision; y++) {
                for (var x = 0; x < subdivision; x++) {
                    var c = grid[x, y];

                    var pos = new Vector2(x * cellSize.x, y * cellSize.y) + min;
                    var rect = new Rect(pos, cellSize);
                    if (!fringe.Overlaps(rect))
                        c.side = base.Side(rect.center);

                    c.area = rect;
                    grid[x, y] = c;
                }
            }
        }

        public struct Cell {

            public WhichSideEnum side;
            public Rect area;

            public Matrix4x4 Model {
                get {
                    return Matrix4x4.TRS(area.center, Quaternion.identity, area.size);
                }
            }
        }
    }
}
