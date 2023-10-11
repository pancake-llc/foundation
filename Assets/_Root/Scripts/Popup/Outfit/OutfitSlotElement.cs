using System;
using Pancake.Scriptable;
using Pancake.Spine;
using Pancake.UI;
using Spine.Unity;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class OutfitSlotElement : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic render;
        [SerializeField, SpineAnimation(dataField: nameof(render))] private string unlockedStateAnim;
        [SerializeField] private GameObject selectedObject;
        [SerializeField] private UIButton button;
        [SerializeField] private ScriptableEventNoParam eventUpdateCoin;
        [SerializeField] private ScriptableEventNoParam eventUpdatePreview;
        [SerializeField] private ScriptableEventPreviewLockedOutfit eventPreviewLockedOutfit;
        [SerializeField] private ScriptableEventNoParam eventUpdateSelectedEffect;
        [SerializeField] private OutfitTypeButtonDictionary buttonDict;

        private OutfitUnitVariable _outfitUnit;
        private OutfitType _outfitType;

        public void Init(ref OutfitUnitVariable element, OutfitType outfitType)
        {
            _outfitUnit = element;
            _outfitType = outfitType;
            eventUpdateSelectedEffect.OnRaised -= UpdateSelectedEffect;
            eventUpdateSelectedEffect.OnRaised += UpdateSelectedEffect;

            render.ChangeSkin(element.Value.skinId);
            render.transform.localPosition = element.Value.viewPosition;
            if (element.Value.isUnlocked)
            {
                foreach (var b in buttonDict)
                {
                    b.Value.gameObject.SetActive(false);
                }

                render.PlayOnly(unlockedStateAnim, true);
                UpdateSelectedEffect();
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
            if (UserData.GetCurrentCoin() >= _outfitUnit.Value.value)
            {
                UserData.MinusCoin(_outfitUnit.Value.value);
                eventUpdateCoin.Raise();
                _outfitUnit.Value.isUnlocked = true;
                _outfitUnit.Save();
                foreach (var b in buttonDict)
                {
                    b.Value.gameObject.SetActive(false);
                }
            }
        }

        private void OnButtonPressed()
        {
            // to_do with outfit
            if (_outfitUnit.Value.isUnlocked)
            {
                selectedObject.SetActive(true);

                switch (_outfitType)
                {
                    case OutfitType.Hat:
                        UserData.SetCurrentSkinHat(_outfitUnit.Value.id);
                        break;
                    case OutfitType.Shirt:
                        UserData.SetCurrentSkinShirt(_outfitUnit.Value.id);
                        break;
                    case OutfitType.Shoe:
                        UserData.SetCurrentSkinShoes(_outfitUnit.Value.id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                eventUpdatePreview.Raise();
                eventUpdateSelectedEffect.Raise();
            }
            else
            {
                eventPreviewLockedOutfit.Raise(_outfitType, _outfitUnit.Value.skinId);
            }
        }

        private void UpdateSelectedEffect()
        {
            if (_outfitUnit.Value.isUnlocked)
            {
                switch (_outfitType)
                {
                    case OutfitType.Hat:
                        string hatId = UserData.GetCurrentSkinHat();
                        selectedObject.SetActive(hatId == _outfitUnit.Value.id);
                        break;
                    case OutfitType.Shirt:
                        string shirtId = UserData.GetCurrentSkinShirt();
                        selectedObject.SetActive(shirtId == _outfitUnit.Value.id);
                        break;
                    case OutfitType.Shoe:
                        string shoeId = UserData.GetCurrentSkinShoes();
                        selectedObject.SetActive(shoeId == _outfitUnit.Value.id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                selectedObject.SetActive(false);
            }
        }

        private void OnDestroy() { eventUpdateSelectedEffect.OnRaised -= UpdateSelectedEffect; }
    }
}