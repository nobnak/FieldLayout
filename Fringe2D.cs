using Gist;
using Gist.BoundingVolume;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    public class Fringe2D : MonoBehaviour {

        [SerializeField] protected float fringeExtent = 0f;
        [SerializeField] protected Polygon2D polygon;

        protected Validator validator;

        #region Unity
        void OnEnable() {
            validator = new Validator();

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();

            validator.Validation += Validator_Validation;
        }
        void OnValidate() {
            validator.Invalidate();
        }
        #endregion

        private void Validator_Validation() {
        }

        public struct OBB {
            public Matrix4x4 quad;

            public OBB(Edge2D edge, float extent) {
                var v01 = edge.v1 - edge.v0;
                var xaxis = v01.normalized;
                var yaxis = new Vector2(-xaxis.y, xaxis.x);

                var len = v01.magnitude;
                xaxis *= len + 2f * extent;
                yaxis *= 2f * extent;

                var center = edge.GetPosition(0.5f);
                quad = Matrix4x4.zero;
                quad[0] = xaxis.x; quad[4] = yaxis.x; quad[12] = center.x;
                quad[1] = xaxis.y; quad[5] = yaxis.y; quad[13] = center.y;
                quad[15] = 1f;

            }
        }
    }
}