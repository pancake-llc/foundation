using System;
using System.Collections.Generic;
using Pancake.Common;
using UnityEngine;

namespace Pancake.Game.UI
{
    public class ShopPackageTab : MonoBehaviour
    {
        [SerializeField] private PackageItemVisual packageItemVisualPrefab;
        [SerializeField] private Transform content;

        public void Initialize(List<PackagePackData> packagesData, Func<EShopRewardType, ShopItemRewardConfig> getShopItemReward)
        {
            content.RemoveAllChildren();
            foreach (var data in packagesData)
            {
                var visual = Instantiate(packageItemVisualPrefab, content);
                visual.Setup(data, getShopItemReward);
            }
        }
    }
}