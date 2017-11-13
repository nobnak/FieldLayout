using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [System.Serializable]
    public class Polygon2D {

        [SerializeField]
        protected List<Vector2> vertices = new List<Vector2>();

        #region List
        public int Count { get { return vertices.Count; } }
        public Vector2 this[int i] {
            get { return vertices[i]; }
            set { vertices[i] = value; }
        }
        public int Add(Vector2 v) {
            var i = vertices.Count;
            vertices.Add(v);
            return i;
        }
        public Vector2 Remove(int i) {
            var v = vertices[i];
            vertices.RemoveAt(i);
            return v;
        }
        #endregion

        public IEnumerable<Vector2> IterateVertices() {
            foreach (var v in vertices)
                yield return v;
        }
        public IEnumerable<Vector2> IterateEdges() {
            var count = vertices.Count;
            for (var i = 0; i < count; i++) {
                var j = (i + 1) % count;
                yield return vertices[i];
                yield return vertices[j];
            }
        }
    }
}
