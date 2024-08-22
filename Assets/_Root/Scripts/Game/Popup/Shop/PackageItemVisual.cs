using System;
using Pancake.IAP;
using Pancake.Localization;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace Pancake.Game.UI
{
    public class PackageItemVisual : MonoBehaviour
    {
        [SerializeField] private Button buttonPurchase;
        [SerializeField] private Image imageBackground;
        [SerializeField] private Transform groupRewardInfo;
        [SerializeField] private Transform groupReward;
        [SerializeField] private LocaleTextComponent localeTextRewardInfoPrefab;
        [SerializeField] private GameObject tagObject;
        [SerializeField] private LocaleTextComponent localeTextTag;
        [SerializeField] private LocaleTextComponent localeTextName;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private TextMeshProUGUI textCost;

        private IAPDataVariable _iap;

        public void Setup(PackagePackData data, Func<EShopRewardType, ShopItemRewardConfig> getShopItemReward)
        {
            _iap = data.IAPData;
            buttonPurchase.onClick.AddListener(OnButtonPurchasePressed);
            imageBackground.sprite = data.PackageSprite;
            localeTextName.Variable = data.PackageName;
            textName.color = data.PackageNameColor;
            tagObject.SetActive(data.HasNameTag);
            localeTextTag.Variable = data.NameTag;

#if UNITY_EDITOR
            textCost.text = data.EditorPrice;
#else
            textCost.text = data.IAPData.localizedPrice;
#endif
            foreach (var reward in data.Rewards)
            {
                var config = getShopItemReward.Invoke(reward.type);
                var item = Instantiate(config.Prefab, groupReward);
                item.Setup($"{config.Prefix}{reward.value}");

                var info = Instantiate(localeTextRewardInfoPrefab, groupRewardInfo);
                info.Variable = config.ItemName;
                info.UpdateArgs($"{reward.value}");
                var text = info.GetComponent<TextMeshProUGUI>();
                text.color = data.PackageContentColor;
            }
        }

        private void OnButtonPurchasePressed() { _iap.OnPurchaseCompleted(OnPurchaseCompleted).Purchase(); }

        private void OnPurchaseCompleted()
        {
            // todo
            Debug.Log($"IAP: pruchase {_iap.id} success");
        }
    }
}