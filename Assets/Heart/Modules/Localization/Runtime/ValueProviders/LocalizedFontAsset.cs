using System.Diagnostics.CodeAnalysis;
using Pancake;
using Pancake.Localization;
using TMPro;
using UnityEngine;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.Internal
{
    /// <summary>
    /// Provides a <see cref="string"/> that has been localized by Unity's Localization package.
    /// <para>
    /// Can be used to retrieve an Init argument at runtime.
    /// </para>
    /// </summary>
#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
    [ValueProviderMenu("Localized Font Asset", typeof(TMP_FontAsset), Order = 1, Tooltip = "Text will be localized at runtime for the active locale.")]
#endif
#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
    [CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 100)]
#endif
    [EditorIcon("icon_value_provider")]
    public class LocalizedFontAsset : ScriptableObject,
        IValueProvider<TMP_FontAsset>
#if UNITY_EDITOR
        ,
        INullGuard
#endif
    {
        private const string MENU_NAME = "Localization/Font Asset";
        [SerializeField] internal LocaleTMPFont value;
        public TMP_FontAsset Value => value == null ? null : value.Value;

        public static implicit operator TMP_Asset(LocalizedFontAsset localeFontAsset) => localeFontAsset.Value;

#if UNITY_EDITOR
        NullGuardResult INullGuard.EvaluateNullGuard([AllowNull] Component client) => value == null ? NullGuardResult.InvalidValueProviderState : NullGuardResult.Passed;
#endif
    }
}