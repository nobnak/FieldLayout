//#define NO_OPTIMIZATION

using Gist.Extensions.RectExt;
using nobnak.Gist;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Rectangle2D : AbstractField2D {
        public static readonly Vector2 DEFAULT_SIZE = Vector2.one;

        [SerializeField] protected Rect field = new Rect(-0.5f * DEFAULT_SIZE, DEFAULT_SIZE);

        [Header("Debug")]
        [SerializeField] protected bool debugEnabled = true;
        [SerializeField] protected Color debugColor = Color.white;

        protected Rect layerField;
        protected Vector2 layerFieldMin;
        protected Vector2 layerFieldSize;

        #region Unity
        protected override void OnEnable() {
            base.OnEnable();

            if (layer == null) {
                enabled = false;
                return;
            }

            validator.Reset();
            validator.Validation += () => {
                layer.LayerValidator.CheckValidation();
                Rebuild();
                transform.hasChanged = false;
            };
            validator.SetCheckers(() => !transform.hasChanged);

            layer.LayerValidator.Invalidated += () => validator.Invalidate();
        }
        void OnValidate() {
            validator.Invalidate();
        }
        void OnRenderObject() {
            if (!CanRender || !debugEnabled)
                return;

            var fig = GetGLFigure();
            fig.CurrentColor = debugColor;
            Draw(fig);
        }
        #endregion

        #region Base
        public override Rect LayerBounds {
            get { return layerField; }
        }
        public override void Draw(GLFigure fig) {
            var modelview = Camera.current.worldToCameraMatrix * layer.LayerToWorld;
            var shape = Matrix4x4.TRS(layerField.center, Quaternion.identity, layerField.size);
            fig.DrawQuad(modelview * shape);
        }
        #endregion

        public override WhichSideEnum Side(Vector2 p) {
            validator.CheckValidation();
            #if NO_OPTIMIZATION
            return layerField.Contains(p) ? WhichSideEnum.Inside : WhichSideEnum.Outside;
            #else
            var x = p.x - layerFieldMin.x;
            var y = p.y - layerFieldMin.y;
            return (0 <= x && x < layerFieldSize.x && 0 <= y && y < layerFieldSize.y)
                ? WhichSideEnum.Inside : WhichSideEnum.Outside;
            #endif
        }
        public override Vector2 ClosestPoint(Vector2 p) {
            validator.CheckValidation();
            return layerField.ClosestPoint(p);
        }

        protected void Rebuild() {
            UpdateLocalToLayer();

            layerFieldMin = LocalToLayer.TransformPoint(field.min);
            layerFieldSize = LocalToLayer.TransformVector(field.size);
            layerField = new Rect(layerFieldMin, layerFieldSize);

        }

        protected override void UpdateLocalToLayer() {
            var localMat = Matrix4x4.TRS(transform.localPosition, Quaternion.identity, transform.localScale);
            var localToLayer = layer.LocalToLayer;
            LocalToLayer.Reset(localToLayer * localMat);
        }
    }
}
