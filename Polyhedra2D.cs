using UnityEngine;
using System.Collections.Generic;

namespace Polyhedra2DZone {
    public class Polyhedra2D {
        public const float EPSILON = 1e-6f;

        bool _valid;
        Vector2[] _vertices;
        Vector2[] _edges;

        public Polyhedra2D() {
        }
        public Polyhedra2D(Vector2[] vertices) {
            SetVertices (vertices);
        }

        public Polyhedra2D SetVertices(Vector2[] vertices) {
            _vertices = vertices;
            CheckInit ();
            return this;
        }
        public IEnumerable<Vector2> EdgePairs() {
            CheckInit ();
            
            for (var i = 0; i < _vertices.Length; i++) {
                yield return _vertices [i];
                yield return _vertices [Repeat (i + 1)];
            }
        }
        public IEnumerable<Vector2> Edges() {
            CheckInit ();
            
            foreach (var e in _edges)
                yield return e;
        }

        public bool Inside(Vector2 p) {
            CheckInit ();

            var total = 0;
            for (var i = 0; i < _vertices.Length; i++) {
                var v = _vertices [i];
                var w = _vertices [Repeat (i + 1)];
               
                var c = (v.y <= p.y && p.y < w.y) ? 1 : ((w.y <= p.y && p.y < v.y) ? -1 : 0);
                if (c != 0 && p.x < EdgeIntersectionPointOnX(p.y, i))
                    total += c;
            }
            return total < 0;
        }
        public Vector2 ClosestPoint(Vector2 p) {
            CheckInit ();

            Vector2 closestPoint = Vector2.zero;
            float sqrClosestDistance = float.MaxValue;
            for (var i = 0; i < _edges.Length; i++) {
                var v = _vertices [i];
                var e = _edges [i];
                var sqrlen = e.sqrMagnitude;
                if (sqrlen < EPSILON)
                    continue;
                var t = Vector2.Dot ((p - v), e) / sqrlen;
                if (t < 0f) {
                    var sqr = (v - p).sqrMagnitude;
                    if (sqr < sqrClosestDistance) {
                        sqrClosestDistance = sqr;
                        closestPoint = v;
                    }
                } else if (t <= 1f) {
                    var cp = v + t * e;
                    var sqr = (cp - p).sqrMagnitude;
                    if (sqr < sqrClosestDistance) {
                        sqrClosestDistance = sqr;
                        closestPoint = cp;
                    }
                } else {
                    var w = _vertices [Repeat (i + 1)];
                    var sqr = (w - p).sqrMagnitude;
                    if (sqr < sqrClosestDistance) {
                        sqrClosestDistance = sqr;
                        closestPoint = w;
                    }
                }
            }
            return closestPoint;
        }

        public Vector2 Edge(int i) { return _edges [i]; }
        public Vector2 Vertex(int i) { return _vertices[i]; }
        public int Count { get { return _vertices.Length; } }
        public bool Valid { get { return _valid; } }


        float EdgeCoordinateOnY(float y, int edgeIndex) {
            var v = _vertices [edgeIndex].y;
            var w = _vertices [Repeat (edgeIndex + 1)].y;
            return (y - v) / (w - v);
        }
        float EdgeIntersectionPointOnX(float y, int edgeIndex) {
            var t = EdgeCoordinateOnY (y, edgeIndex);
            return _vertices [edgeIndex].x + t * _edges [edgeIndex].x;
        }
        void CheckInit() {
            if (!_valid) {
                _valid = true;
                GenerateEdges ();
            }
        }
        void GenerateEdges() {
            if (_edges == null || _edges.Length != _vertices.Length)
                System.Array.Resize (ref _edges, _vertices.Length);
            for (var i = 0; i < _edges.Length; i++)
                _edges [i] = _vertices [Repeat (i + 1)] - _vertices [i];
        }
        int Repeat(int i) {
            return i % _vertices.Length;
        }

    }
}
