using System;
using System.Linq;
using Pancake.Apex;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleTextComponent : LocaleComponentGeneric<LocaleText, string>
    {
        [SerializeField, Array] private string[] formatArgs = Array.Empty<string>();

        public string[] FormatArgs
        {
            get => formatArgs;
            set
            {
                formatArgs = value ?? Array.Empty<string>();
                ForceUpdate();
            }
        }

        protected override object GetLocaleValue()
        {
            var value = (string) base.GetLocaleValue();
            if (FormatArgs.Length > 0 && !string.IsNullOrEmpty(value))
            {
                return string.Format(value, FormatArgs.Cast<object>().ToArray());
            }

            return value;
        }

        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Text>("text");
            TrySetComponentAndPropertyIfNotSet<TextMeshProUGUI>("text");
        }
    }
}