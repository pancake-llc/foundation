using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class TabView : MonoBehaviour
    {
        [SerializeField] private Image imageTab;
        [SerializeField] private Image imageIcon;
        [Space] [SerializeField] private Sprite bgTabActive;
        [SerializeField] private Sprite bgTabUnactive;
        [SerializeField] private Sprite iconActive;
        [SerializeField] private Sprite iconUnactive;

        public void Active(bool forceNativeSize)
        {
            imageTab.sprite = bgTabActive;
            imageIcon.sprite = iconActive;
            if (forceNativeSize)
            {
                imageTab.SetNativeSize();
                imageIcon.SetNativeSize();
            }
        }

        public void Deactive(bool forceNativeSize)
        {
            imageTab.sprite = bgTabUnactive;
            imageIcon.sprite = iconUnactive;
            if (forceNativeSize)
            {
                imageTab.SetNativeSize();
                imageIcon.SetNativeSize();
            }
        }
    }
}