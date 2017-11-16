﻿using Gist;
using Gist.BoundingVolume;
using Gist.Extensions.Behaviour;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Fringe2D : MonoBehaviour {

        [SerializeField] protected Polygon2D polygon;
        
        [SerializeField] protected float fringeExtent = 0f;
        [SerializeField] protected Color debugBoundaryColor = Color.red;

        protected Validator validator;
        protected List<OBB> boundaries;
        protected GLFigure fig;

        #region Unity
        void OnEnable() {
            validator = new Validator();
            boundaries = new List<OBB>();
            fig = new GLFigure();

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();

            validator.Validation += () => {
                GenerateBoundaries();
            };
            polygon.OnGenerate.AddListener(PolygonOnGenerate);
        }
        private void OnRenderObject() {
            if (polygon == null || !polygon.IsActiveAndEnabledAlsoInEditMode())
                return;

            validator.CheckValidation();

            var modelview = Camera.current.worldToCameraMatrix
                * polygon.ModelMatrix;
            foreach (var b in boundaries) {
                var m = modelview * b.quad;
                fig.FillQuad(m, 0.5f * debugBoundaryColor);
                fig.DrawQuad(m, debugBoundaryColor);
            }
        }
        void OnValidate() {
            if (validator != null)
                validator.Invalidate();
        }
        private void OnDisable() {
            if (fig != null) {
                fig.Dispose();
                fig = null;
            }
            if (polygon != null)
                polygon.OnGenerate.RemoveListener(PolygonOnGenerate);
        }
        #endregion

        protected void PolygonOnGenerate() {
            validator.Invalidate();
        }
        protected void GenerateBoundaries() {
            boundaries.Clear();
            foreach (var e in polygon.IterateEdges()) {
                var obb = new OBB(e, fringeExtent);
                boundaries.Add(obb);
            }
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