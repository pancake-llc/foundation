using System;
using UnityEngine;

namespace Pancake.Localization
{
    [ExecuteInEditMode]
    public abstract class LocaleComponent : MonoBehaviour
    {
        private bool _isOnValidate;
        public event EventHandler<EventArgs> LocaleChangedEvent;

        protected virtual void OnEnable()
        {
            ForceUpdate();
            if (Application.isPlaying) Locale.LocaleChangedEvent += LocaleChangedInvoke;
        }

        private void LocaleChangedInvoke(object sender, LocaleChangedEventArgs e) { ForceUpdate(); }

        public void ForceUpdate()
        {
            if (TryUpdateComponentLocalization(_isOnValidate)) OnLocaleChangedEvent();

            _isOnValidate = false;
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying) Locale.LocaleChangedEvent -= LocaleChangedInvoke;
        }

        protected virtual void OnLocaleChangedEvent() { LocaleChangedEvent?.Invoke(this, EventArgs.Empty); }

        /// <summary>
        /// Gets the localized value safely.
        /// </summary>
        protected static T GetValueOrDefault<T>(LocaleVariable<T> variable) where T : class { return variable ? variable.Value : default; }

#if UNITY_EDITOR
        private bool _firstOnValidateHasOccurred;

        private void OnValidate()
        {
            if (!_firstOnValidateHasOccurred)
            {
                _firstOnValidateHasOccurred = true;
                return;
            }

            if (System.Threading.Thread.CurrentThread.IsBackground)  UnityEditor.EditorApplication.delayCall += OnSubsequentValidateOnMainThread;
            else OnSubsequentValidateOnMainThread();
        }

        private async void OnSubsequentValidateOnMainThread()
        {
            while (UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            if (this) OnValueChangedUsingTheInspector();
        }

        private void OnValueChangedUsingTheInspector()
        {
            _isOnValidate = true;
            ForceUpdate();
        }
#endif

        /// <summary>
        /// Updates component localization if possible.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ForceUpdate"/> to update component localization.
        /// Use this method to override only.
        /// </remarks>
        /// <param name="isOnValidate">This method is whether called by <see cref="OnValidate"/> or not.</param>
        /// <returns>True if component is updated successfully.</returns>
        protected abstract bool TryUpdateComponentLocalization(bool isOnValidate);
    }
}