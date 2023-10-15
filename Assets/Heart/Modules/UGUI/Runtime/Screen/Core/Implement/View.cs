using Pancake.Apex;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        [Header("SOUND"), SerializeField] protected bool enabledSound;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioOpen;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioClose;
        [SerializeField, ShowIf(nameof(enabledSound))] protected ScriptableEventAudio playAudioEvent;
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
            if (enabledSound && audioOpen != null) playAudioEvent.Raise(audioOpen);
        }

        protected virtual void PlaySoundClose()
        {
            if (enabledSound && audioClose != null) playAudioEvent.Raise(audioClose);
        }
    }
}