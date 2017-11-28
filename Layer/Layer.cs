using Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Layer : MonoBehaviour, ILayer {
        public enum WhichSideEnum { Unknown = 0, Inside, Outside }

        public const float EPSILON = 1e-3f;
        public const float CIRCLE_INV_DEG = 1f / 360;

        protected Validator validator = new Validator();

        public Layer() {
            LayerToWorld = new DefferedMatrix();
            LocalToLayer = new DefferedMatrix();
            LocalToWorld = new DefferedMatrix();
        }

        #region Unity
        protected virtual void OnEnable() {
            validator.Reset();
            validator.Validation += () => {
                transform.hasChanged = false;
                GenerateLayerData();
            };
            validator.SetCheckers(() => !transform.hasChanged);
        }
        protected virtual void OnValidate() {
            validator.Invalidate();
        }
        protected virtual void OnDisable() {

        }
        #endregion

        #region ILayer
        public DefferedMatrix LayerToWorld { get; protected set; }
        public DefferedMatrix LocalToLayer { get; protected set; }
        public DefferedMatrix LocalToWorld { get; protected set; }

        public virtual bool Raycast(Ray ray, out float distance) {
            distance = default(float);

            var n = transform.forward;
            var c = transform.position;
            var det = Vector3.Dot(n, ray.direction);
            if (-EPSILON < det && det < EPSILON)
                return false;

            distance = Vector3.Dot(n, c - ray.origin) / det;
            return true;
        }

        public virtual Validator ValidatorGetter { get { return validator; } }
        #endregion
        
        protected virtual void GenerateLayerData() {
            var layer = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            var local = Matrix4x4.Scale(transform.localScale);
            LayerToWorld.Reset(layer);
            LocalToLayer.Reset(local);
            LocalToWorld.Reset(layer, local);
        }
    }
}
