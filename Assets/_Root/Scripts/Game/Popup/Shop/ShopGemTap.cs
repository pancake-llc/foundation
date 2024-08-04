using System;
using System.Collections.Generic;
using Pancake.Common;
using UnityEngine;

namespace Pancake.Game.UI
{
    public class ShopGemTap : MonoBehaviour
    {
        [SerializeField] private PackageItemVisual packageItemVisualPrefab;
        [SerializeField] private Transform packageContent;
        [SerializeField] private GemItemVisual gemItemVisualPrefab;
        [SerializeField] private Transform gemContent;

        public void Initialize(List<PackagePackData> packagesData, Func<EShopRewardType, ShopItemRewardConfig> getShopItemReward, List<GemPackData> gemsData)
        {
            packageContent.RemoveAllChildren();
            gemContent.RemoveAllChildren();
            foreach (var data in packagesData)
            {
                var visual = Instantiate(packageItemVisualPrefab, packageContent);
                visual.Setup(data, getShopItemReward);
            }

            foreach (var data in gemsData)
            {
                var visual = Instantiate(gemItemVisualPrefab, gemContent);
                visual.Setup(data);
            }
        }
    }
}