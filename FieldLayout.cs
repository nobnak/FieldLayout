using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Layer2;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class FieldLayout : MonoBehaviour {

        [SerializeField] protected Layer layer;
        [SerializeField] protected AbstractField2D[] fields;
        [SerializeField] protected Color baseColor = Color.white;
        [SerializeField] protected float colorOffsetPerLayer = 0.2f;

        protected GLFigure fig;

        #region Unity
        void OnEnable() {
            fig = new GLFigure();
        }
        void OnRenderObject() {
            if (!this.IsActiveAndEnabledAlsoInEditMode())
                return;

            for (var i = 0; i < fields.Length; i++) {
                var f = fields[i];
                if (f == null || !f.CanRender)
                    continue;

                var layerToWorldMat = layer.LayerToWorld;
                var viewMat = Camera.current.worldToCameraMatrix;
                var modelView = viewMat * layerToWorldMat;

                var bounds = f.LayerBounds;
                var layerColor = GetLayerColor(i);
                
                var boundsMat = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
                fig.CurrentColor = 0.5f * layerColor;
                fig.DrawQuad(modelView * boundsMat);

                fig.CurrentColor = layerColor;
                f.Draw(fig);

                #if UNITY_EDITOR
                var labelPos = layerToWorldMat.Matrix.MultiplyPoint3x4(
                    new Vector2(bounds.xMin, bounds.yMax));
                var offset = layerToWorldMat.Matrix.MultiplyVector(
                    (0.2f * UnityEditor.HandleUtility.GetHandleSize(labelPos))
                    * Vector2.up);
                UnityEditor.Handles.Label(
                    labelPos + offset, 
                    string.Format("{0} : {1}", i, f.gameObject.tag));
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

        public Color GetLayerColor(int layer) {
            float h, s, v;
            Color.RGBToHSV(baseColor, out h, out s, out v);
            h += (layer + 1) * colorOffsetPerLayer;
            h -= Mathf.Floor(h);
            return Color.HSVToRGB(h, 1f, 1f);
        }
        public Color GetLayerMakColor(int layerMask) {
            var count = 0;
            var c = Color.black;
            for (var i = 0; i < fields.Length; i++) {
                var bit = 1 << i;
                if ((layerMask & bit) != 0) {
                    count++;
                    c += GetLayerColor(i).linear;
                }
            }
            c = (count > 0) ? (c / count) : baseColor;
            return c.gamma;
        }
    }
}
