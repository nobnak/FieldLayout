using Gist.Extensions.RectExt;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public class Rectangle : AbstractField {

        [SerializeField] protected Rect localSize = new Rect(-0.5f, -0.5f, 1f, 1f);

        [Header("Debug")]
        [SerializeField]
        protected bool debugEnabled = true;
        [SerializeField]
        protected Color debugColor = Color.white;

        protected Rect layerInside;
        protected Vector2 layerInsideMin;
        protected Vector2 layerInsideMax;
        protected Rect layerOutside;
        protected Vector2 layerOutsideMin;
        protected Vector2 layerOutsideMax;

        #region Unity
        protected virtual void OnRenderObject() {
            if (!CanRender)
                return;

            var view = Camera.current.worldToCameraMatrix * layer.LayerToWorld.Matrix;

            gl.CurrentColor = debugColor;
            gl.DrawQuad(view * Matrix4x4.TRS(
                layerInside.center, Quaternion.identity, layerInside.size));

            gl.CurrentColor *= 0.5f;
            gl.DrawQuad(view * Matrix4x4.TRS(
                layerOutside.center, Quaternion.identity, layerOutside.size));

            gl.CurrentColor *= 0.5f;
            gl.FillQuad(view * Matrix4x4.TRS(
                layerInside.center, Quaternion.identity, layerInside.size));
        }
        protected virtual void OnDrawGizmos() {
            #if UNITY_EDITOR
                if (!CanRender || !debugEnabled)
                    return;

                var layerTopLeft = new Vector2(layerInsideMin.x, layerInsideMax.y);
                var worldTopLeft = layer.LayerToWorld.TransformPoint(layerTopLeft);

                var style = new GUIStyle();
                style.normal.textColor = debugColor;
                Handles.Label(worldTopLeft, string.Format("{0}({1})", gameObject.name, tag), style);
            #endif
        }
        #endregion
        
        public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            switch (side) {
                case SideEnum.Outside:
                    return layerOutside.ClosestPoint(layerPoint);
                default:
                    return layerInside.ClosestPoint(layerPoint);
            }
        }

        public override ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            var contain = Contains(layerOutsideMin, layerOutsideMax, layerPoint);
            return new ContainsResult(this, contain, BoundaryMode.Outer);
        }
        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            var contain = Contains(layerInsideMin, layerInsideMax, layerPoint);
            return new ContainsResult(this, contain, BoundaryMode.Inner);
        }

        public override void Rebuild() {
            localToLayer.Reset(layer.LocalToLayer.Matrix,
                Matrix4x4.TRS(transform.localPosition, Quaternion.identity, transform.localScale));

            layerInsideMin = localToLayer.TransformPoint(localSize.min);
            layerInsideMax = localToLayer.TransformPoint(localSize.max);
            layerInside = Rect.MinMaxRect(layerInsideMin.x, layerInsideMin.y, 
                layerInsideMax.x, layerInsideMax.y);

            layerOutsideMin = layerInsideMin - borderThickness * Vector2.one;
            layerOutsideMax = layerInsideMax + borderThickness * Vector2.one;
            layerOutside = Rect.MinMaxRect(layerOutsideMin.x, layerOutsideMin.y,
                layerOutsideMax.x, layerOutsideMax.y);
        }

        public static bool Contains(Vector2 min, Vector2 max, Vector2 point) {
            var minx = min.x;
            var miny = min.y;
            var maxx = max.x;
            var maxy = max.y;
            var px = point.x;
            var py = point.y;

            return minx <= px && px < maxx && miny <= py && py < maxy;
        }
    }
}
