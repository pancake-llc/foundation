using Pancake.Apex;
using Pancake.Common;
using Pancake.Localization;
using Pancake.Scriptable;
using Pancake.Sound;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Component
{
    public class InGameNotification : GameComponent
    {
        [SerializeField] private Image imageBackgound;
        [SerializeField] private LocaleTextComponent localeTextMessage;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float timeAnimate = 0.5f;
        [SerializeField] private float sizeYColapse = 74f;
        [SerializeField] private float sizeYExpland = 90f;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;

        [Header("SOUND"), SerializeField] protected bool enabledSound;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioOpen;
        [SerializeField, ShowIf(nameof(enabledSound))] protected Audio audioClose;
        [SerializeField, ShowIf(nameof(enabledSound))] protected ScriptableEventAudio playAudioEvent;


        public void Show(LocaleText localeText)
        {
            PlaySoundOpen();
            Tween.UISizeDelta(imageBackgound.rectTransform, new Vector2(0, sizeYExpland), timeAnimate)
                .OnComplete(() =>
                {
                    localeTextMessage.gameObject.SetActive(true);
                    localeTextMessage.Variable = localeText;
                    App.Delay(this, duration, Hide);
                });
        }

        private void Hide()
        {
            PlaySoundClose();
            localeTextMessage.gameObject.SetActive(false);
            Tween.UISizeDelta(imageBackgound.rectTransform, new Vector2(-GetComponent<RectTransform>().rect.width + sizeYColapse, sizeYColapse), timeAnimate)
                .OnComplete(() => returnPoolEvent.Raise(gameObject));
        }

        private void PlaySoundOpen()
        {
            if (enabledSound && audioOpen != null) playAudioEvent.Raise(audioOpen);
        }

        private void PlaySoundClose()
        {
            if (enabledSound && audioClose != null) playAudioEvent.Raise(audioClose);
        }
    }
}