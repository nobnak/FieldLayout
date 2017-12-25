using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [System.Serializable]
    public class LayerColorSampler {
        public Color baseColor = Color.white;
        public float colorOffsetPerLayer = 0.2f;

        #region Color
        public Color GetColorOfLayer(int layer) {
            float h, s, v;
            Color.RGBToHSV(baseColor, out h, out s, out v);
            h += (layer + 1) * colorOffsetPerLayer;
            h -= Mathf.Floor(h);
            return Color.HSVToRGB(h, 1f, 1f);
        }
        public Color GetColorOfLayerMask(int layerMask) {
            var count = 0;
            var c = Color.clear;
            for (var i = 0; (layerMask >>= 1) != 0; i++) {
                if ((layerMask & 1) != 0) {
                    count++;
                    c += GetColorOfLayer(i).linear;
                }
            }
            c = (count > 0) ? (c / count) : baseColor;
            return c.gamma;
        }
        #endregion
    }
}
