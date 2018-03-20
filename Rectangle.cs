using Gist.Extensions.RectExt;
using UnityEngine;
using nobnak.Gist.Intersection;
using nobnak.Gist.Exhibitor;
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

        protected OBB2 innerBounds = new OBB2();
        protected OBB2 outerBounds = new OBB2();

        #region Unity
        protected virtual void OnRenderObject() {
            if (!CanRender)
                return;

            var view = Camera.current.worldToCameraMatrix;
            var layerToWorld = layer.LayerToWorld.Matrix;

            var c = debugColor;
            gl.CurrentColor = c;
            gl.DrawQuad(view * layerToWorld * innerBounds.Model);

            c.a *= 0.2f;
            gl.CurrentColor = c;
            gl.DrawQuad(view * layerToWorld * outerBounds.Model);

            c.a *= 0.2f;
            gl.CurrentColor = c;
            gl.FillQuad(view * layerToWorld * innerBounds.Model);
        }
        protected virtual void OnDrawGizmos() {
#if UNITY_EDITOR
			if (!CanRender || !debugEnabled)
				return;

			var layerTopLeft = localToLayer.TransformPoint(new Vector2(-0.5f, 0.5f));
			var worldTopLeft = layer.LayerToWorld.TransformPoint(layerTopLeft);

			var style = new GUIStyle();
			style.normal.textColor = debugColor;
			Handles.Label(worldTopLeft, Title, style);
#endif
		}

		public string Title {
			get { return string.Format("{0}({1})", gameObject.name, tag); }
		}
		#endregion

		#region AbstractField
		public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            switch (side) {
                case SideEnum.Outside:
                    return outerBounds.ClosestPoint(layerPoint);
                default:
                    return innerBounds.ClosestPoint(layerPoint);
            }
        }

        public override ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            var contain = outerBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, layerPoint, BoundaryMode.Outer);
        }
        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            var contain = innerBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, layerPoint, BoundaryMode.Inner);
        }

        public override void Rebuild() {
            var localScale = transform.localScale;
            localScale.z = 1f;
            var localToLayerMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, localScale);
            localToLayer.Reset(layer.LocalToLayer.Matrix, localToLayerMatrix);

            var center = (Vector2)localToLayer.TransformPoint(Vector2.zero);
            var xaxis = (Vector2)localToLayer.TransformVector(Vector2.right);
            var yaxis = (Vector2)localToLayer.TransformVector(Vector2.up);
            var size = new Vector2(xaxis.magnitude, yaxis.magnitude);
            xaxis.Normalize();
            
            innerBounds.Reset(center, size, xaxis);

            var outerSize = size + 2f * borderThickness * Vector2.one;
            outerBounds.Reset(center, outerSize, xaxis);
        }
        #endregion

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
