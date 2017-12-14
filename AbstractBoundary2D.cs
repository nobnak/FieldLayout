using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using UnityEngine;

namespace Polyhedra2DZone {
    [ExecuteInEditMode]
    public abstract class AbstractBoundary2D : MonoBehaviour {

        public abstract WhichSideEnum Side(Vector2 p);
        public abstract Vector2 ClosestPoint(Vector2 p);
        
        [SerializeField] protected Layer layer;

        protected Validator validator = new Validator();
        protected GLFigure fig;

        public DefferedMatrix LocalToLayer { get; protected set; }

        public AbstractBoundary2D() {
            LocalToLayer = new DefferedMatrix();
        }

        #region Unity
        protected virtual void OnEnable() {
            fig = new GLFigure();
        }
        #endregion

        #region Message
        protected virtual void CrownLayer(Layer layer) {
            this.layer = layer;
            if (layer != null)
                enabled = true;
        }
        #endregion

        public bool CanRender {
            get {
                return this.IsActiveAndEnabledAlsoInEditMode() && validator.CheckValidation();
            }
        }

        protected virtual void UpdateLocalToLayer() {
            var localMat = transform.LocalToParent();
            var localToLayer = layer.LocalToLayer;
            LocalToLayer.Reset(localToLayer * localMat);
        }
    }
}
