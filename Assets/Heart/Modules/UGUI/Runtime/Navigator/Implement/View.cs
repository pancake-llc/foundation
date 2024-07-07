using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        [Header("Sound"), SerializeField] protected bool enabledSound;
        [SerializeField, AudioPickup, ShowIf(nameof(enabledSound)), Indent] protected AudioId audioOpen;
        [SerializeField, AudioPickup, ShowIf(nameof(enabledSound)), Indent] protected AudioId audioClose;
        private bool _isInitialized;

        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            PlaySoundOpen();
            await Initialize();
        }

        protected abstract UniTask Initialize();

        protected virtual void PlaySoundOpen()
        {
            if (enabledSound) audioOpen.Play();
        }

        protected virtual void PlaySoundClose()
        {
            if (enabledSound) audioClose.Play();
        }
    }
}