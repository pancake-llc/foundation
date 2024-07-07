using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
//using Pancake.Sound;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        [Header("Sound"), SerializeField] protected bool enabledSound;
        //[SerializeField, ShowIf(nameof(enabledSound))] protected SoundId audioOpen;
        //[SerializeField, ShowIf(nameof(enabledSound))] protected SoundId audioClose;
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
            //if (enabledSound) audioOpen.Play();
        }

        protected virtual void PlaySoundClose()
        {
            //if (enabledSound) audioClose.Play();
        }
    }
}