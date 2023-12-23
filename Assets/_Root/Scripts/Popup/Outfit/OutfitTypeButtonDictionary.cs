using System;
using System.Collections.Generic;
using Pancake.Apex.Serialization.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [Serializable]
    public class OutfitTypeButtonDictionary : SerializableDictionary<OutfitUnlockType, Button>
    {
        [SerializeField] private List<OutfitUnlockType> keys;
        [SerializeField] private List<Button> values;
        protected override List<OutfitUnlockType> GetKeys() { return keys; }

        protected override List<Button> GetValues() { return values; }

        protected override void SetKeys(List<OutfitUnlockType> keys) { this.keys = keys; }

        protected override void SetValues(List<Button> values) { this.values = values; }
    }
}