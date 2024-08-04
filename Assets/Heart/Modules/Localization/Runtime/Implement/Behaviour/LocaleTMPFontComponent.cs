using TMPro;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleTMPFontComponent : LocaleComponentGeneric<LocaleTMPFont, TMP_FontAsset>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<TextMeshProUGUI>("font");
            TrySetComponentAndPropertyIfNotSet<TextMeshPro>("font");
        }
    }
}