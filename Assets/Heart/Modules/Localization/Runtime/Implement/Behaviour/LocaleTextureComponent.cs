using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleTextureComponent : LocaleComponentGeneric<LocaleTexture, Texture>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<RawImage>("texture");
        }
    }
}