using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    public class LocaleFontBehaviour : LocaleBehaviourGeneric<LocaleFont, Font>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Text>("label");
            TrySetComponentAndPropertyIfNotSet<TextMeshProUGUI>("label");
        }
    }
}