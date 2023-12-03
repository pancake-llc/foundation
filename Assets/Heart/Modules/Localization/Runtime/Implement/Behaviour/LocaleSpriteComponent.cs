using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleSpriteComponent : LocaleComponentGeneric<LocaleSprite, Sprite>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Image>("sprite");
        }
    }
}