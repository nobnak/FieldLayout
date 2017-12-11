using nobnak.Gist.Extensions.ComponentExt;
using System.Linq;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2DFacade : MonoBehaviour, IBoundary2D {

        protected int supportLayerMask;
        protected Polygon2D[] polygons;

        #region Unity
        void OnEnable() {
            polygons = transform.AggregateComponentsInChildren<Polygon2D>().ToArray();
            supportLayerMask = 0;
            foreach (var p in polygons)
                supportLayerMask |= p.SupportLayerMask;
        }
        #endregion

        #region IBoundary2D
        public int SupportLayerMask { get { return supportLayerMask; } }

        public WhichSideEnum Side(Vector2 p) {
            var result = WhichSideEnum.Outside;

            foreach (var poly in polygons) {
                result = poly.Side(p);
                if (result == WhichSideEnum.Inside)
                    return result;
            }
            return result;
        }
        public virtual Vector2 ClosestPoint(Vector2 point) {
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
