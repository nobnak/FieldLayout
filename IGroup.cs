using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.FieldLayout {

    public interface IGroup<ELementType> {

        IList<ELementType> Elements { get; }
        void AddField(ELementType element);
        void RemvoeField(ELementType element);
    }
}
