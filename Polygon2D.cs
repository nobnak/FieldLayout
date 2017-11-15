using Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2D : MonoBehaviour {
        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        [SerializeField] protected List<Vector2> vertices = new List<Vector2>();

        protected Validator scaledDataValidator;
        protected Matrix4x4 scaledModelMatrix;
        protected Matrix4x4 scaledInverseModelMatrix;
        protected List<Vector2> scaledVertices = new List<Vector2>();
        protected List<Edge2D> scaledEdges = new List<Edge2D>();

        #region Unity
        void OnEnable() {
            scaledDataValidator = new Validator();
            scaledDataValidator.Validation += () => {
                transform.hasChanged = false;
                GenerateScaledData();
            };
            scaledDataValidator.SetExtraValidityChecker(() => !transform.hasChanged);
        }
        void OnValidate() {
            if (scaledDataValidator != null)
                scaledDataValidator.Invalidate();
        }
        #endregion

        #region List
        public int Count { get { return vertices.Count; } }
        public Vector2 this[int i] {
            get { return vertices[i]; }
            set {
                scaledDataValidator.Invalidate();
                vertices[i] = value;
            }
        }

        public int Add(Vector2 v) {
            scaledDataValidator.Invalidate();
            var i = vertices.Count;
            vertices.Add(v);
            return i;
        }
        public Vector2 Remove(int i) {
            scaledDataValidator.Invalidate();
            var v = vertices[i];
            vertices.RemoveAt(i);
            return v;
        }
        #endregion

        public IEnumerable<Vector2> IterateVertices(bool scaled = true) {
            scaledDataValidator.CheckValidation();
            foreach (var v in scaledVertices)
                yield return v;
        }
        public IEnumerable<Edge2D> IterateEdges(bool scaled = true) {
            scaledDataValidator.CheckValidation();
            foreach (var e in scaledEdges)
                yield return e;
        }

        #region Transform
        public Matrix4x4 ModelMatrix {
            get { return scaledModelMatrix; }
        }
        public Vector2 LocalPosition(Vector3 worldPosition) {
            return (Vector2)scaledInverseModelMatrix.MultiplyPoint3x4(worldPosition);
        }
        public Vector3 WorldPosition(Vector2 localPosition) {
            return scaledModelMatrix.MultiplyPoint3x4(localPosition);
        }
        #endregion

        public bool Raycast(Ray ray, out float distance) {
            distance = default(float);

            var n = transform.forward;
            var c = transform.position;
            var det = Vector3.Dot(n, ray.direction);
            if (-EPSILON < det && det < EPSILON)
                return false;

            distance = Vector3.Dot(n, c - ray.origin) / det;
            return true;
        }
        public float Distance(Vector2 p, out Edge2D resEdge, out float resT, out int side) {
            scaledDataValidator.CheckValidation();

            resEdge = default(Edge2D);
            resT = default(float);

            var minDist = float.MaxValue;
            var totalAngle = 0f;
            foreach (var e in IterateEdges()) {
                float t;
                var dist = e.Distance(p, out t);
                if (dist < minDist) {
                    minDist = dist;
                    resEdge = e;
                    resT = t;
                }

                totalAngle += e.Angle(p);
            }

            side = Mathf.RoundToInt(totalAngle * CIRCLE_INV_DEG);

            return minDist;
        }
        public float DistanceByWorldPosition(Vector3 worldPos, out Edge2D edge, out float t, out int side) {
            return Distance(LocalPosition(worldPos), out edge, out t, out side);
        }
        
        protected void GenerateScaledData() {
            scaledVertices.Clear();
            scaledEdges.Clear();

            scaledModelMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            scaledInverseModelMatrix = scaledModelMatrix.inverse;

            var scale = (Vector2)transform.localScale;
            var limit = vertices.Count;
            for (var i = 0; i < limit; i++) {
                var j = (i + 1) % limit;
                var v0 = Vector2.Scale(vertices[i], scale);
                var v1 = Vector2.Scale(vertices[j], scale);
                scaledVertices.Add(v0);
                scaledVertices.Add(v1);
                scaledEdges.Add(new Edge2D(v0, v1));
            }
        }
    }
}
