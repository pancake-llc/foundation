using UnityEngine;

namespace Pancake.Localization
{
    public class LocaleMaterialBehaviour : LocaleBehaviour
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