using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleTextureComponent : LocaleComponentGeneric<LocaleTexture, Texture>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<RawImage>("texture"); }
    }
}