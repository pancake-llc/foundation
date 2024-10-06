#if UNITY_UGUI
using Pancake.Localization;
using TMPro;
using UnityEngine;

namespace Sisus.Init.Internal.TMPro
{
    /// <summary>
    /// Sets the <see cref="TMP_Text.text"/> property of a <see cref="TMP_Text"/> component
    /// to an Inspector-assigned value when the object is being loaded.
    /// </summary>
    [AddComponentMenu(""), RequireComponent(typeof(TMP_Text)), InitInEditMode]
    internal sealed class TextInitializer : CustomInitializer<TMP_Text, string, TMP_FontAsset>
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
            public TMP_FontAsset FontAsset;
        }
#pragma warning restore CS0649
#endif
        protected override bool IsRemovedAfterTargetInitialized => firstArgument.reference is not LocalizedString && secondArgument.reference is not LocalizedFontAsset;

        private void OnEnable()
        {
            if (firstArgument.reference is LocalizedString)
            {
                if (Application.isPlaying) Locale.LocaleChangedEvent += OnLocaleChanged;
                return;
            }

            Awake();
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            if (target == null)
            {
                if (firstArgument.reference is LocalizedString) Locale.LocaleChangedEvent -= OnLocaleChanged;

                return;
            }

            if (firstArgument.reference is LocalizedString text && secondArgument.reference is LocalizedFontAsset font) InitTarget(target, text.Value, font.Value);
        }

        private void OnDisable()
        {
            if (firstArgument.reference is LocalizedString)
            {
                if (Application.isPlaying) Locale.LocaleChangedEvent -= OnLocaleChanged;
            }
        }

        protected override void InitTarget(TMP_Text target, string text, TMP_FontAsset font)
        {
            target.text = text;
            target.font = font;
        }
    }
}
#endif