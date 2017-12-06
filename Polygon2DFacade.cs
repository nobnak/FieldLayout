using Gist;
using Gist.Extensions.Behaviour;
using Gist.Intersection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Polygon2DFacade : MonoBehaviour {

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

        public virtual bool TryClosestPoint(Vector2 point, 
                out ILayer layer, out Vector2 closestPoint, int layerMask = -1) {
            var result = false;
            closestPoint = default(Vector2);
            layer = default(ILayer);

            var minSqDist = float.MaxValue;
            foreach (var poly in polygons) {
                if (((1 << poly.gameObject.layer) & layerMask) == 0)
                    continue;

                ILayer layerOfPolygon;
                Vector2 pOnPolygon;
                if (poly.TryClosestPoint(point, out layerOfPolygon, out pOnPolygon, layerMask)) {
                    var sqDist = (pOnPolygon - point).sqrMagnitude;
                    if (sqDist < minSqDist) {
                        result = true;
                        minSqDist = sqDist;
                        closestPoint = pOnPolygon;
                        layer = layerOfPolygon;
                    }
                }
            }

            return result;
        }
    }

    public static class ComponentExtension {
        public static IEnumerable<T> AggregateComponentsInChildren<T>(this Transform parent) 
            where T:Component {

            if (parent == null)
                yield break;

            for (var i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                var comp = child.GetComponent<T>();
                if (comp != null)
                    yield return comp;
                else
                    foreach (var c in child.AggregateComponentsInChildren<T>())
                        yield return c;
            }
        }
    }
}
