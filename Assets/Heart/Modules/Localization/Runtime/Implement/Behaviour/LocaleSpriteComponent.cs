using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleSpriteComponent : LocaleComponentGeneric<LocaleSprite, Sprite>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<Image>("sprite"); }
    }
}