using Gist;
using Gist.BoundingVolume;
using Gist.Extensions.AABB;
using Gist.Extensions.Behaviour;
using Gist.Intersection;
using Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Fringe2D : MonoBehaviour {

        public UnityEvent OnGenerate;

        [SerializeField] protected Polygon2D polygon;
        
        [SerializeField] protected float fringeExtent = 0f;

        [Header("Debug")]
        [SerializeField] protected bool debugEnabled = true;
        [SerializeField] protected Color debugBoundaryColor = Color.red;

        protected Validator validator;
        protected List<OBB2> boundaries;
        protected GLFigure fig;
        protected ScopedPlug<UnityEvent> scopeOnGenerate;

        #region Unity
        void OnEnable() {
            validator = new Validator();
            boundaries = new List<OBB2>();
            fig = new GLFigure();

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();

            validator.Validation += () => {
                GenerateBoundaries();
            };

            var polygonOnGenerate = new UnityAction( () => validator.Invalidate());
            scopeOnGenerate = new ScopedPlug<UnityEvent>(
                polygon.OnGenerate, e => e.RemoveListener(polygonOnGenerate));
            scopeOnGenerate.Data.AddListener(polygonOnGenerate);
        }
        void OnRenderObject() {
            if (!debugEnabled || polygon == null || !polygon.IsActiveAndEnabledAlsoInEditMode())
                return;

            validator.CheckValidation();

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
        void OnValidate() {
            if (validator != null)
                validator.Invalidate();
        }
        private void OnDisable() {
            if (fig != null) {
                fig.Dispose();
                fig = null;
            }
            scopeOnGenerate.Dispose();
        }
        #endregion
        
        public Rect Bounds {
            get {
                validator.CheckValidation();

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
            validator.CheckValidation();

            foreach (var obb in boundaries)
                if (aabb.Intersect(obb))
                    return true;
            return false;
        }
        public bool Overlaps(Rect r) {
            return Overlaps((AABB2)r);
        }

        protected void GenerateBoundaries() {
            boundaries.Clear();
            foreach (var e in polygon.IterateEdges()) {
                var obb = GenerateConvex(e, fringeExtent);
                boundaries.Add(obb);
            }
            OnGenerate.Invoke();
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