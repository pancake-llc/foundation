using Pancake.Apex;
using Pancake.Sound;
using UnityEngine;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        [Header("SOUND"), SerializeField] protected bool enabledSound;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioOpen;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioClose;
        [SerializeField, ShowIf(nameof(enabledSound))] protected ScriptableEventAudio playAudioEvent;
        private bool _isInitialized;

#if PANCAKE_UNITASK
        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            PlaySoundOpen();
            await Initialize();
        }
#endif

#if PANCAKE_UNITASK
        protected abstract UniTask Initialize();
#endif

        protected virtual void PlaySoundOpen()
        {
            if (enabledSound && audioOpen != null) playAudioEvent.Raise(audioOpen);
        }

        protected virtual void PlaySoundClose()
        {
            if (enabledSound && audioClose != null) playAudioEvent.Raise(audioClose);
        }
    }
}