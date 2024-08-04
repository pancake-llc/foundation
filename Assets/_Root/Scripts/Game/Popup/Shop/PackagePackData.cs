using System;
using System.Collections.Generic;
using Pancake.IAP;
using Pancake.Localization;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(menuName = "Pancake/Game/Shop/Package Data")]
    public class PackagePackData : ScriptableObject
    {
        [Serializable]
        public class RewardData
        {
            public EShopRewardType type;
            public int value;
        }

        [SerializeField] private Sprite packageSprite;
        [SerializeField] private LocaleText packageName;
        [SerializeField] private Color packageNameColor = Color.white;
        [SerializeField] private Color packageContentColor = Color.white;
        [SerializeField] private List<RewardData> rewards = new();
#if UNITY_EDITOR
        [SerializeField] private string editorPrice;
#endif
        [SerializeField] private IAPDataVariable iapData;

        public Sprite PackageSprite => packageSprite;
        public LocaleText PackageName => packageName;
        public Color PackageNameColor => packageNameColor;
        public Color PackageContentColor => packageContentColor;
        public List<RewardData> Rewards => rewards;
        public IAPDataVariable IAPData => iapData;
#if UNITY_EDITOR
        public string EditorPrice => editorPrice;
#endif
    }
}