using System;
using UnityEngine;

namespace Pancake.Localization
{
    [Serializable]
    public abstract class ScriptableLocaleBase : ScriptableObject
    {
        /// <summary>
        /// Gets the read-only locale items.
        /// </summary>
        public abstract LocaleItemBase[] LocaleItems { get; }

        /// <summary>
        /// Gets the value type.
        /// </summary>
        public abstract Type GetGenericType { get; }
    }
}