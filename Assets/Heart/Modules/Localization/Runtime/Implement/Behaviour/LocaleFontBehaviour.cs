using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleFontBehaviour : LocaleBehaviourGeneric<LocaleFont, Font>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Text>("font");
            TrySetComponentAndPropertyIfNotSet<TextMeshProUGUI>("font");
        }
    }
}