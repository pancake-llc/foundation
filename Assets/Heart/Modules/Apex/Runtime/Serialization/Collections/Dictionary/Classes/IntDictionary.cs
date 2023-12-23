using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    [Serializable]
    public class IntDictionary : SerializableDictionary<int, int>
    {
        [SerializeField] private List<int> keys;
        [SerializeField] private List<int> values;

        protected override List<int> GetKeys() { return keys; }

        protected override List<int> GetValues() { return values; }

        protected override void SetKeys(List<int> keys) { this.keys = keys; }

        protected override void SetValues(List<int> values) { this.values = values; }
    }
}