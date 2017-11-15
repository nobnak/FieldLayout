using Gist;
using Gist.InputDevice;
using System.Collections.Generic;
using UnityEngine;
using Gist.Extensions.Behaviour;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class PolygonMaker : MonoBehaviour {

        public static readonly Vector2[] QUAD_VERTICES = new Vector2[] {
            0.5f * new Vector2(-1,-1), 0.5f * new Vector2(-1,1),
            0.5f * Vector2.one, 0.5f * new Vector2(1, -1)
        };

        [SerializeField] protected Polygon2D polygon;
        [SerializeField] protected Color edgeColor = Color.green;
        [SerializeField] protected Color quadColor = Color.grey;

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
                        var sdist = polygon.SignedDistance(p, out edge, out t);
                        var q = polygon.ModelMatrix.MultiplyPoint3x4(edge.GetPosition(t));
                        Debug.DrawLine(p, q, Color.red);
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

                GL.LoadIdentity();
                GL.MultMatrix(view * transform.localToWorldMatrix);
                GL.Begin(GL.LINES);
                glmat.Color(quadColor);
                foreach (var v in IterateQuadEdges())
                    GL.Vertex(v);
                GL.End();

            } finally {
                GL.PopMatrix();
            }
        }
        void OnDisable() {
            glmat.Dispose();
        }
        #endregion

        IEnumerable<Vector2> IterateQuadEdges() {
            for (var i = 0; i < QUAD_VERTICES.Length; i++) {
                var j = (i + 1) % QUAD_VERTICES.Length;
                yield return QUAD_VERTICES[i];
                yield return QUAD_VERTICES[j];
            }
        }
    }
}
