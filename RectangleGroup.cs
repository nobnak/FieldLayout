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
    public class RectangleGroup : AbstractField {

        [SerializeField] protected List<AbstractField> fields = new List<AbstractField>();

        #region Unity
        protected override void OnValidate() {
            base.OnValidate();
            validator.Invalidate();
        }
        #endregion

        #region Fields
        public List<AbstractField> Fields { get { return fields; } set { fields = value; } }
        public void AddField(AbstractField f) {
            fields.Add(f);
            f.NotifySelf<Layer.IMessageReceiver>(r => r.CrownLayer(layer));
        }
        public void RemvoeField(AbstractField f) {
            fields.Remove(f);
        }
        #endregion

        public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            var minSqDist = float.MaxValue;
            var result = Vector2.zero;
            foreach (var f in IterateAbstractFields()) {
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
            foreach (var f in IterateAbstractFields()) {
                var contain = f.ContainsInOuterBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            foreach (var f in IterateAbstractFields()) {
                var contain = f.ContainsInInnerBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override void Rebuild() {
            foreach (var f in IterateAbstractFields()) {
                f.BorderThickness = borderThickness;
            }
            Debug.LogFormat("RectangleGroup Rebuld()");
        }
        protected IEnumerable<AbstractField> IterateAbstractFields() {
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                yield return f;
            }
        }
    }
}
