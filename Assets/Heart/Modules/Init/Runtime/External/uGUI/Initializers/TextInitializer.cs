#if UNITY_TEXT_MESH_PRO
using TMPro;
using UnityEngine;

namespace Sisus.Init.Internal.TMPro
{
	/// <summary>
	/// Sets the <see cref="TMP_Text.text"/> property of a <see cref="TMP_Text"/> component
	/// to an Inspector-assigned value when the object is being loaded.
	/// </summary>
	[AddComponentMenu(""), RequireComponent(typeof(TMP_Text)), InitInEditMode]
	internal sealed class TextInitializer : CustomInitializer<TMP_Text, string>
	{
		#if UNITY_EDITOR
		#pragma warning disable CS0649
		/// <summary>
		/// This section can be used to customize how the Init arguments will be drawn in the Inspector.
		/// <para>
		/// The Init argument names shown in the Inspector will match the names of members defined inside this section.
		/// </para>
		/// <para>
		/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.
		/// </para>
		/// </summary>
		private sealed class Init
		{
			public string Text;
		}
		#pragma warning restore CS0649
		#endif

		#if UNITY_LOCALIZATION
		protected override bool IsRemovedAfterTargetInitialized => !(argument.reference is LocalizedString);
		#endif

		#if UNITY_EDITOR
		private protected override void OnValidate()
		{
			base.OnValidate();

			UnityEditor.EditorApplication.delayCall += ()=>
			{
				if(this != null)
				{
					InitTarget();
				}
			};
		}
		#endif

		protected override void InitTarget(TMP_Text target, string text) => target.text = text;

		#if UNITY_LOCALIZATION
		private void OnEnable()
		{
			if(argument.reference is LocalizedString localizedString)
			{
				localizedString.value.StringChanged += ApplyLocalizedString;
				return;
			}

			base.Awake();
		}

		private void OnDisable()
		{
			if(argument.reference is LocalizedString localizedString)
			{
				localizedString.value.StringChanged -= ApplyLocalizedString;
			}
		}

		private void ApplyLocalizedString(string text)
		{
			if(target == null)
			{
				if(argument.reference is LocalizedString localizedString)
				{
					localizedString.value.StringChanged -= ApplyLocalizedString;
				}

				return;
			}

			InitTarget(target, text);
		}
		#endif
	}
}
#endif