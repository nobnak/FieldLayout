using Gist;
using Gist.InputDevice;
using System.Collections.Generic;
using UnityEngine;
using Gist.Extensions.Behaviour;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class PolygonMaker : MonoBehaviour {

        [SerializeField] protected Polygon2D polygon;
        [SerializeField] protected Color edgeColor = Color.green;

        protected GLMaterial glmat;
        protected MouseTracker mouse;

        #region Unity
        void OnEnable() {
            glmat = new GLMaterial();
            mouse = new MouseTracker();

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();

            mouse.OnSelection += (mt, f) => {
                if ((f & MouseTracker.ButtonFlag.Left) != 0) {
                    var ray = Camera.main.ScreenPointToRay(mt.CurrPosition);

                    float t;
                    if (polygon.Raycast(ray, out t)) {
                        Debug.DrawRay(ray.origin, t * ray.direction, Color.magenta);

                        var p = ray.GetPoint(t);
                        Edge2D edge;
                        polygon.DistanceByWorldPosition(p, out edge, out t);
                        var q = polygon.ModelMatrix.MultiplyPoint3x4(edge.GetPosition(t));

                        var side = polygon.Side(p);
                        Debug.DrawLine(p, q, (side == Polygon2D.WhichSideEnum.Inside ? Color.red : Color.green));
                    }
                }
            };
        }

        void Update() {
            mouse.Update();
        }
        void OnRenderObject() {
            if (glmat == null || !polygon.IsActiveAndEnabledAlsoInEditMode())
                return;

            var view = Camera.current.worldToCameraMatrix;

            GL.PushMatrix();
            try {
                GL.LoadIdentity();
                GL.MultMatrix(view * polygon.ModelMatrix);

                GL.Begin(GL.LINES);
                glmat.Color(edgeColor);
                foreach (var e in polygon.IterateEdges(false)) {
                    GL.Vertex(e.v0);
                    GL.Vertex(e.v1);
                }
                GL.End();

            } finally {
                GL.PopMatrix();
            }
        }
        void OnDisable() {
            glmat.Dispose();
        }
        #endregion
    }
}
