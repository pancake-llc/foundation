using TMPro;

namespace Pancake.Localization
{
    using UnityEngine;

    [EditorIcon("csharp")]
    public class LocaleMaterialComponent : LocaleComponentGeneric<LocaleMaterial, Material>
    {
        private void Reset()
        {
            TrySetComponentAndPropertyIfNotSet<Renderer>("material");
            TrySetComponentAndPropertyIfNotSet<TMP_Text>("fontMaterial");
        }
    }
}