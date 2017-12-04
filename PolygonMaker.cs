using Gist;
using Gist.InputDevice;
using System.Collections.Generic;
using UnityEngine;
using Gist.Extensions.Behaviour;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class PolygonMaker : MonoBehaviour {

        [SerializeField] protected Color edgeColor = Color.green;
        [SerializeField] protected float selectionDistance = 5f;

        protected GLMaterial glmat;
        protected GLFigure glfig;
        protected MouseTracker mouse;

        protected List<Polygon2D> polygons;
        protected PolygonSelection selection = new PolygonSelection();

        #region Unity
        void OnEnable() {
            glmat = new GLMaterial();
            glfig = new GLFigure();
            mouse = new MouseTracker();
            polygons = new List<Polygon2D>(GetComponentsInChildren<Polygon2D>());

            mouse.OnSelectionDown += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var ray = Camera.main.ScreenPointToRay(mt.CurrPosition);

                    float dmin = float.MaxValue;
                    foreach (var polygon in polygons) { 
                        float t;
                        if (polygon.LayerGetter.Raycast(ray, out t)) {
                            var p = ray.GetPoint(t);
                            var player = (Vector2)polygon.LayerGetter.LayerToWorld.InverseTransformPoint(p);
                            var j = polygon.ClosestVertexIndex(player);
                            var v = polygon.GetVertex(j);
                            var sqd = (v - player).sqrMagnitude;
                            if (sqd < selectionDistance && sqd < dmin) {
                                dmin = sqd;
                                selection.Select(polygon, j);
                            }
                        }
                    }
                }
            };
            mouse.OnSelection += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var c = Camera.main;

                    if (selection.selectedVertexIndex >= 0) {
                        Vector2 dp;

                        if (LocalDistance(selection.selectedPolygon, mt, c, out dp)) {
                            var p = selection.GetVertex();
                            selection.SetVertex(p + dp);
                        }
                    }
                }
            };
            mouse.OnSelectionUp += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    selection.Unselect();
                }
            };
        }
        void Update() {
            mouse.Update();
        }
        void OnRenderObject() {
            if (glmat == null)
                return;

            foreach (var polygon in polygons) {
                if (!polygon.IsActiveAndEnabledAlsoInEditMode())
                    continue;

                var view = Camera.current.worldToCameraMatrix;
                var modelView = view * polygon.LayerGetter.LayerToWorld;
                GL.PushMatrix();
                try {
                    GL.LoadIdentity();
                    GL.MultMatrix(modelView);

                    GL.Begin(GL.LINES);
                    glmat.Color(edgeColor);
                    foreach (var e in polygon.IterateEdges()) {
                        GL.Vertex(e.v0);
                        GL.Vertex(e.v1);
                    }
                    GL.End();

                    if (selection.Selected) {
                        var v = selection.GetVertex();
                        v = polygon.LayerGetter.LocalToLayer.InverseTransformPoint(v);
                        var quadShape = Matrix4x4.TRS(v, Quaternion.identity, 0.1f * Vector3.one);
                        glfig.FillQuad(modelView * quadShape, edgeColor);
                    }

                } finally {
                    GL.PopMatrix();
                }
            }
        }
        void OnDisable() {
            glfig.Dispose();
            glmat.Dispose();
        }
        #endregion
        

        protected bool LocalDistance(Polygon2D polygon, MouseTracker mt, Camera c, out Vector2 dp) {
            dp = default(Vector2);

            var rayPrev = c.ScreenPointToRay(mt.PrevPosition);
            var rayCurr = c.ScreenPointToRay(mt.CurrPosition);
            
            var layer = polygon.LayerGetter;
            Vector3 p0, p1;
            if (FindPointOnLayer(layer, rayPrev, out p0) && FindPointOnLayer(layer, rayCurr, out p1)) {
                dp = layer.LocalToLayer.InverseTransformPoint(
                    layer.LayerToWorld.InverseTransformPoint(p1 - p0));
                return true;
            }
            return false;
        }
        protected bool FindPointOnLayer(Layer layer, Ray ray, out Vector3 worldPos) {
            worldPos = default(Vector3);

            float t;
            if (layer.Raycast(ray, out t)) {
                worldPos = ray.GetPoint(t);
                return true;
            }
            return false;
        }


        public class PolygonSelection {
            public Polygon2D selectedPolygon { get; protected set; }
            public int selectedVertexIndex { get; protected set; }

            public PolygonSelection() {
                Unselect();
            }

            public void Select(Polygon2D polygon, int vertexIndex) {
                this.selectedPolygon = polygon;
                this.selectedVertexIndex = vertexIndex;
            }
            public void Unselect() {
                selectedPolygon = null;
                selectedVertexIndex = -1;
            }
            public bool Selected { get {
                    return selectedPolygon != null && selectedVertexIndex >= 0; } }

            public Vector2 GetVertex(int vertexIndex) {
                return selectedPolygon.GetVertex(vertexIndex);
            }
            public Vector2 GetVertex() {
                return GetVertex(selectedVertexIndex);
            }
            public void SetVertex(int vertexIndex, Vector2 p) { 
                selectedPolygon.SetVertex(vertexIndex, p);
            }
            public void SetVertex(Vector2 p) {
                SetVertex(selectedVertexIndex, p);
            }
        }
    }
}
