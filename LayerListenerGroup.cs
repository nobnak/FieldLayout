using Gist.Extensions.RectExt;
using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public class LayerListenerGroup : MonoBehaviour,
		IGroup<Layer.ILayerListener>, IEnumerable<Layer.ILayerListener> {

		public Link link = new Link();
		public Runtime tmp = new Runtime();

        #region Unity
        protected void OnEnable() {
            tmp.listeners.RemoveAll(f => f == null);
        }
        #endregion

        #region IGroup
        public IList<Layer.ILayerListener> Elements {
            get { return tmp.listeners; }
            set {
                tmp.listeners.Clear();
                tmp.listeners.AddRange(value);
            }
        }
        public void AddField(Layer.ILayerListener f) {
            tmp.listeners.Add(f);
            f.TargetOnChange(link.layer);
        }
        public void RemvoeField(Layer.ILayerListener f) {
            tmp.listeners.Remove(f);
        }
        #endregion

        #region IEnumerable
        public IEnumerator<Layer.ILayerListener> GetEnumerator() {
            foreach (var f in tmp.listeners) {
                if (f == null)
                    continue;
                yield return f;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
		#endregion

		#region declarations
		[System.Serializable]
		public class Link {
			public Layer layer;
		}
		[System.Serializable]
		public class Runtime {
			public List<Layer.ILayerListener> listeners = new List<Layer.ILayerListener>();
		}
		#endregion
	}
}
