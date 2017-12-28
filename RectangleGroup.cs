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

        [SerializeField] protected AbstractField[] fields;
        
        public override SideEnum Side(Vector2 layerPoint) {
            return ContainsInOuterBoundary(layerPoint)
                ? (ContainsInInnerBoundary(layerPoint) ? SideEnum.Inside : SideEnum.Border)
                : SideEnum.Outside;
        }

        public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            var minSqDist = float.MaxValue;
            var result = Vector2.zero;
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                var cp = f.ClosestPoint(layerPoint, side);
                var sqDist = (cp - layerPoint).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = cp;
                }
            }
            return result;
        }

        public override bool ContainsInOuterBoundary(Vector2 layerPoint) {
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                if (f.ContainsInOuterBoundary(layerPoint))
                    return true;
            }
            return false;
        }

        public override bool ContainsInInnerBoundary(Vector2 layerPoint) {
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                if (f.ContainsInInnerBoundary(layerPoint))
                    return true;
            }
            return false;
        }

        protected override void Rebuild() { }
    }
}
