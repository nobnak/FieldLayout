using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout.Extension {

    public static class GroupExtension {

        public static void Add<ElementType>(this Transform parent, ElementType element) {
            ActionOnParent<ElementType>(parent, g => g.AddField(element));
        }
        public static void Remove<ElementType>(this Transform parent, ElementType element) {
            ActionOnParent<ElementType>(parent, g => g.RemvoeField(element));
        }
        public static void ActionOnParent<ElementType>(
            Transform parent, System.Action<IGroup<ElementType>> action) { 
            var group = parent.GetComponent<IGroup<ElementType>>();
            if (group != null)
                action(group);
        }
    }
}