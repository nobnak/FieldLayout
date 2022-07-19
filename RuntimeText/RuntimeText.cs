using nobnak.Gist.Extensions.CameraExt;
using nobnak.Gist.ObjectExt;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace nobnak.FieldLayout.RuntimeTextSystem {
	[ExecuteInEditMode]
    public class RuntimeText : MonoBehaviour {
		public const float HANDLE_SIZE_BASE_SIZE = 0.01f;
		public const string ROOT_OBJ_NAME = "Text Holder";

		public string text = "Sample";
        public float size = 20;
		public Color color = Color.white;
		public bool autoScale = true;
		public Vector2 anchor = new Vector2(0f, 1f);
		public Vector3 anchorPosition;

		static GameObject root;

        protected TextMeshPro pro;

        #region unity
        protected void OnEnable() {
			if (root == null) root = GameObject.Find(ROOT_OBJ_NAME);
			if (root == null) root = new GameObject(ROOT_OBJ_NAME);

			pro = new GameObject("Text").AddComponent<TextMeshPro>();
			pro.enableWordWrapping = false;
			pro.gameObject.hideFlags = HideFlags.DontSave;
			//pro.gameObject.hideFlags |= HideFlags.HideInHierarchy;
			pro.transform.SetParent(root.transform, true);
        }
		protected void OnDisable() {
			pro.DestroyGo();
		}
		protected void Update() {
			var worldPos = transform.TransformPoint(anchorPosition);

			pro.text = text;
			pro.color = color;
			pro.fontSize = size * (autoScale ? GetHandleSize(worldPos) : 1f);
			pro.enableAutoSizing = false;

			var recttr = pro.rectTransform;
            //recttr.SetParent(transform, false);
			recttr.pivot = anchor;
			recttr.rotation = transform.rotation;
			recttr.anchoredPosition3D = worldPos;

			pro.gameObject.tag = gameObject.tag;
			pro.gameObject.layer = gameObject.layer;
		}
        private void OnDrawGizmos() {
            if (pro != null) {
                var tr = pro.rectTransform;
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(
                    tr.TransformPoint(0.5f * Vector3.left), 
                    tr.TransformPoint(0.5f * Vector3.right));
                Gizmos.DrawLine(
                    tr.TransformPoint(0.5f * Vector3.down),
                    tr.TransformPoint(0.5f * Vector3.up));
            }
        }
        #endregion

        public static float GetHandleSize(Vector3 worldPos) {
			return HANDLE_SIZE_BASE_SIZE * Camera.main.GetHandleSize(worldPos);
		}
    }
}
