using nobnak.FieldLayout.Extension;
using nobnak.Gist;
using nobnak.Gist.Exhibitor;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Intersection;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using UnityEngine;
using nobnak.Gist.Extensions.CameraExt;
using nobnak.Gist.Extension.FloatArray;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public class Rectangle : MonoBehaviour, Layer.ILayerListener, IExhibitorListener {

        public static readonly Rect LOCAL_RECT = new Rect(-0.5f, -0.5f, 1f, 1f);

        [SerializeField] protected Layer layer;
        [Range(0f, 10f)]
        [SerializeField] protected float borderThickness = 0.1f;

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

        protected DefferedMatrix localToLayer = new DefferedMatrix();
        protected Validator validator = new Validator();
        protected OBB2 innerBounds = new OBB2();
        protected OBB2 outerBounds = new OBB2();
        protected GLFigure gl;

        #region Unity
        protected virtual void OnEnable() {
            gl = new GLFigure();

            validator.Reset();
            validator.Validation += () => {
                if (layer == null)
                    return;
                layer.LayerValidator.Validate();

                Rebuild();
                transform.hasChanged = false;
                this.CallbackSelf<IAbstractFieldListener>(a => a.TargetOnChange(this));
            };
            validator.SetCheckers(() => 
                layer != null 
                && layer.IsActiveAndEnabledAlsoInEditMode()
                && layer.LayerValidator.IsValid 
                && !transform.hasChanged);
        }
        protected virtual void OnDisable() {
            if (gl != null) {
                gl.Dispose();
                gl = null;
            }
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected virtual void Update() {
            validator.Validate();
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

        #region Message
        public virtual void TargetOnChange(Layer layer) {
            this.layer = layer;
            validator.Invalidate();
        }
        #endregion

        #region interface
        #region IExhibitorListener
        public virtual void ExhibitorOnParent(Transform parent) {
            parent.Add(this);
        }
        public virtual void ExhibitorOnUnparent(Transform parent) {
            parent.Remove(this);
        }
        #endregion

        public string Title {
            get { return string.Format("{0}", gameObject.name); }
        }
        public Layer Layer { get { return layer; } }
        public DefferedMatrix LocalToLayer { get { return localToLayer; } }
        public float BorderThickness {
            get { return borderThickness; }
            set {
                validator.Invalidate();
                borderThickness = Mathf.Max(0f, value);
            }
        }
        public virtual Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            switch (side) {
                case SideEnum.Outside:
                    return outerBounds.ClosestPoint(layerPoint);
                default:
                    return innerBounds.ClosestPoint(layerPoint);
            }
        }
        public virtual ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            var contain = outerBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, layerPoint, BoundaryMode.Outer);
        }
        public virtual ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            var contain = innerBounds.Contains(layerPoint);
            return new ContainsResult(this, contain, layerPoint, BoundaryMode.Inner);
        }
        public virtual void Rebuild() {
            var localScale = transform.localScale;
            localScale.z = 1f;

            transform.rotation = layer.transform.rotation;
            var layerPos = layer.LayerToWorld.InverseTransformPoint(transform.position);
            layerPos.z = 0f;
            transform.position = layer.LayerToWorld.TransformPoint(layerPos).RoundBelowZero();

            localToLayer.Reset(layer.LayerToWorld.Inverse, transform.localToWorldMatrix);

            var center = (Vector2)localToLayer.TransformPoint(Vector2.zero);
            var xaxis = (Vector2)localToLayer.TransformVector(Vector2.right);
            var yaxis = (Vector2)localToLayer.TransformVector(Vector2.up);
            var size = new Vector2(xaxis.magnitude, yaxis.magnitude);
            xaxis.Normalize();

            innerBounds.Reset(center, size, xaxis);

            var outerSize = size + 2f * borderThickness * Vector2.one;
            outerBounds.Reset(center, outerSize, xaxis);
        }
        public virtual SideEnum Side(Vector2 layerPoint) {
            if (ContainsInOuterBoundary(layerPoint).contain) {
                if (ContainsInInnerBoundary(layerPoint).contain)
                    return SideEnum.Inside;
                else
                    return SideEnum.Border;
            }
            return SideEnum.Outside;
        }
        #endregion

        #region member
        protected virtual bool CanRender {
            get {
                return layer != null 
                    && this.IsActiveAndEnabledAlsoInEditMode()
                    && this.IsVisibleLayer()
                    && validator.Validate();
            }
        }
        #endregion

        #region classes
        public enum BoundaryMode { Unknown = 0, Inner, Outer }
        public struct ContainsResult {
            public readonly Rectangle tip;
            public readonly BoundaryMode boundary;
            public readonly bool contain;
            public readonly Vector2 layerPoint;

            public ContainsResult(Rectangle tip, bool contain, Vector2 layerPoint,
                BoundaryMode boundary = BoundaryMode.Unknown) {

                this.tip = tip;
                this.boundary = boundary;
                this.contain = contain;
                this.layerPoint = layerPoint;
            }

            public static implicit operator bool(ContainsResult cres) {
                return cres.contain;
            }

            public Vector2 LocalPosition {
                get { return tip.localToLayer.InverseTransformPoint(layerPoint); }
            }
            public Vector3 WorldPosition {
                get { return tip.layer.LayerToWorld.TransformPoint(layerPoint); }
            }

            public override string ToString() {
                return string.Format("<AbstractField.ContainsResult : {0} local={1} layer={2}>",
                    tip.name, LocalPosition, layerPoint);
            }
        }

        public interface IAbstractFieldListener : IChangeListener<Rectangle> {}
        #endregion
    }
}
