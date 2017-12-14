using nobnak.Gist.Extensions.ComponentExt;
using System.Linq;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2DFacade : AbstractBoundary2D {
        
        protected Polygon2D[] polygons;

        #region Unity
        protected override void OnEnable() {
            base.OnEnable();
            polygons = transform.AggregateComponentsInChildren<Polygon2D>().ToArray();
        }
        #endregion

        #region IBoundary2D
        public override WhichSideEnum Side(Vector2 p) {
            var result = WhichSideEnum.Outside;

            foreach (var poly in polygons) {
                result = poly.Side(p);
                if (result == WhichSideEnum.Inside)
                    return result;
            }
            return result;
        }
        public override Vector2 ClosestPoint(Vector2 point) {
            var result = default(Vector2);

            var minSqDist = float.MaxValue;
            foreach (var poly in polygons) {
                var pOnPolygon = poly.ClosestPoint(point);
                var sqDist = (pOnPolygon - point).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = pOnPolygon;
                }
            }
            return result;
        }
        #endregion
    }
}
