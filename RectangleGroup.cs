using Gist.Extensions.RectExt;
using nobnak.FieldLayout.Extensions;
using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Extensions.ComponentExt;
using nobnak.Gist.Layer2;
using nobnak.Gist.MathAlgorithms;
using nobnak.Gist.Primitive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout {

    [ExecuteInEditMode]
    public class RectangleGroup : Rectangle, IGroup<Rectangle>, IEnumerable<Rectangle>, Rectangle.IFieldListener {

        [SerializeField] protected List<Rectangle> fields = new List<Rectangle>();

        #region Unity
        protected override void OnEnable() {
			rand = new LocalRandom(GetInstanceID());
            fields.RemoveAll(f => f == null);
			changed.Invalidate();
		}
        #endregion

        #region IGroup
        public IList<Rectangle> Elements {
            get { return fields; }
            set {
                fields.Clear();
                fields.AddRange(value);
				changed.Invalidate();
            }
        }
        public void AddField(Rectangle f) {
            fields.Add(f);
            InitField(f);
            f.CallbackChildren<Layer.ILayerListener>(r => r.TargetOnChange(layer));
			changed.Invalidate();
		}
        public void RemvoeField(Rectangle f) {
            fields.Remove(f);
			changed.Invalidate();
		}
        #endregion

        #region IFieldListener
        public void TargetOnChange(Rectangle target) {
            changed.Invalidate();
        }
        #endregion

        #region IEnumerable
        public IEnumerator<Rectangle> GetEnumerator() {
            foreach (var f in fields) {
                if (f == null || !f.IsActiveAndEnabledAlsoInEditMode())
                    continue;
                yield return f;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
		#endregion

		#region interfaces
		public override Vector3 Sample(SideEnum side = SideEnum.Inside) {
			var i = rand.Range(0, fields.Count);
			return  (i < 0) ? default : fields[i].Sample(side);
		}

		public override Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
            var minSqDist = float.MaxValue;
            var result = Vector2.zero;
            foreach (var f in this) {
                var cp = f.ClosestPoint(layerPoint, side);
                var sqDist = (cp - layerPoint).sqrMagnitude;
                if (sqDist < minSqDist) {
                    minSqDist = sqDist;
                    result = cp;
                }
            }
            return result;
        }

        public override ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
            foreach (var f in this) {
                var contain = f.ContainsInOuterBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
            foreach (var f in this) {
                var contain = f.ContainsInInnerBoundary(layerPoint);
                if (contain)
                    return contain;
            }
            return default(ContainsResult);
        }

        public override void Rebuild() {
            foreach (var f in this) {
                InitField(f);
            }
            Debug.LogFormat("RectangleGroup Rebuld()");
        }
		#endregion

		#region methods
		private void InitField(Rectangle f) {
            f.BorderThickness = borderThickness;
        }
		#endregion
	}
}
