using Gist.Extensions.RectExt;
using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public abstract class AbstractField : MonoBehaviour, Layer.IMessageReceiver {

        [SerializeField] protected Layer layer;
        [Range(0f, 10f)]
        [SerializeField] protected float borderThickness = 0.1f;

        protected DefferedMatrix localToLayer = new DefferedMatrix();
        protected Validator validator = new Validator();
        protected GLFigure gl;

        #region Unity
        protected virtual void OnEnable() {
            if (layer == null) {
                enabled = false;
                return;
            }

            gl = new GLFigure();

            validator.Reset();
            validator.Validation += () => {
                layer.LayerValidator.CheckValidation();
                Rebuild();
                transform.hasChanged = false;
            };
            validator.SetCheckers(() => layer.LayerValidator.IsValid && !transform.hasChanged);
            layer.LayerValidator.Invalidated += () => validator.Invalidate();
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected virtual void OnDisable() {
            gl.Dispose();
        }
        protected virtual void Update() {
            validator.CheckValidation();
        }
        #endregion

        #region Message
        public virtual void CrownLayer(Layer layer) {
            this.layer = layer;
            if (layer != null)
                enabled = true;
        }
        #endregion

        public Layer Layer { get { return layer; } }
        public DefferedMatrix LocalToLayer { get { return localToLayer; } }
        public float BorderThickness {
            get { return borderThickness; }
            set {
                validator.Invalidate();
                Debug.Log("Invalidate() at AbstractField");
                borderThickness = Mathf.Max(0f, value);
            }
        }

        public abstract Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside);
        public abstract ContainsResult ContainsInOuterBoundary(Vector2 layerPoint);
        public abstract ContainsResult ContainsInInnerBoundary(Vector2 layerPoint);

        public abstract void Rebuild();

        public virtual SideEnum Side(Vector2 layerPoint) {
            if (ContainsInOuterBoundary(layerPoint).contain) {
                if (ContainsInInnerBoundary(layerPoint).contain)
                    return SideEnum.Inside;
                else
                    return SideEnum.Border;
            }
            return SideEnum.Outside;
        }

        protected virtual bool CanRender {
            get {
                return layer != null 
                    && this.IsActiveAndEnabledAlsoInEditMode()
                    && this.IsActiveLayer()
                    && validator.CheckValidation();
            }
        }

        public enum BoundaryMode { Unknown = 0,Inner, Outer }
        public struct ContainsResult {
            public readonly AbstractField tip;
            public readonly BoundaryMode boundary;
            public readonly bool contain;

            public ContainsResult(AbstractField tip, bool contain, BoundaryMode boundary = BoundaryMode.Unknown) {
                this.tip = tip;
                this.boundary = boundary;
                this.contain = contain;
            }

            public static implicit operator bool(ContainsResult cres) {
                return cres.contain;
            }
        }
    }
}
