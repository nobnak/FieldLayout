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
        
        protected int selectionIndex = -1;

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

                    float t;
                    if (polygon.Raycast(ray, out t)) {
                        var p = ray.GetPoint(t);
                        var player = (Vector2)polygon.WorldToLayer(p);
                        int index;
                        var d = polygon.DistanceToVertex(player, out index);
                        if (d < selectionDistance) {
                            selectionIndex = index;
                        }
                    }
                }
            };
            mouse.OnSelection += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var c = Camera.main;

                    if (selectionIndex >= 0) {
                        Vector2 dp;
                        if (UnscaledLocalDistance(mt, c, out dp)) {
                            var p = polygon.GetVertex(selectionIndex);
                            polygon.SetVertex(selectionIndex, p + dp);
                        }
                    }
                }
            };
            mouse.OnSelectionUp += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    selectionIndex = -1;
                }
            };
        }

        protected bool UnscaledLocalDistance(MouseTracker mt, Camera c, out Vector2 dp) {
            dp = default(Vector2);

            var rayPrev = c.ScreenPointToRay(mt.PrevPosition);
            var rayCurr = c.ScreenPointToRay(mt.CurrPosition);

            float tprev, tcurr;
            if (polygon.Raycast(rayPrev, out tprev) && polygon.Raycast(rayCurr, out tcurr)) {
                dp = polygon.InverseLayerMatrix.MultiplyVector(
                    rayCurr.GetPoint(tcurr) - rayPrev.GetPoint(tprev));
                return true;
            }
            return false;
        }

        void Update() {
            mouse.Update();
        }
        void OnRenderObject() {
            if (glmat == null || !polygon.IsActiveAndEnabledAlsoInEditMode())
                return;

            var view = Camera.current.worldToCameraMatrix;
            var modelView = view * polygon.LayerMatrix;
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

                if (selectionIndex >= 0) {
                    var v = polygon.GetVertex(selectionIndex);
                    var quadShape = Matrix4x4.TRS(v, Quaternion.identity, 0.1f * Vector3.one);
                    glfig.FillQuad(modelView * quadShape, edgeColor);
                }

            } finally {
                GL.PopMatrix();
            }
        }
        void OnDisable() {
            glfig.Dispose();
            glmat.Dispose();
        }
        #endregion
    }
}
