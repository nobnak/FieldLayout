using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout.Extensions {

	public static class FieldExtension {

        public const float TWO_PI = 2f * Mathf.PI;
        public const float TWO_PI_INV = 1f / TWO_PI;

		public static Vector3 LocalToLayerPos(this Rectangle f, Vector3 localPos) {
			return f.LocalToLayer.TransformPoint(localPos);
		}
		public static Vector3 LayerToLocalPos(this Rectangle f, Vector3 localPos) {
			return f.LocalToLayer.InverseTransformPoint(localPos);
		}

		public static Vector3 LocalToWorldPos(this Rectangle f, Vector3 localPos) {
			var layerPos = f.LocalToLayerPos(localPos);
			return f.Layer.LayerToWorld.TransformPoint(layerPos);
        }
        public static Vector2 WorldToLocalPos(this Rectangle tip, Vector3 worldPos) {
            var localPos = (Vector2)tip.LayerToLocalPos(
                (Vector2)tip.Layer.LayerToWorld.InverseTransformPoint(worldPos));
            return localPos;
        }

        public static Vector3 UvToWorldPos(this Rectangle f, Vector2 normalizedPos) {
            var localPos = UvToLocalPos(normalizedPos);
            var worldPos = f.LocalToWorldPos(localPos);
            return worldPos;
        }
        public static Vector2 WorldToUvPos(this Rectangle f, Vector3 worldPos) {
            var localPos = f.WorldToLocalPos(worldPos);
            return LocalToUvPos(localPos);
        }

        public static Vector2 WorldToNormalizedPos(this Rectangle f, Vector3 worldPos) {
            var localPos = f.WorldToLocalPos(worldPos);
            return f.LocalToNormalized(localPos);
        }

        public static Vector2 LocalToNormalized(this Rectangle f, Vector2 localPos) {
            var s = f.transform.localScale;
            var aspect = s.x / s.y;
            return new Vector2((localPos.x + 0.5f) * aspect, localPos.y + 0.5f);
        }

        public static Vector2 LocalToUvPos(this Vector2 localPos) {
            return new Vector2(localPos.x + 0.5f, localPos.y + 0.5f);
        }
        public static Vector2 UvToLocalPos(this Vector2 normalizedPos) {
            return new Vector2(normalizedPos.x - 0.5f, normalizedPos.y - 0.5f);
        }

        public static float WorldToLocalRot(this Rectangle f, Vector3 forward) {
            var layerDir = f.Layer.LayerToWorld.InverseTransformVector(forward).normalized;
            var localDir = f.LocalToLayer.InverseTransformVector(layerDir).normalized;
            return Mathf.Atan2(localDir.y, localDir.x) * TWO_PI_INV;
        }
        public static Vector3 LocalToWorldRot(this Rectangle f, float normalizedRot) {
            var rad = normalizedRot * TWO_PI;
            var localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            var layerDir = f.LocalToLayer.TransformVector(localDir);
            return f.Layer.LayerToWorld.TransformVector(layerDir);
        }
    }
}
