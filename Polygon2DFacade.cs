using Gist;
using Gist.Extensions.Behaviour;
using Gist.Extensions.ComponentExt;
using Gist.Intersection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2DFacade : MonoBehaviour, IDistance2D {

        protected Polygon2D[] polygons;

        #region Unity
        void OnEnable() {
            polygons = transform.AggregateComponentsInChildren<Polygon2D>().ToArray();
        }
        #endregion

#if false
        public virtual Polygon2D.WhichSideEnum Side(Vector2 p, int layerMask) {

        }
#endif

        public virtual bool TryClosestPoint(Vector2 point, out Vector2 closest, int layerMask = -1) {
            var result = false;
            closest = default(Vector2);

            var minSqDist = float.MaxValue;
            foreach (var poly in polygons) {
                if (((1 << poly.gameObject.layer) & layerMask) == 0)
                    continue;

                Vector2 pOnPolygon;
                if (poly.TryClosestPoint(point, out pOnPolygon, layerMask)) {
                    var sqDist = (pOnPolygon - point).sqrMagnitude;
                    if (sqDist < minSqDist) {
                        minSqDist = sqDist;
                        closest = pOnPolygon;
                    }
                }
            }
            return result;
        }
    }
}
