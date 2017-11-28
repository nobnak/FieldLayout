using UnityEngine;

namespace Polyhedra2DZone {

    public interface ILayer {

        Matrix4x4 LayerMatrix { get; }
        Matrix4x4 InverseLayerMatrix { get; }
        Matrix4x4 LocalMatrix { get; }
        Matrix4x4 InverseLocalMatrix { get; }

        Vector3 LayerToLocal(Vector3 layerPosition);
        Vector3 LocalToLayer(Vector3 normalizedPosition);

        Vector3 WorldToLayer(Vector3 worldPosition);
        Vector3 LayerToWorld(Vector3 layerPosition);

        bool Raycast(Ray ray, out float t);
    }
}