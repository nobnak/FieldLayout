using UnityEngine;

namespace Polyhedra2DZone {

    public interface IBoundary2D {

        int SupportLayerMask { get; }
        WhichSideEnum Side(Vector2 p, int layerMask = -1);
        Vector2 ClosestPoint(Vector2 point, int layerMask = -1);
    }
}