using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    public class Edge2D {

        public readonly Vector2 v0;
        public readonly Vector2 v1;

        protected bool cached;
        protected float length;
        protected Vector2 reciproTangent;

        public Edge2D(Vector2 v0, Vector2 v1) {
            this.v0 = v0;
            this.v1 = v1;
        }
        
        public float TOfClosestPoint(Vector2 p) {
            CheckCache();

            var vp = p - v0;
            return Mathf.Clamp01(Vector2.Dot(vp, reciproTangent));
        }
        public Vector2 ClosestPoint(Vector2 p) {
            var t = TOfClosestPoint(p);
            return GetPosition(t);
        }

        public Vector2 GetPosition(float t) {
            return t * v1 + (1f - t) * v0;
        }
        public float Angle(Vector2 p) {
            return Vector2.SignedAngle(v0 - p, v1 - p);
        }

        protected void CheckCache() {
            if (cached)
                return;
            cached = true;

            var v01 = v1 - v0;
            length = v01.magnitude;
            reciproTangent = (1f / (length * length)) * v01;
        }
    }
}
