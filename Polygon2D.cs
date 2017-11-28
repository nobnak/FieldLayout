using Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2D : MonoBehaviour, ILayer {
        public enum WhichSideEnum { Unknown = 0, Inside, Outside }

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        public UnityEvent OnGenerate;

        [SerializeField] protected List<Vector2> normalizedVertices = new List<Vector2>();

        protected Validator validator = new Validator();
        protected Matrix4x4 layerMatrix;
        protected Matrix4x4 inverseLayerMatrix;
        protected Matrix4x4 localMatrix;
        protected Matrix4x4 inverseLocalMatrix;
        protected List<Vector2> layerVertices = new List<Vector2>();
        protected List<Edge2D> layerEdges = new List<Edge2D>();

        #region Unity
        protected virtual void OnEnable() {
            validator.Reset();
            validator.Validation += () => Validate();
            validator.SetExtraValidityChecker(() => !transform.hasChanged);
        }
        protected virtual void OnValidate() {
            Invalidate();
        }
        protected virtual void OnDisable() {

        }
        #endregion

        #region Vertex
        public virtual int VertexCount {
            get { return normalizedVertices.Count; }
        }
        public virtual Vector2 GetVertex(int i) {
            return normalizedVertices[i];
        }
        public virtual void SetVertex(int i, Vector2 value) {
            Invalidate();
            normalizedVertices[i] = value;
        }
        public virtual int AddVertex(Vector2 v) {
            Invalidate();
            var i = normalizedVertices.Count;
            normalizedVertices.Add(v);
            return i;
        }
        public virtual void RemoveVertex(int i) {
            Invalidate();
            normalizedVertices.RemoveAt(i);
        }
        public virtual IEnumerable<Vector2> IterateVertices() {
            validator.CheckValidation();
            foreach (var v in layerVertices)
                yield return v;
        }
        #endregion

        #region Edge
        public virtual IEnumerable<Edge2D> IterateEdges() {
            validator.CheckValidation();
            foreach (var e in layerEdges)
                yield return e;
        }
        #endregion

        #region ILayer
        public virtual Matrix4x4 LayerMatrix {
            get { return layerMatrix; }
        }
        public virtual Matrix4x4 InverseLayerMatrix {
            get { return inverseLayerMatrix; }
        }
        public virtual Matrix4x4 LocalMatrix {
            get { return localMatrix; }
        }
        public virtual Matrix4x4 InverseLocalMatrix {
            get { return inverseLocalMatrix; }
        }
        public virtual Vector3 LayerToLocal(Vector3 layerPosition) {
            return inverseLayerMatrix.MultiplyPoint3x4(layerPosition);
        }
        public virtual Vector3 LocalToLayer(Vector3 normalizedPosition) {
            return localMatrix.MultiplyPoint3x4(normalizedPosition);
        }
        public virtual Vector3 WorldToLayer(Vector3 worldPosition) {
            return inverseLayerMatrix.MultiplyPoint3x4(worldPosition);
        }
        public virtual Vector3 LayerToWorld(Vector3 localPosition) {
            return layerMatrix.MultiplyPoint3x4(localPosition);
        }
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
        #endregion

        public virtual WhichSideEnum Side(Vector2 p) {
            var totalAngle = 0f;
            foreach (var e in IterateEdges())
                totalAngle += e.Angle(p);
            return (Mathf.RoundToInt(totalAngle * CIRCLE_INV_DEG) != 0)
                ? WhichSideEnum.Inside : WhichSideEnum.Outside;
        }
        public virtual float DistanceToVertex(Vector2 p, out int index) {
            validator.CheckValidation();
            index = -1;

            var minSqDist = float.MaxValue;
            for (var i = 0; i < layerVertices.Count; i++) {
                var v = layerVertices[i];
                var sqDist = (v - p).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    index = i;
                }
            }
            return Mathf.Sqrt(minSqDist);
        }
        public virtual float DistanceToEdge(Vector2 p, out Edge2D resEdge, out float resT) {
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
        public virtual float DistanceToEdgeByWorldPosition(Vector3 worldPos, out Edge2D edge, out float t) {
            return DistanceToEdge(WorldToLayer(worldPos), out edge, out t);
        }

        #region Validation
        protected virtual void Validate() {
            transform.hasChanged = false;
            GenerateLayerData();
        }
        protected virtual void Invalidate() {
            validator.Invalidate();
        }
        #endregion

        protected virtual void GenerateLayerData() {
            layerVertices.Clear();
            layerEdges.Clear();

            layerMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            inverseLayerMatrix = layerMatrix.inverse;

            localMatrix = Matrix4x4.Scale(transform.localScale);
            inverseLocalMatrix = localMatrix.inverse;

            var limit = normalizedVertices.Count;
            for (var i = 0; i < limit; i++) {
                var j = (i + 1) % limit;
                var v0 = (Vector2)localMatrix.MultiplyPoint3x4(normalizedVertices[i]);
                var v1 = (Vector2)localMatrix.MultiplyPoint3x4(normalizedVertices[j]);
                layerVertices.Add(v0);
                layerEdges.Add(new Edge2D(v0, v1));
            }

            OnGenerate.Invoke();
        }
    }
}
