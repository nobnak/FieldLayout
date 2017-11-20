using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gist;
using UnityEngine.Events;
using Gist.Scoped;
using Gist.Extensions.Behaviour;
using Polyhedra2DZone.SpacePartition;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Accelerator2D : MonoBehaviour {

        [SerializeField] protected Polygon2D polygon;
        [SerializeField] protected Fringe2D fringe;

        [Range(10, 100)]
        [SerializeField] protected int subdivision = 10;
        [SerializeField] protected Color debugColorFringeBoundary = Color.blue;
        [SerializeField] protected Color debugColorCell;
        [Range(-1f,1f)]
        [SerializeField] protected float debugColorShift;

        protected Validator validator;
        protected UniformGrid2D<Cell> grid;
        protected ScopedPlug<UnityEvent> scopeOnGenerate;

        protected GLFigure fig;

        #region Unity
        protected void OnEnable() {
            validator = new Validator();
            grid = new UniformGrid2D<Cell>();
            fig = new GLFigure();
            fig.glmat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();
            if (fringe == null)
                fringe = GetComponent<Fringe2D>();

            var fringeOnGenerate = new UnityAction(() => validator.Invalidate());
            scopeOnGenerate = new ScopedPlug<UnityEvent>(
                fringe.OnGenerate, e => e.RemoveListener(fringeOnGenerate));
            scopeOnGenerate.Data.AddListener(fringeOnGenerate);
            validator.Validation += () => {
                GenerateGrid();
            };
        }
        protected void OnValidate() {
            if (validator != null)
                validator.Invalidate();
        }
        protected void OnRenderObject() {
            if (!this.IsActiveAndEnabledAlsoInEditMode())
                return;

            validator.CheckValidation();

            var modelview = Camera.current.worldToCameraMatrix * polygon.ModelMatrix;
            var bounds = fringe.Bounds;
            var shape = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
            fig.DrawQuad(modelview * shape, debugColorFringeBoundary);

            float h, s, v;
            Color.RGBToHSV(debugColorCell, out h, out s, out v);
            foreach (var cell in grid) {
                var m = modelview * cell.Model;
                var offset = 0f;
                switch (cell.side) {
                    case Cell.Side.Inside:
                        offset = 0f;
                        break;
                    case Cell.Side.Unknown:
                        offset = 1f;
                        break;
                    case Cell.Side.Outside:
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
        protected void OnDisable() {
            scopeOnGenerate.Dispose();
            fig.Dispose();
        }
        #endregion

        protected void GenerateGrid() {
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
                        c.side = (polygon.Side(rect.center) == 0 ? Cell.Side.Outside : Cell.Side.Inside);

                    c.area = rect;
                    grid[x, y] = c;
                }
            }
        }

        public struct Cell {
            public enum Side { Unknown = 0, Inside, Outside }

            public Side side;
            public Rect area;

            public Matrix4x4 Model {
                get {
                    return Matrix4x4.TRS(area.center, Quaternion.identity, area.size);
                }
            }
        }
    }
}
