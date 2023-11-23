using System;
using UnityEngine;

namespace Pancake.Localization
{
    [Serializable]
    public abstract class LocaleItemBase : ISerializationCallbackReceiver
    {
        [SerializeField] private int languageCompatible = -1; // For backward compability.
        [SerializeField] private Language language = Language.English;

        public Language Language { get => language; set => language = value; }

        public abstract object ObjectValue { get; set; }

        public void OnBeforeSerialize()
        {
            // Intentionally empty.
        }

        public void OnAfterDeserialize()
        {
            if (languageCompatible >= 0)
            {
                language = (SystemLanguage) languageCompatible;
                languageCompatible = -1;
            }
        }
    }
}