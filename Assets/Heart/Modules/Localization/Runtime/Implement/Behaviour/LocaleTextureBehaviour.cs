using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    public class LocaleTextureBehaviour : LocaleBehaviourGeneric<LocaleTexture, Texture>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<RawImage>("texture");
        }
    }
}