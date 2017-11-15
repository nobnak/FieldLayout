using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    public struct Edge2D {

        public readonly Vector2 v0;
        public readonly Vector2 v1;

        public bool cached;
        public float length;
        public Vector2 reciproTangent;
        public Vector2 normal;

        public Edge2D(Vector2 v0, Vector2 v1) {
            this.v0 = v0;
            this.v1 = v1;

            this.cached = false;
            this.length = 0f;
            this.reciproTangent = default(Vector2);
            this.normal = default(Vector2);
        }

        public float Distance(Vector2 p, out float t, out float n) {
            CheckCache();

            var vp = p - v0;
            t = Mathf.Clamp01(Vector2.Dot(vp, reciproTangent));
            n = Vector2.Dot(vp, normal);
            return (p - GetPosition(t)).magnitude;
        }

        public void CheckCache() {
            if (cached)
                return;
            cached = true;

            var v01 = v1 - v0;
            length = v01.magnitude;

            var tangent = v01 / length;
            reciproTangent = (1f / (length * length)) * v01;
            normal = new Vector2(-tangent.y, tangent.x);
        }

        public Vector2 GetPosition(float t) {
            return t * v1 + (1f - t) * v0;
        }
    }
}
