using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleSpriteBehaviour : LocaleBehaviourGeneric<LocaleSprite, Sprite>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Image>("sprite");
        }
    }
}