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
    public class RectangleGroup : Field, IGroup<Field>, IEnumerable<Field>, Field.IFieldListener {

        [SerializeField] protected List<Field> fields = new List<Field>();

        #region Unity
        protected override void OnEnable() {
            fields.RemoveAll(f => f == null);
        }
        #endregion

        #region IGroup
        public IList<Field> Elements {
            get { return fields; }
            set {
                fields.Clear();
                fields.AddRange(value);
            }
        }
        public void AddField(Field f) {
            fields.Add(f);
            InitField(f);
            f.CallbackChildren<Layer.ILayerListener>(r => r.TargetOnChange(layer));
        }
        public void RemvoeField(Field f) {
            fields.Remove(f);
        }
        #endregion

        #region IFieldListener
        public void TargetOnChange(Field target) {
            validator.Invalidate();
        }
        #endregion

        #region IEnumerable
        public IEnumerator<Field> GetEnumerator() {
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                yield return f;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            var minSqDist = float.MaxValue;
            var result = Vector2.zero;
            foreach (var f in this) {
                var cp = f.ClosestPoint(layerPoint, side);
                var sqDist = (cp - layerPoint).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = cp;
                }
            }
            return result;
        }

        public override ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            foreach (var f in this) {
                var contain = f.ContainsInOuterBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            foreach (var f in this) {
                var contain = f.ContainsInInnerBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override void Rebuild() {
            foreach (var f in this) {
                InitField(f);
            }
            Debug.LogFormat("RectangleGroup Rebuld()");
        }

        private void InitField(Field f) {
            f.BorderThickness = borderThickness;
        }
    }
}
