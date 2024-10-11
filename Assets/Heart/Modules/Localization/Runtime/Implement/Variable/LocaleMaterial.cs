using System;

namespace Pancake.Localization
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Localization/Material", fileName = "material_localizevalue", order = 1)]
    [EditorIcon("so_yellow_material")]
    [Searchable]
    [Serializable]
    public class LocaleMaterial : LocaleVariable<Material>
    {
        [Serializable]
        private class MaterialLocaleItem : LocaleItem<Material>
        {
        };

        [SerializeField] private MaterialLocaleItem[] items = new MaterialLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}