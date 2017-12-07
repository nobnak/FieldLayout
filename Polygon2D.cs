using Gist;
using Gist.Extensions.Behaviour;
using Gist.Intersection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {
    [ExecuteInEditMode]
    public class Polygon2D : MonoBehaviour, IBoundary2D {

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        public UnityEvent OnGenerate;

        public Layer layer;
        [SerializeField] protected PolygonData data;

        protected Validator validator = new Validator();
        protected List<Vector2> layerVertices = new List<Vector2>();
        protected List<Edge2D> layerEdges = new List<Edge2D>();
        protected AABB2 layerBounds = new AABB2();

        #region Unity
        protected virtual void OnEnable() {
            if (data == null) {
                Debug.LogFormat("PolygonData not found");
                enabled = false;
                return;
            }

            validator.Reset();
            validator.Validation += () => {
                layer.LayerValidator.CheckValidation();
                GenerateLayerData();
            };
            layer.LayerValidator.Invalidated += () => validator.Invalidate();
            validator.SetCheckers(() => layer != null && layer.LayerValidator.IsValid);
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected virtual void OnDisable() {
        }
        #endregion

        #region Vertex
        public virtual int VertexCount {
            get { return data.normalizedVertices.Count; }
        }
        public virtual Vector2 GetVertex(int i) {
            return data.normalizedVertices[i];
        }
        public virtual void SetVertex(int i, Vector2 value) {
            validator.Invalidate();
            data.normalizedVertices[i] = value;
        }
        public virtual int AddVertex(Vector2 v) {
            validator.Invalidate();
            var i = data.normalizedVertices.Count;
            data.normalizedVertices.Add(v);
            return i;
        }
        public virtual void RemoveVertex(int i) {
            validator.Invalidate();
            data.normalizedVertices.RemoveAt(i);
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
        public Rect LayerBounds { get { return layerBounds; } }
        public virtual int ClosestVertexIndex(Vector2 p, int layerMask = -1) {
            validator.CheckValidation();
            var index = -1;

            var minSqDist = float.MaxValue;
            for (var i = 0; i < layerVertices.Count; i++) {
                var v = layerVertices[i];
                var sqDist = (v - p).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    index = i;
                }
            }
            return index;
        }

        public virtual int SupportLayerMask {
            get { return 1 << gameObject.layer; }
        }
        public virtual WhichSideEnum Side(Vector2 p, int layerMask = -1) {
            validator.CheckValidation();
            var totalAngle = 0f;
            foreach (var e in IterateEdges())
                totalAngle += e.Angle(p);
            return (Mathf.RoundToInt(totalAngle * CIRCLE_INV_DEG) != 0)
                ? WhichSideEnum.Inside : WhichSideEnum.Outside;
        }
        public virtual Vector2 ClosestPoint(Vector2 point, int layerMask = -1) { 
            validator.CheckValidation();
            
            var result = default(Vector2);
            if ((SupportLayerMask & layerMask) == 0)
                return result;

            var minSqDist = float.MaxValue;
            foreach (var e in layerEdges) {
                var v = e.ClosestPoint(point);
                var sqDist = (v - point).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = v;
                }
            }
            return result;
        }
        
        protected virtual void GenerateLayerData() {
            layerVertices.Clear();
            layerEdges.Clear();
            layerBounds.Clear();

            var limit = data.normalizedVertices.Count;
            var local = layer.LocalToLayer;
            for (var i = 0; i < limit; i++) {
                var j = (i + 1) % limit;
                var v0 = (Vector2)local.TransformPoint(data.normalizedVertices[i]);
                var v1 = (Vector2)local.TransformPoint(data.normalizedVertices[j]);
                layerVertices.Add(v0);
                layerEdges.Add(new Edge2D(v0, v1));
                layerBounds.Encapsulate(v0);
            }

            OnGenerate.Invoke();
        }
    }
}
