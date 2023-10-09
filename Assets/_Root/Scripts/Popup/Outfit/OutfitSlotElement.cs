using Pancake.Apex;
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
        [SerializeField] private OutfitTypeButtonDictionary buttonDict;

        private OutfitUnitVariable _outfit;

        public void Init(OutfitUnitVariable element)
        {
            _outfit = element;

            render.ChangeSkin(element.Value.skinId);
            render.transform.localPosition = element.Value.viewPosition;
            if (element.Value.isUnlocked)
            {
                foreach (var b in buttonDict)
                {
                    b.Value.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (var b in buttonDict)
                {
                    if (element.Value.unlockType == b.Key)
                    {
                        b.Value.GetComponent<CurrencyButtonStatus>().Setup(element.Value.value);
                        b.Value.gameObject.SetActive(true);
                        b.Value.onClick.RemoveListener(OnButtonPurchaseByCoinPressed);
                        b.Value.onClick.AddListener(OnButtonPurchaseByCoinPressed);
                    }
                    else
                    {
                        b.Value.gameObject.SetActive(false);
                    }
                }
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonPressed);
        }

        private void OnButtonPurchaseByCoinPressed()
        {
            if (UserData.GetCurrentCoin() >= _outfit.Value.value)
            {
                UserData.MinusCoin(_outfit.Value.value);
            }
        }

        private void OnButtonPressed()
        {
            // to_do with outfit
        }
    }
}