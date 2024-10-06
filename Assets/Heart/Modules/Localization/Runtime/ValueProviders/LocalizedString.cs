using System.Diagnostics.CodeAnalysis;
using Pancake;
using Pancake.Localization;
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
    [ValueProviderMenu("Localized String", typeof(string), Order = 1, Tooltip = "Text will be localized at runtime for the active locale.")]
#endif
#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
    [CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 101)]
#endif
    [EditorIcon("icon_value_provider")]
    public sealed class LocalizedString : ScriptableObject,
        IValueProvider<string>
#if UNITY_EDITOR
        ,
        INullGuard
#endif
    {
        private const string MENU_NAME = "Localization/String";

        [SerializeField] internal LocaleText value;

        public string Value => value == null ? null : value.Value;

        public static implicit operator string(LocalizedString localizedString) => localizedString.Value;

#if UNITY_EDITOR
        NullGuardResult INullGuard.EvaluateNullGuard([AllowNull] Component client) => value == null ? NullGuardResult.InvalidValueProviderState : NullGuardResult.Passed;
#endif
    }
}