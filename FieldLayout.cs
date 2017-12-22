using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Layer2;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class FieldLayout : MonoBehaviour {

        public LayerColorSampler layerColorSampler;

        [SerializeField] protected Layer layer;
        [SerializeField] protected AbstractField2D[] fields;

        protected GLFigure fig;

        #region Unity
        void OnEnable() {
            fig = new GLFigure();
        }
        void OnRenderObject() {
            if (!this.IsActiveAndEnabledAlsoInEditMode() || !this.IsActiveLayer())
                return;

            for (var i = 0; i < fields.Length; i++) {
                var f = fields[i];
                if (f == null || !f.CanRender)
                    continue;

                var layerToWorldMat = layer.LayerToWorld;
                var viewMat = Camera.current.worldToCameraMatrix;
                var modelView = viewMat * layerToWorldMat;

                var bounds = f.LayerBounds;
                var layerColor = layerColorSampler.GetColorOfLayer(i);

                var boundsMat = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
                fig.CurrentColor = 0.5f * layerColor;
                fig.DrawQuad(modelView * boundsMat);

                fig.CurrentColor = layerColor;
                f.Draw(fig);
            }
        }
        void OnDrawGizmos() {
            if (!this.IsActiveAndEnabledAlsoInEditMode() || !this.IsActiveLayer())
                return;

            for (var i = 0; i < fields.Length; i++) {
                var f = fields[i];
                if (f == null || !f.CanRender)
                    continue;
                
                var layerToWorldMat = layer.LayerToWorld;
                var bounds = f.LayerBounds;
                var labelPos = layerToWorldMat.Matrix.MultiplyPoint3x4(
                    new Vector2(bounds.xMin, bounds.yMax));
                #if UNITY_EDITOR
                var offset = layerToWorldMat.Matrix.MultiplyVector(
                    (0.2f * UnityEditor.HandleUtility.GetHandleSize(labelPos))
                    * Vector2.up);
                UnityEditor.Handles.Label(
                    labelPos + offset, 
                    string.Format("{0}:{1} / {2}", i, f.name, f.gameObject.tag));
                #endif
            }
        }
        #endregion
        
        public int Side(Vector2 layerPoint) {
            var flags = 0;
            for (var i = 0; i < fields.Length; i++) {
                var bit = 1 << i;
                var f = fields[i];
                if (!f.IsActiveAndEnabledAlsoInEditMode())
                    continue;

                if (f.Side(layerPoint) == WhichSideEnum.Inside)
                    flags |= bit;
            }

            return flags;
        }
        public int Side(Vector3 worldPoint) {
            return Side((Vector2)layer.LayerToWorld.InverseTransformPoint(worldPoint));
        }

        public Vector2 ClosestPoint(Vector2 layerPoint, int layerIndex) {
            return fields[layerIndex].ClosestPoint(layerPoint);
        }
        public Vector3 ClosestPoint(Vector3 worldPoint, int layerIndex) {
            return layer.LayerToWorld.TransformPoint(
                ClosestPoint((Vector2)
                    layer.LayerToWorld.InverseTransformPoint(worldPoint),
                    layerIndex));
        }

        public Layer Layer { get { return layer; } }
        public AbstractField2D[] Fields { get { return fields; } }
    }
}
