using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleMaterialComponent : LocaleComponent
    {
        public Material material;
        public string propertyName = "_MainTex";
        public LocaleTexture localeTexture;

        protected override bool TryUpdateComponentLocalization(bool isOnValidate)
        {
            if (material && localeTexture)
            {
                material.SetTexture(propertyName, GetValueOrDefault(localeTexture));
                return true;
            }

            return false;
        }
    }
}