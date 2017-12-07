using UnityEngine;

namespace Polyhedra2DZone {

    public interface IDistance2D {
        bool TryClosestPoint(Vector2 point, out Vector2 closest, int layerMask = -1);
    }
}