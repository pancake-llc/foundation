using Pancake.Spine;
using Pancake.UI;
using Spine.Unity;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class OutfitSlotElement : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic render;
        [SerializeField] private UIButton button;

        private OutfitElement _outfit;

        public void Init(OutfitElement element)
        {
            _outfit = element;

            render.ChangeSkin(element.skinId);
            render.transform.localPosition = element.viewPosition;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonPressed);
        }

        private void OnButtonPressed()
        {
            
        }
    }
}