using nobnak.Gist;
using UnityEngine;
using UnityEngine.Events;

namespace Polyhedra2DZone {

    public interface ILayer {

        DefferedMatrix LayerToWorld { get; }
        DefferedMatrix LocalToLayer { get; }
        DefferedMatrix LocalToWorld { get; }

        bool Raycast(Ray ray, out float t);

        Validator LayerValidator { get; }
    }
}