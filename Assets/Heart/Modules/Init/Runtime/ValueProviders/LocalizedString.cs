#if UNITY_LOCALIZATION
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Localization.Settings;
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
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME)]
	#endif
	internal sealed class LocalizedString : ScriptableObject, IValueProvider<string>, IConvertible
	#if UNITY_EDITOR
	, INullGuard
	#endif
	{
		private const string MENU_NAME = "Localized String";

		[SerializeField]
		internal UnityEngine.Localization.LocalizedString value = new UnityEngine.Localization.LocalizedString();

		public string Value => value.IsEmpty ? null : value.GetLocalizedString();

		TypeCode IConvertible.GetTypeCode() => TypeCode.String;
		string IConvertible.ToString(IFormatProvider provider) => Value;
		bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)Value).ToBoolean(provider);
		byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)Value).ToByte(provider);
		char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)Value).ToChar(provider);
		DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)Value).ToDateTime(provider);
		decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)Value).ToDecimal(provider);
		double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)Value).ToDouble(provider);
		short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)Value).ToInt16(provider);
		int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)Value).ToInt32(provider);
		long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)Value).ToInt64(provider);
		sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)Value).ToSByte(provider);
		float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)Value).ToSingle(provider);
		object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)Value).ToType(conversionType, provider);
		ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)Value).ToUInt16(provider);
		uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)Value).ToUInt32(provider);
		ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)Value).ToUInt64(provider);

		public static implicit operator string(LocalizedString localizedString) => localizedString.Value;

		private void OnDestroy() => ((IDisposable)value).Dispose();

		#if UNITY_EDITOR
		private void OnValidate()
		{
			LocalizationSettings.SelectedLocaleChanged -= SelectedLocaleChanged;
			LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;

			value.RefreshString();

			// TEMP
			if(!UnityEditor.AssetDatabase.IsMainAsset(this) && name.Length > 0)
			{
				name = "";
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		private static void SelectedLocaleChanged(UnityEngine.Localization.Locale locale) => InitInEditModeUtility.UpdateAll();

		NullGuardResult INullGuard.EvaluateNullGuard([AllowNull] Component client) => value.IsEmpty ? NullGuardResult.InvalidValueProviderState : NullGuardResult.Passed;
		#endif
	}
}
#endif