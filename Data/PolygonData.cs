using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [CreateAssetMenu]
    public class PolygonData : ScriptableObject {

        public List<Vector2> normalizedVertices = new List<Vector2>();
    }
}