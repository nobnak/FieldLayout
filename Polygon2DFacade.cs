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

        public int SupportLayerMask { get { return supportLayerMask; } }
        public WhichSideEnum Side(Vector2 p, int layerMask = -1) {
            var result = WhichSideEnum.Outside;

            foreach (var poly in polygons) {
                if ((poly.SupportLayerMask & layerMask) == 0)
                    continue;

                result = poly.Side(p, layerMask);
                if (result == WhichSideEnum.Inside)
                    return result;
            }
            return result;
        }

        public virtual Vector2 ClosestPoint(Vector2 point, int layerMask = -1) {
            var result = default(Vector2);

            var minSqDist = float.MaxValue;
            foreach (var poly in polygons) {
                if ((poly.SupportLayerMask & layerMask) == 0)
                    continue;

                var pOnPolygon = poly.ClosestPoint(point, layerMask);
                var sqDist = (pOnPolygon - point).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = pOnPolygon;
                }
            }
            return result;
        }
    }
}
