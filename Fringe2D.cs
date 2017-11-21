using Gist;
using Gist.Extensions.AABB;
using Gist.Extensions.Behaviour;
using Gist.Intersection;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [System.Serializable]
    public class Fringe2D {
        [SerializeField] protected float fringeExtent = 0f;

        [Header("Debug")]
        [SerializeField] protected Color debugBoundaryColor = Color.red;

        protected Polygon2D polygon;
        protected List<OBB2> boundaries;

        public void Init(Polygon2D polygon) {
            this.polygon = polygon;
            
            boundaries = new List<OBB2>();
        }

        public void OnRenderObject(GLFigure fig) {
            if (polygon == null || !polygon.IsActiveAndEnabledAlsoInEditMode())
                return;

            var modelview = Camera.current.worldToCameraMatrix * polygon.ModelMatrix;
            foreach (var obb in boundaries) {
                var m = modelview * obb.Model;
                var halfColor = 0.5f * debugBoundaryColor;
                fig.FillQuad(m, halfColor);
                fig.DrawQuad(m, debugBoundaryColor);

                var aabb = obb.WorldBounds;
                var aabbModel = Matrix4x4.TRS(aabb.center, Quaternion.identity, aabb.size);
                fig.DrawQuad(modelview * aabbModel, 0.5f * halfColor);
            }
        }
        
        public Rect Bounds {
            get {
                var aabb = new AABB2();
                var quad = new Rect(-0.5f * Vector2.one, Vector2.one);
                foreach (var b in boundaries) {
                    var bb = quad.EncapsulateInTargetSpace(b.Model);

                    aabb.Encapsulate(bb);
                }
                return aabb;
            }
        }
        public bool Overlaps(AABB2 aabb) {
            foreach (var obb in boundaries)
                if (aabb.Intersect(obb))
                    return true;
            return false;
        }
        public bool Overlaps(Rect r) {
            return Overlaps((AABB2)r);
        }

        public void Generate() {
            boundaries.Clear();
            foreach (var e in polygon.IterateEdges()) {
                var obb = GenerateConvex(e, fringeExtent);
                boundaries.Add(obb);
            }
        }
        protected OBB2 GenerateConvex(Edge2D edge, float extent) {
            var v01 = edge.v1 - edge.v0;
            var xaxis = v01.normalized;
            var len = v01.magnitude;
            var size = new Vector2(len + 2f * extent, 2f * extent);
            var center = edge.GetPosition(0.5f);
            return new OBB2(center, size, xaxis);
        }
    }
}