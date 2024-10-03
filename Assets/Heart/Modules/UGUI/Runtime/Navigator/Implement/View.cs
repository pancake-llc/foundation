using Sirenix.OdinInspector;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using Pancake.Sound;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        [Header("Sound"), SerializeField] protected bool enabledSound;

        [SerializeField, AudioPickup, ShowIf(nameof(enabledSound)), Indent]
        protected AudioId audioOpen;

        [SerializeField, AudioPickup, ShowIf(nameof(enabledSound)), Indent]
        protected AudioId audioClose;

        private bool _isInitialized;

#if PANCAKE_UNITASK
        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            PlaySoundOpen();
            await Initialize();
        }

        protected abstract UniTask Initialize();

#endif

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