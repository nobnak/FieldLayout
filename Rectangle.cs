using UnityEngine;
using nobnak.Gist.Extensions.CameraExt;
using nobnak.Gist.GLTools;
using nobnak.Gist.Layer2;
using nobnak.Gist.Primitive;
using nobnak.Gist;
using nobnak.Gist.Intersection;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.FieldLayout.Extensions;
using nobnak.Gist.Events.Interfaces;
using nobnak.Gist.Extension.FloatArray;
using nobnak.Gist.Extensions.ComponentExt;
using System;
using nobnak.Gist.Exhibitor;
using nobnak.Gist.MathAlgorithms;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace nobnak.FieldLayout {

	[ExecuteAlways]
    public class Rectangle : MonoBehaviour, 
		Layer.ILayerListener, IExhibitorListener, IChangeListener<Transform> {

		public EventHolder events = new EventHolder();

		public static readonly Rect LOCAL_RECT = new Rect(-0.5f, -0.5f, 1f, 1f);

		[SerializeField] protected Layer layer;
		[Range(0f, 10f)]
		[SerializeField] protected float borderThickness = 0.1f;

		protected DefferedMatrix localToLayer = new DefferedMatrix();
		protected Validator changed = new Validator();
		protected OBB2 innerBounds = new OBB2();
		protected OBB2 outerBounds = new OBB2();

		protected LocalRandom rand;

		[Header("Debug")]
        public bool debugEnabled = true;
        public Color debugColor = Color.white;
        public bool debugFill = true;
        [Range(0.01f, 0.1f)]
        public float debugLineWidth = 0.1f;

        protected GLFigure gl;

		#region Unity
		protected virtual void OnEnable() {
			rand = new LocalRandom(GetInstanceID());
			gl = new GLFigure();
            gl.DefaultLineMat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;

			changed.Validation += () => {
				if (layer == null)
					return;
				layer.LayerValidator.Validate();

				Rebuild();
				transform.hasChanged = false;
				this.CallbackSelf<IFieldListener>(a => a.TargetOnChange(this));
				//this.CallbackParent<IFieldListener>(a => a.TargetOnChange(this));
			};
			changed.Validated += () => {
				events.NotifyValidated(this);
			};
			changed.SetCheckers(() =>
				layer != null
				&& layer.IsActiveAndEnabledAlsoInEditMode()
				&& layer.LayerValidator.IsValid
				&& !transform.hasChanged);

			changed.Validate();
		}
        protected virtual void OnDisable() {
			changed.Reset();
			if (gl != null) {
                gl.Dispose();
                gl = null;
            }
		}
		protected virtual void OnValidate() {
			changed.Invalidate();
		}
		protected virtual void OnRenderObject() {
            if (!CanRender)
                return;

            var cam = Camera.current;
			if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView)
				return;

			var view = cam.worldToCameraMatrix;
            var layerToWorld = layer.LayerToWorld.Matrix;

            var worldCenter = layerToWorld.MultiplyPoint3x4(Vector3.zero);
            var width = Mathf.Min(
                debugLineWidth * cam.GetHandleSize(worldCenter),
                0.8f * borderThickness);

            var c = debugColor;
            gl.CurrentColor = c;
            gl.DrawQuad(view * layerToWorld * innerBounds.Model, width);

            gl.CurrentColor = 0.5f * c;
            gl.DrawQuad(view * layerToWorld * outerBounds.Model, width);

            if (debugFill) {
                gl.CurrentColor = 0.5f * c;
                gl.FillQuad(view * layerToWorld * innerBounds.Model);
            }
        }
        protected virtual void OnDrawGizmos() {
			#if UNITY_EDITOR
            if (!CanRender || !debugEnabled)
                return;

            var layerTopLeft = localToLayer.TransformPoint(new Vector2(-0.5f, 0.5f));
            var worldTopLeft = layer.LayerToWorld.TransformPoint(layerTopLeft);

            var style = new GUIStyle();
            style.normal.textColor = debugColor;
            Handles.Label(worldTopLeft, Title, style);
			#endif
		}
		protected virtual void Update() {
			changed.Validate();
		}
		#endregion

		#region Message
		public virtual void TargetOnChange(Layer layer) {
			if (this.layer != layer) {
				this.layer = layer;
				changed.Invalidate();
			}
		}
		#endregion

		#region IExhibitorListener
		public virtual void ExhibitorOnParent(Transform parent) {
			parent.Add(this);
		}
		public virtual void ExhibitorOnUnparent(Transform parent) {
			parent.Remove(this);
		}
		#endregion

		#region IChangeListener<Transform>
		public void TargetOnChange(Transform target) {
			if (target == transform)
				changed.Invalidate();
		}
		#endregion

		#region properties
		public string Title {
			get { return string.Format("{0}", gameObject.name); }
		}
		public Layer Layer { get { return layer; } }
		public DefferedMatrix LocalToLayer { get { return localToLayer; } }
		public float BorderThickness {
			get { return borderThickness; }
			set {
				changed.Invalidate();
				borderThickness = Mathf.Max(0f, value);
			}
		}
		#endregion

		#region interfaces
		public virtual Vector3 Sample(SideEnum side = SideEnum.Inside) {
			return this.UvToWorldPos(new Vector2(rand.Value, rand.Value));
		}

		public virtual OBB2 Bounds(SideEnum side = SideEnum.Inside) {
			switch (side) {
				case SideEnum.Outside:
				return outerBounds;
				default:
				return innerBounds;
			}
		}
		public virtual Vector2 ClosestPoint(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
			return Bounds(side).ClosestPoint(layerPoint);
		}
		public virtual ContainsResult Contains(Vector2 layerPoint, SideEnum side = SideEnum.Inside) {
			var contain = Bounds(side).Contains(layerPoint);
			return new ContainsResult(this, contain, layerPoint, ToBoundaryMode(side));
		}
		public virtual ContainsResult ContainsInOuterBoundary(Vector2 layerPoint) {
			return Contains(layerPoint, SideEnum.Outside);
		}
		public virtual ContainsResult ContainsInInnerBoundary(Vector2 layerPoint) {
			return Contains(layerPoint, SideEnum.Inside);
		}
		public virtual void Rebuild() {
			var localScale = transform.localScale;
			localScale.z = 1f;
			transform.localScale = localScale;

			var localRotation = Quaternion.Inverse(layer.transform.rotation) * transform.rotation;
			var localEuler = localRotation.eulerAngles;
			localEuler.x = localEuler.y = 0f;
			localEuler = localEuler.Quantize(0.01f);
			transform.rotation = layer.transform.rotation * Quaternion.Euler(localEuler);

			var layerPos = layer.LayerToWorld.InverseTransformPoint(transform.position);
			layerPos.z = 0f;
			transform.position = layer.LayerToWorld.TransformPoint(layerPos).Quantize(1e-6f);

			localToLayer.Reset(layer.LayerToWorld.Inverse, transform.localToWorldMatrix);

			var center = (Vector2)localToLayer.TransformPoint(Vector2.zero);
			var xaxis = (Vector2)localToLayer.TransformVector(Vector2.right);
			var yaxis = (Vector2)localToLayer.TransformVector(Vector2.up);
			var size = new Vector2(xaxis.magnitude, yaxis.magnitude);
			xaxis.Normalize();

			innerBounds.Reset(center, size, xaxis);

			var outerSize = size + 2f * borderThickness * Vector2.one;
			outerBounds.Reset(center, outerSize, xaxis);
		}
		public virtual SideEnum Side(Vector2 layerPoint) {
			if (ContainsInOuterBoundary(layerPoint).contain) {
				if (ContainsInInnerBoundary(layerPoint).contain)
					return SideEnum.Inside;
				else
					return SideEnum.Border;
			}
			return SideEnum.Outside;
		}
		#endregion

		#region member
		protected virtual bool CanRender {
			get {
				return layer != null
					&& gl != null
					&& this.IsActiveAndEnabledAlsoInEditMode()
					&& this.IsVisibleLayer()
					&& changed.Validate();
			}
		}
		protected BoundaryMode ToBoundaryMode(SideEnum side) {
			switch (side) {
				case SideEnum.Outside:
				return BoundaryMode.Outer;
				case SideEnum.Inside:
				return BoundaryMode.Inner;
				default:
				return BoundaryMode.Unknown;
			}
		}

		#endregion

		#region declarations
		public enum BoundaryMode { Unknown = 0, Inner, Outer }
		public struct ContainsResult {
			public readonly Rectangle tip;
			public readonly BoundaryMode boundary;
			public readonly bool contain;
			public readonly Vector2 layerPoint;

			public ContainsResult(Rectangle tip, bool contain, Vector2 layerPoint,
				BoundaryMode boundary = BoundaryMode.Unknown) {

				this.tip = tip;
				this.boundary = boundary;
				this.contain = contain;
				this.layerPoint = layerPoint;
			}

			public static implicit operator bool(ContainsResult cres) {
				return cres.contain;
			}

			public Vector2 LocalPosition {
				get { return tip.localToLayer.InverseTransformPoint(layerPoint); }
			}
			public Vector3 WorldPosition {
				get { return tip.layer.LayerToWorld.TransformPoint(layerPoint); }
			}

			public override string ToString() {
				return string.Format("<AbstractField.ContainsResult : {0} local={1} layer={2}>",
					tip.name, LocalPosition, layerPoint);
			}
		}
		[System.Serializable]
		public class CustomTransform {
			public bool customPosition;
			public Vector3 position = Vector3.zero;
			public bool customRotation;
			public Vector3 rotation = Vector3.zero;
			public bool customScale;
			public Vector3 scale = Vector3.one;
		}
		public interface IFieldListener : IChangeListener<Rectangle> { }
		[System.Serializable]
		public class EventHolder {
			public event System.Action<Rectangle> EventOnChange;

			public FieldEvent Changed = new FieldEvent();

			public void NotifyValidated(Rectangle self) {

				EventOnChange?.Invoke(self);
				Changed.Invoke(self);
			}
		}
		#endregion
	}
}
