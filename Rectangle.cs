using UnityEngine;
using nobnak.Gist.Extensions.CameraExt;
using nobnak.Gist.GLTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nobnak.FieldLayout {

	[ExecuteInEditMode]
    public class Rectangle : Field {

        [Header("Debug")]
        [SerializeField]
        protected bool debugEnabled = true;
        [SerializeField]
        protected Color debugColor = Color.white;
        [SerializeField]
        protected bool debugFill = true;
        [SerializeField]
        [Range(0.01f, 0.1f)]
        protected float debugLineWidth = 0.1f;

        protected GLFigure gl;

        #region Unity
        protected override void OnEnable() {
            base.OnEnable();
            gl = new GLFigure();
            gl.DefaultLineMat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;
        }
        protected override void OnDisable() {
            base.OnDisable();
            if (gl != null) {
                gl.Dispose();
                gl = null;
            }
        }
        protected virtual void OnRenderObject() {
            if (!CanRender)
                return;

            var cam = Camera.current;
            var view = cam.worldToCameraMatrix;
            var layerToWorld = layer.LayerToWorld.Matrix;

            var worldCenter = layerToWorld.MultiplyPoint3x4(Vector3.zero);
            var width = Mathf.Min(
                debugLineWidth * cam.GetHandleSize(worldCenter),
                0.8f * borderThickness);

            var c = debugColor;
            gl.CurrentColor = c;
            gl.DrawQuad(view * layerToWorld * innerBounds.Model, width);

            gl.CurrentColor = 0.5f * c;
            gl.DrawQuad(view * layerToWorld * outerBounds.Model, width);

            if (debugFill) {
                gl.CurrentColor = 0.5f * c;
                gl.FillQuad(view * layerToWorld * innerBounds.Model);
            }
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
        #endregion

        #region interface
        #endregion

        #region member
        protected override bool CanRender {
            get {
                return gl != null && base.CanRender;
            }
        }
        #endregion
    }
}
