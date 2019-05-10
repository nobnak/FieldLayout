using nobnak.FieldLayout.RuntimeTextSystem;
using UnityEngine;

namespace nobnak.FieldLayout {
	[RequireComponent(typeof(RuntimeText))]
	public class RectangleWithText : Rectangle {
		[SerializeField]
		protected RuntimeText text;

		protected override void OnEnable() {
			base.OnEnable();

			if ((text = GetComponent<RuntimeText>()) == null)
				text = gameObject.AddComponent<RuntimeText>();
			text.gameObject.layer = gameObject.layer;
		}
		protected override void OnDisable() {
			base.OnDisable();
		}
		protected override void OnDrawGizmos() {
			//base.OnDrawGizmos();
		}
		protected override void Update() {
			base.Update();
			text.text = Title;
			text.color = debugColor;
		}
	}
}
