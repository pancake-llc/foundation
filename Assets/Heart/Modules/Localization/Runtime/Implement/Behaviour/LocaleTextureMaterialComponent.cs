using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleTextureMaterialComponent : LocaleComponent
    {
        public Material material;
        public string propertyName = "_MainTex";
        public LocaleTexture localeTexture;

        protected override bool TryUpdateComponentLocalization(bool isOnValidate)
        {
            if (material != null && localeTexture != null)
            {
                material.SetTexture(propertyName, GetValueOrDefault(localeTexture));
                return true;
            }

            return false;
        }
    }
}