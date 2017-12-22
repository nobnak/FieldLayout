using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public abstract class AbstractField2D : MonoBehaviour {

        public abstract WhichSideEnum Side(Vector2 p);
        public abstract Vector2 ClosestPoint(Vector2 p);
        
        [SerializeField] protected Layer layer;

        protected Validator validator = new Validator();

        private GLFigure fig;

        public AbstractField2D() {
            LocalToLayer = new DefferedMatrix();
        }

        #region Unity
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() {
            if (fig != null) {
                fig.Dispose();
                fig = null;
            }
        }
        #endregion

        #region Message
        protected virtual void CrownLayer(Layer layer) {
            this.layer = layer;
            if (layer != null)
                enabled = true;
        }
        #endregion

        public abstract Rect LayerBounds { get; }
        public abstract void Draw(GLFigure fig);

        public virtual DefferedMatrix LocalToLayer { get; protected set; }
        public virtual bool CanRender {
            get {
                return this.IsActiveAndEnabledAlsoInEditMode()
                    && this.IsActiveLayer()
                    && validator.CheckValidation();
            }
        }

        protected GLFigure GetGLFigure() {
            if (fig == null)
                fig = new GLFigure();
            return fig;
        }
        protected virtual void UpdateLocalToLayer() {
            var localMat = transform.LocalToParent();
            var localToLayer = layer.LocalToLayer;
            LocalToLayer.Reset(localToLayer * localMat);
        }
    }
}
