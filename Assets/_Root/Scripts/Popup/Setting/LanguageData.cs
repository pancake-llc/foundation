using System;
using System.Collections.Generic;
using Pancake.Apex.Serialization.Collections.Generic;
using Pancake.Localization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [Serializable]
    public class LanguageData : SerializableDictionary<SystemLanguage, LocaleText>
    {
        [SerializeField] private List<SystemLanguage> keys;
        [SerializeField] private List<LocaleText> values;

        protected override List<SystemLanguage> GetKeys() { return keys; }

        protected override List<LocaleText> GetValues() { return values; }

        protected override void SetKeys(List<SystemLanguage> keys) { this.keys = keys; }

        protected override void SetValues(List<LocaleText> values) { this.values = values; }
    }
}