using nobnak.Gist.Extensions.CameraExt;
using nobnak.Gist.ObjectExt;
using TMPro;
using UnityEngine;

namespace nobnak.FieldLayout.RuntimeTextSystem {
	[ExecuteInEditMode]
    public class RuntimeText : MonoBehaviour {
		public const float HANDLE_SIZE_BASE_SIZE = 0.01f;

        public string text = "Sample";
        public float size = 20;
		public Color color = Color.white;
		public bool autoScale = true;
		public Vector2 anchor = new Vector2(0f, 1f);
		public Vector3 anchorPosition;

        protected TextMeshPro pro;

        protected void OnEnable() {
            pro = new GameObject("Text").AddComponent<TextMeshPro>();
			pro.enableWordWrapping = false;
			pro.gameObject.hideFlags = HideFlags.DontSave;
			pro.gameObject.hideFlags |= HideFlags.HideInHierarchy;
        }
		protected void OnDisable() {
			pro.DestroyGameObject();
		}
		protected void Update() {
			var worldPos = transform.TransformPoint(anchorPosition);

			pro.text = text;
			pro.color = color;
			pro.fontSize = size * (autoScale ? GetHandleSize(worldPos) : 1f);
			pro.enableAutoSizing = false;

			var recttr = pro.rectTransform;
			recttr.pivot = anchor;
			recttr.rotation = transform.rotation;
			recttr.anchoredPosition3D = worldPos;

			pro.gameObject.tag = gameObject.tag;
			pro.gameObject.layer = gameObject.layer;
		}

		public static float GetHandleSize(Vector3 worldPos) {
			return HANDLE_SIZE_BASE_SIZE * Camera.main.GetHandleSize(worldPos);
		}
    }
}
