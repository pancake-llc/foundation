using System;
using System.Collections.Generic;
using Pancake.Apex.Serialization.Collections.Generic;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [Serializable]
    public class CreditTypeElementDictionary : SerializableDictionary<CreditElementType, GameObject>
    {
        [SerializeField] private List<CreditElementType> keys;
        [SerializeField] private List<GameObject> values;
        protected override List<CreditElementType> GetKeys() { return keys; }

        protected override List<GameObject> GetValues() { return values; }

        protected override void SetKeys(List<CreditElementType> keys) { this.keys = keys; }

        protected override void SetValues(List<GameObject> values) { this.values = values; }
    }
}