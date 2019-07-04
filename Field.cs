using nobnak.FieldLayout.Extensions;
using nobnak.Gist;
using nobnak.Gist.Exhibitor;
using nobnak.Gist.Extension.FloatArray;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Intersection;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace nobnak.FieldLayout {

	[ExecuteInEditMode]
    public class Field : MonoBehaviour, Layer.ILayerListener, IExhibitorListener {

        public static readonly Rect LOCAL_RECT = new Rect(-0.5f, -0.5f, 1f, 1f);

        [SerializeField] protected Layer layer;
        [Range(0f, 10f)]
        [SerializeField] protected float borderThickness = 0.1f;

        protected DefferedMatrix localToLayer = new DefferedMatrix();
        protected Validator validator = new Validator();
        protected OBB2 innerBounds = new OBB2();
        protected OBB2 outerBounds = new OBB2();

        #region Unity
        protected virtual void OnEnable() {
            validator.Validate();
        }
        public Field() { 
            validator.Reset();
            validator.Validation += () => {
                if (layer == null)
                    return;
                layer.LayerValidator.Validate();

                Rebuild();
                transform.hasChanged = false;
                this.CallbackSelf<IFieldListener>(a => a.TargetOnChange(this));
            };
            validator.SetCheckers(() => 
                layer != null 
                && layer.IsActiveAndEnabledAlsoInEditMode()
                && layer.LayerValidator.IsValid 
                && !transform.hasChanged);
        }
        protected virtual void OnDisable() {
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected virtual void Update() {
            validator.Validate();
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
        public virtual OBB2 Bounds(SideEnum side = SideEnum.Inside) {
            switch (side) {
                case SideEnum.Outside:
                    return outerBounds;
                default:
                    return innerBounds;
            }
        }
        public virtual Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            return Bounds(side).ClosestPoint(layerPoint);
        }
        public virtual ContainsResult Contains(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            var contain = Bounds(side).Contains(layerPoint);
            return new ContainsResult(this, contain, layerPoint, ToBoundaryMode(side));
        }
        public virtual ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            return Contains(layerPoint, SideEnum.Outside);
        }
        public virtual ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            return Contains(layerPoint, SideEnum.Inside);
        }
        public virtual void Rebuild() {
            var localScale = transform.localScale;
            localScale.z = 1f;
			transform.localScale = localScale;

			var localRotation = Quaternion.Inverse(layer.transform.rotation) * transform.rotation;
			var localEuler = localRotation.eulerAngles;
			localEuler.x = localEuler.y = 0f;
			localEuler = localEuler.Quantize(0.01f);
            transform.rotation = layer.transform.rotation * Quaternion.Euler(localEuler);

            var layerPos = layer.LayerToWorld.InverseTransformPoint(transform.position);
            layerPos.z = 0f;
            transform.position = layer.LayerToWorld.TransformPoint(layerPos).Quantize(1e-6f);

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
                    && validator.IsValid; ;
            }
        }
        protected BoundaryMode ToBoundaryMode(SideEnum side) {
            switch (side) {
                case SideEnum.Outside:
                    return BoundaryMode.Outer;
                case SideEnum.Inside:
                    return BoundaryMode.Inner;
                default:
                    return BoundaryMode.Unknown;
            }
        }
        #endregion

        #region classes
        public enum BoundaryMode { Unknown = 0, Inner, Outer }
        public struct ContainsResult {
            public readonly Field tip;
            public readonly BoundaryMode boundary;
            public readonly bool contain;
            public readonly Vector2 layerPoint;

            public ContainsResult(Field tip, bool contain, Vector2 layerPoint,
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

        public interface IFieldListener : IChangeListener<Field> {}
        #endregion
    }
}
