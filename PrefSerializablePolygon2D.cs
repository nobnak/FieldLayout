using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class PrefSerializablePolygon2D : Accelerator2D {

        [SerializeField] protected string uniqueName;

        #region Unity
        protected override void OnEnable() {
            AssureHavingUniqueName();
            Load();
            base.OnEnable();
        }

        protected override void OnValidate() {
            AssureHavingUniqueName();
            base.OnValidate();
        }
        protected override void OnDisable() {
            Save();
            base.OnDisable();
        }

        #endregion

        protected void AssureHavingUniqueName() {
            if (string.IsNullOrEmpty(uniqueName))
                uniqueName = string.Format("{0}_{1}", name, GetInstanceID());
        }
        protected void Load() {
            if (PlayerPrefs.HasKey(uniqueName)) {
                var json = PlayerPrefs.GetString(uniqueName);
                JsonUtility.FromJsonOverwrite(json, this);
            }
        }
        protected void Save() {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(uniqueName, json);
        }
    }
}
