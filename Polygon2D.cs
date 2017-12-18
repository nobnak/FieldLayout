//#define WINDING_NUMBER_ALGOTIRHM

using nobnak.Gist;
using nobnak.Gist.Intersection;
using nobnak.Gist.Layer2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {
    [ExecuteInEditMode]
    public class Polygon2D : AbstractField2D {

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        public UnityEvent OnGenerate;
        
        [SerializeField] protected PolygonData data;

        protected List<Vector2> layerVertices = new List<Vector2>();
        protected List<Edge2D> layerEdges = new List<Edge2D>();
        protected AABB2 layerBounds = new AABB2();

        #region Unity
        protected override void OnEnable() {
            base.OnEnable();

            if (data == null) {
                Debug.LogFormat("PolygonData not found");
                enabled = false;
                return;
            }
            if (layer == null) {
                enabled = false;
                return;
            }

            validator.Reset();
            validator.Validation += () => {
                layer.LayerValidator.CheckValidation();
                GenerateLayerData();
                transform.hasChanged = false;
            };
            layer.LayerValidator.Invalidated += () => validator.Invalidate();
            validator.SetCheckers(() => layer != null 
                && layer.LayerValidator.IsValid 
                && !transform.hasChanged);
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected override void OnDisable() {
            base.OnDisable();
        }
        #endregion

        #region Vertex
        public virtual int VertexCount {
            get { return data.localVertices.Count; }
        }
        public virtual Vector2 GetVertex(int i) {
            return data.localVertices[i];
        }
        public virtual void SetVertex(int i, Vector2 value) {
            validator.Invalidate();
            data.localVertices[i] = value;
        }
        public virtual int AddVertex(Vector2 v) {
            validator.Invalidate();
            var i = data.localVertices.Count;
            data.localVertices.Add(v);
            return i;
        }
        public virtual void RemoveVertex(int i) {
            validator.Invalidate();
            data.localVertices.RemoveAt(i);
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
        public virtual IEnumerable<Vector2> IterateEdgeVertices() {
            validator.CheckValidation();
            foreach (var e in layerEdges) {
                yield return e.v0;
                yield return e.v1;
            }
        }
        #endregion

        #region Base
        public override Rect LayerBounds { get { return layerBounds; } }
        public override void Draw(GLFigure fig) {
            validator.CheckValidation();
            
            var modelView = Camera.current.worldToCameraMatrix * layer.LayerToWorld;
            fig.DrawLines(IterateEdgeVertices().Select(v=>(Vector3)v), modelView);
        }
        #endregion

        public Layer LayerGetter {
            get {
                validator.CheckValidation();
                return layer;
            }
        }
        public virtual int ClosestVertexIndex(Vector2 p) {
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

        #region AbstractBoundary2D
        public override WhichSideEnum Side(Vector2 p) {
            validator.CheckValidation();

            #if WINDING_NUMBER_ALGOTIRHM
            var totalAngle = 0f;
            foreach (var e in IterateEdges())
                totalAngle += e.Angle(p);
            return (Mathf.RoundToInt(totalAngle * CIRCLE_INV_DEG) != 0)
                ? WhichSideEnum.Inside : WhichSideEnum.Outside;
            
            #else

            var c = 0;
            foreach (var e in IterateEdges()) {
                var v0 = e.v0;
                var v1 = e.v1;

                var x0 = v0.x;
                var y0 = v0.y;
                var x1 = v1.x;
                var y1 = v1.y;
                var xp = p.x;
                var yp = p.y;

                if ((y0 <= yp && yp < y1) || (y1 <= yp && yp < y0)) {
                    var t = (yp - y0) / (y1 - y0);
                    var xt = x0 + t * (x1 - x0);
                    if (xt <= xp)
                        c++;
                }
            }
            return ((c % 2) == 0 ? WhichSideEnum.Outside : WhichSideEnum.Inside);
            #endif
        }
        public override Vector2 ClosestPoint(Vector2 p) { 
            validator.CheckValidation();
            
            var result = default(Vector2);
            var minSqDist = float.MaxValue;
            foreach (var e in layerEdges) {
                var v = e.ClosestPoint(p);
                var sqDist = (v - p).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = v;
                }
            }
            return result;
        }
        #endregion
        
        protected virtual void GenerateLayerData() {
            layerVertices.Clear();
            layerEdges.Clear();
            layerBounds.Clear();

            var localMat = transform.LocalToParent();
            var localToLayer = layer.LocalToLayer;
            LocalToLayer.Reset(localToLayer * localMat);

            var limit = data.localVertices.Count;
            for (var i = 0; i < limit; i++) {
                var j = (i + 1) % limit;
                var vl0 = (Vector2)LocalToLayer.TransformPoint(
                        data.localVertices[i]);
                var vl1 = (Vector2)LocalToLayer.TransformPoint(
                        data.localVertices[j]);
                layerVertices.Add(vl0);
                layerEdges.Add(new Edge2D(vl0, vl1));
                layerBounds.Encapsulate(vl0);
            }

            OnGenerate.Invoke();
        }
    }
}
