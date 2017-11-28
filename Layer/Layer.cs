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

        public UnityEvent Changed;

        protected Validator validator = new Validator();
        protected Matrix4x4 layerMatrix;
        protected Matrix4x4 inverseLayerMatrix;
        protected Matrix4x4 localMatrix;
        protected Matrix4x4 inverseLocalMatrix;

        #region Unity
        protected virtual void OnEnable() {
            validator.Reset();
            validator.Validation += () => Validate();
            validator.SetExtraValidityChecker(() => !transform.hasChanged);
        }
        protected virtual void OnValidate() {
            Invalidate();
        }
        protected virtual void Update() {
            validator.CheckValidation();
        }
        protected virtual void OnDisable() {

        }
        #endregion
        
        #region ILayer
        public virtual Matrix4x4 LayerMatrix {
            get { return layerMatrix; }
        }
        public virtual Matrix4x4 InverseLayerMatrix {
            get { return inverseLayerMatrix; }
        }
        public virtual Matrix4x4 LocalMatrix {
            get { return localMatrix; }
        }
        public virtual Matrix4x4 InverseLocalMatrix {
            get { return inverseLocalMatrix; }
        }
        public virtual Vector3 LayerToLocal(Vector3 layerPosition) {
            return inverseLayerMatrix.MultiplyPoint3x4(layerPosition);
        }
        public virtual Vector3 LocalToLayer(Vector3 normalizedPosition) {
            return localMatrix.MultiplyPoint3x4(normalizedPosition);
        }
        public virtual Vector3 WorldToLayer(Vector3 worldPosition) {
            return inverseLayerMatrix.MultiplyPoint3x4(worldPosition);
        }
        public virtual Vector3 LayerToWorld(Vector3 localPosition) {
            return layerMatrix.MultiplyPoint3x4(localPosition);
        }
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
        #endregion
        
        protected virtual void Validate() {
            transform.hasChanged = false;
            GenerateLayerData();
        }
        protected virtual void Invalidate() {
            validator.Invalidate();
        }
        protected virtual void GenerateLayerData() {
            layerMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            inverseLayerMatrix = layerMatrix.inverse;

            localMatrix = Matrix4x4.Scale(transform.localScale);
            inverseLocalMatrix = localMatrix.inverse;

            Changed.Invoke();
        }
    }
}
