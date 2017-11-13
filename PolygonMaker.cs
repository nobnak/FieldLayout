using Gist;
using System.Collections.Generic;
using UnityEngine;

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

        void OnEnable() {
            glmat = new GLMaterial();
        }
        void OnDisable() {
            glmat.Dispose();
        }
        void OnRenderObject() {
            if (glmat == null)
                return;

            var view = Camera.current.worldToCameraMatrix;

            GL.PushMatrix();
            GL.LoadIdentity();
            GL.MultMatrix(view * transform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            glmat.Color(edgeColor);
            foreach (var v in polygon.IterateEdges())
                GL.Vertex(v);
            GL.End();

            GL.Begin(GL.LINES);
            glmat.Color(quadColor);
            foreach (var v in IterateQuadEdges())
                GL.Vertex(v);
            GL.End();

            GL.PopMatrix();
            
        }


        IEnumerable<Vector2> IterateQuadEdges() {
            for (var i = 0; i < QUAD_VERTICES.Length; i++) {
                var j = (i + 1) % QUAD_VERTICES.Length;
                yield return QUAD_VERTICES[i];
                yield return QUAD_VERTICES[j];
            }
        }
    }
}
