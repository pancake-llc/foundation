using TMPro;

namespace Pancake.Localization
{
    using UnityEngine;

    [EditorIcon("icon_default")]
    public class LocaleMaterialComponent : LocaleComponentGeneric<LocaleMaterial, Material>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Renderer>("material");
            TrySetComponentAndPropertyIfNotSet<TMP_Text>("fontMaterial");
        }
    }
}