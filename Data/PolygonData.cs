using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [CreateAssetMenu]
    public class PolygonData : ScriptableObject {
        public static readonly Vector2[] QUAD = new Vector2[] {
            new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f)
        };


        public List<Vector2> localVertices = new List<Vector2>();

        public PolygonData() {
            localVertices.Clear();
            localVertices.AddRange(QUAD);
        }
    }
}