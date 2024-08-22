using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleFontComponent : LocaleComponentGeneric<LocaleFont, Font>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Text>("font");
            TrySetComponentAndPropertyIfNotSet<TextMesh>("font");
        }
    }
}