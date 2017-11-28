using Gist;
using Gist.InputDevice;
using System.Collections.Generic;
using UnityEngine;
using Gist.Extensions.Behaviour;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class PolygonMaker : MonoBehaviour {

        [SerializeField] protected Polygon2D[] polygons;
        [SerializeField] protected Color edgeColor = Color.green;
        [SerializeField] protected float selectionDistance = 5f;

        protected GLMaterial glmat;
        protected GLFigure glfig;
        protected MouseTracker mouse;

        protected Polygon2D selectedPolygon;
        protected int selectedVertexIndex = -1;

        #region Unity
        void OnEnable() {
            glmat = new GLMaterial();
            glfig = new GLFigure();
            mouse = new MouseTracker();

            if (polygons == null)
                polygons = FindObjectsOfType<Polygon2D>();

            mouse.OnSelectionDown += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var ray = Camera.main.ScreenPointToRay(mt.CurrPosition);

                    float dmin = float.MaxValue;
                    foreach (var polygon in polygons) { 
                        float t;
                        if (polygon.LayerGetter.Raycast(ray, out t)) {
                            var p = ray.GetPoint(t);
                            var player = polygon.LayerGetter.LayerToWorld.InverseTransformPoint(p);
                            int j;
                            var d = polygon.DistanceToVertex(player, out j);
                            if (d < selectionDistance && d < dmin) {
                                dmin = d;
                                selectedPolygon = polygon;
                                selectedVertexIndex = j;
                            }
                        }
                    }
                }
            };
            mouse.OnSelection += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var c = Camera.main;

                    if (selectedVertexIndex >= 0) {
                        Vector2 dp;

                        if (UnscaledLocalDistance(selectedPolygon, mt, c, out dp)) {
                            var p = selectedPolygon.GetVertex(selectedVertexIndex);
                            selectedPolygon.SetVertex(selectedVertexIndex, p + dp);
                        }
                    }
                }
            };
            mouse.OnSelectionUp += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    selectedPolygon = null;
                    selectedVertexIndex = -1;
                }
            };
        }

        protected bool UnscaledLocalDistance(Polygon2D polygon, MouseTracker mt, Camera c, out Vector2 dp) {
            dp = default(Vector2);

            var rayPrev = c.ScreenPointToRay(mt.PrevPosition);
            var rayCurr = c.ScreenPointToRay(mt.CurrPosition);

            float tprev, tcurr;
            var layer = polygon.LayerGetter;
            if (layer.Raycast(rayPrev, out tprev) && layer.Raycast(rayCurr, out tcurr)) {
                var v = rayCurr.GetPoint(tcurr) - rayPrev.GetPoint(tprev);
                dp = layer.LocalToLayer.InverseTransformPoint(
                    layer.LayerToWorld.InverseTransformPoint(v));
                return true;
            }
            return false;
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

                    if (polygon == selectedPolygon && selectedVertexIndex >= 0) {
                        var v = polygon.GetVertex(selectedVertexIndex);
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
    }
}
