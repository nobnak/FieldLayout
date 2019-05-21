using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout.Extensions {

	public static class FieldExtension {

		public static Vector3 LocalToWorldPos(this Field af, Vector3 localPos) {
			var layerPos = af.LocalToLayer.TransformPoint(localPos);
			return af.Layer.LayerToWorld.TransformPoint(layerPos);
		}
	}
}
