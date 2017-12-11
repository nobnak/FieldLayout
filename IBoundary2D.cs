using UnityEngine;

namespace Polyhedra2DZone {

    public interface IBoundary2D {

        int SupportLayerMask { get; }

        WhichSideEnum Side(Vector2 p);
        Vector2 ClosestPoint(Vector2 p);
    }
}
