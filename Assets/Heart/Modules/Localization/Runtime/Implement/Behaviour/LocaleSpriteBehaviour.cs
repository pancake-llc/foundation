using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Localization
{
    public class LocaleSpriteBehaviour : LocaleBehaviourGeneric<LocaleSprite, Sprite>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Image>("image");
        }
    }
}