using Gist.Extensions.RectExt;
using UnityEngine;
using nobnak.Gist.Intersection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public class Rectangle : AbstractField {

        public static readonly Rect LOCAL_RECT = new Rect(-0.5f, -0.5f, 1f, 1f);

        [Header("Debug")]
        [SerializeField]
        protected bool debugEnabled = true;
        [SerializeField]
        protected Color debugColor = Color.white;

        protected OBB2 insideBounds = new OBB2();
        protected OBB2 outsideBounds = new OBB2();

        #region Unity
        protected virtual void OnRenderObject() {
            if (!CanRender)
                return;

            var view = Camera.current.worldToCameraMatrix;
            var model = layer.LayerToWorld.Matrix * localToLayer.Matrix;

            var c = debugColor;
            gl.CurrentColor = c;
            gl.DrawQuad(view * model);

            c.a *= 0.2f;
            gl.CurrentColor = c;
            gl.DrawQuad(view * model);

            c.a *= 0.2f;
            gl.CurrentColor = c;
            gl.FillQuad(view * model);
        }
        protected virtual void OnDrawGizmos() {
            #if UNITY_EDITOR
                if (!CanRender || !debugEnabled)
                    return;

                var layerTopLeft = localToLayer.TransformPoint(new Vector2(-0.5f, 0.5f));
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
                    return outsideBounds.ClosestPoint(layerPoint);
                default:
                    return insideBounds.ClosestPoint(layerPoint);
            }
        }

        public override ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            var contain = outsideBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, BoundaryMode.Outer);
        }
        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            var contain = insideBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, BoundaryMode.Inner);
        }

        public override void Rebuild() {
            localToLayer.Reset(layer.LocalToLayer.Matrix,
                Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale));

            var center = (Vector2)localToLayer.TransformPoint(Vector2.zero);
            var size = (Vector2)localToLayer.TransformVector(Vector2.one);
            var xaxis = (Vector2)localToLayer.TransformVector(Vector2.right);
            insideBounds.Reset(center, size, xaxis);

            var outerSize = size + 2f * borderThickness * Vector2.one;
            outsideBounds.Reset(center, outerSize, xaxis);
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
