using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout.AbstractFieldExt {

	public static class AbstractFieldExtension {

		public static Vector3 LocalToWorldPos(this AbstractField af, Vector3 localPos) {
			var layerPos = af.LocalToLayer.TransformPoint(localPos);
			return af.Layer.LayerToWorld.TransformPoint(layerPos);
		}
	}
}
