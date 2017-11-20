using Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2D : MonoBehaviour {
        public enum WhichSideEnum { Unknown = 0, Inside, Outside }

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        public UnityEvent OnGenerate;

        [SerializeField] protected List<Vector2> vertices = new List<Vector2>();

        protected Validator validator;
        protected Matrix4x4 scaledModelMatrix;
        protected Matrix4x4 scaledInverseModelMatrix;
        protected List<Vector2> scaledVertices = new List<Vector2>();
        protected List<Edge2D> scaledEdges = new List<Edge2D>();

        #region Unity
        protected virtual void OnEnable() {
            validator = new Validator();
            validator.Validation += () => Validate();
            validator.SetExtraValidityChecker(() => !transform.hasChanged);
        }
        protected virtual void OnValidate() {
            Invalidate();
        }
        #endregion

        #region List
        public int Count { get { return vertices.Count; } }
        public Vector2 this[int i] {
            get { return vertices[i]; }
            set {
                Invalidate();
                vertices[i] = value;
            }
        }

        public int Add(Vector2 v) {
            Invalidate();
            var i = vertices.Count;
            vertices.Add(v);
            return i;
        }
        public Vector2 Remove(int i) {
            Invalidate();
            var v = vertices[i];
            vertices.RemoveAt(i);
            return v;
        }
        #endregion

        public virtual IEnumerable<Vector2> IterateVertices(bool scaled = true) {
            validator.CheckValidation();
            foreach (var v in scaledVertices)
                yield return v;
        }
        public virtual IEnumerable<Edge2D> IterateEdges(bool scaled = true) {
            validator.CheckValidation();
            foreach (var e in scaledEdges)
                yield return e;
        }

        #region Transform
        public virtual Matrix4x4 ModelMatrix {
            get { return scaledModelMatrix; }
        }
        public virtual Vector2 LocalPosition(Vector3 worldPosition) {
            return (Vector2)scaledInverseModelMatrix.MultiplyPoint3x4(worldPosition);
        }
        public virtual Vector3 WorldPosition(Vector2 localPosition) {
            return scaledModelMatrix.MultiplyPoint3x4(localPosition);
        }
        #endregion

        public virtual bool Raycast(Ray ray, out float distance) {
            distance = default(float);

            var n = transform.forward;
            var c = transform.position;
            var det = Vector3.Dot(n, ray.direction);
            if (-EPSILON < det && det < EPSILON)
                return false;

            distance = Vector3.Dot(n, c - ray.origin) / det;
            return true;
        }
        public virtual WhichSideEnum Side(Vector2 p) {
            var totalAngle = 0f;
            foreach (var e in IterateEdges())
                totalAngle += e.Angle(p);
            return (Mathf.RoundToInt(totalAngle * CIRCLE_INV_DEG) != 0)
                ? WhichSideEnum.Inside : WhichSideEnum.Outside;
        }
        public virtual float Distance(Vector2 p, out Edge2D resEdge, out float resT) {
            validator.CheckValidation();

            resEdge = default(Edge2D);
            resT = default(float);

            var minDist = float.MaxValue;
            foreach (var e in IterateEdges()) {
                float t;
                var dist = e.Distance(p, out t);
                if (dist < minDist) {
                    minDist = dist;
                    resEdge = e;
                    resT = t;
                }
            }
            return minDist;
        }
        public virtual float DistanceByWorldPosition(Vector3 worldPos, out Edge2D edge, out float t) {
            return Distance(LocalPosition(worldPos), out edge, out t);
        }

        #region Validation
        protected virtual void Validate() {
            transform.hasChanged = false;
            GenerateScaledData();
        }
        protected virtual void Invalidate() {
            if (validator != null)
                validator.Invalidate();
        }
        #endregion

        protected virtual void GenerateScaledData() {
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

            OnGenerate.Invoke();
        }
    }
}
