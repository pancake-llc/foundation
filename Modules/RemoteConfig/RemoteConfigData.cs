using System;
using System.Collections.Generic;
using Pancake.Apex.Serialization.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Tracking
{
    [Serializable]
    public class RemoteConfigData : SerializableDictionary<string, StringVariable>
    {
        [SerializeField] private List<string> keys;
        [SerializeField] private List<StringVariable> values;
        
        protected override List<string> GetKeys() { return keys; }

        protected override List<StringVariable> GetValues() { return values; }

        protected override void SetKeys(List<string> keys) { this.keys = keys; }

        protected override void SetValues(List<StringVariable> values) { this.values = values; }
    }
}