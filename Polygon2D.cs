using Gist;
using Gist.Extensions.Behaviour;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {
    
    public class Polygon2D : MonoBehaviour {
        public enum WhichSideEnum { Unknown = 0, Inside, Outside }

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        public UnityEvent OnGenerate;

        [SerializeField] protected Layer layer;
        [SerializeField] protected List<Vector2> normalizedVertices = new List<Vector2>();

        protected Validator validator = new Validator();
        protected List<Vector2> layerVertices = new List<Vector2>();
        protected List<Edge2D> layerEdges = new List<Edge2D>();

        #region Unity
        protected virtual void OnEnable() {
            validator.Reset();
            validator.Validation += () => {
                layer.ValidatorGetter.CheckValidation();
                transform.hasChanged = false;
                GenerateLayerData();
            };
            layer.ValidatorGetter.Invalidated += () => validator.Invalidate();
            validator.SetCheckers(() => 
                !transform.hasChanged && (layer != null && layer.IsActiveAndEnabledAlsoInEditMode()));
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
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
            validator.Invalidate();
            normalizedVertices[i] = value;
        }
        public virtual int AddVertex(Vector2 v) {
            validator.Invalidate();
            var i = normalizedVertices.Count;
            normalizedVertices.Add(v);
            return i;
        }
        public virtual void RemoveVertex(int i) {
            validator.Invalidate();
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

        public Layer LayerGetter {
            get {
                validator.CheckValidation();
                return layer;
            }
        }
        public virtual WhichSideEnum Side(Vector2 p) {
            validator.CheckValidation();
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
            return DistanceToEdge(layer.LayerToWorld.InverseTransformPoint(worldPos), out edge, out t);
        }
        
        protected virtual void GenerateLayerData() {
            layerVertices.Clear();
            layerEdges.Clear();

            var limit = normalizedVertices.Count;
            var local = layer.LocalToLayer;
            for (var i = 0; i < limit; i++) {
                var j = (i + 1) % limit;
                var v0 = (Vector2)local.TransformPoint(normalizedVertices[i]);
                var v1 = (Vector2)local.TransformPoint(normalizedVertices[j]);
                layerVertices.Add(v0);
                layerEdges.Add(new Edge2D(v0, v1));
            }

            OnGenerate.Invoke();
        }
    }
}
