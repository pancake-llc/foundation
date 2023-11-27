using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/GameObject", order = 2)]
    public class LocalePrefab : LocaleVariable<GameObject>
    {
        [Serializable]
        private class PrefabLocaleItem : LocaleItem<GameObject>
        {
        };

        [SerializeField] private PrefabLocaleItem[] items = new PrefabLocaleItem[1];
        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}