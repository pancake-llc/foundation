#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
#endif
#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif
using Pancake.Common;
using Pancake.Localization;
using Pancake.Sound;
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

        [Header("SOUND"), SerializeField] protected bool enabledSound;

#if PANCAKE_ALCHEMY
        [ShowIf(nameof(enabledSound))]
#endif
        [SerializeField]
        protected Audio audioOpen;

#if PANCAKE_ALCHEMY
        [ShowIf(nameof(enabledSound))]
#endif
        [SerializeField]
        protected Audio audioClose;

        public void Show(LocaleText localeText)
        {
            PlaySoundOpen();
#if PANCAKE_LITMOTION
            LMotion.Create(imageBackgound.rectTransform.sizeDelta, new Vector2(0, sizeYExpland), timeAnimate)
                .WithOnComplete(() =>
                {
                    localeTextMessage.gameObject.SetActive(true);
                    localeTextMessage.Variable = localeText;
                    App.Delay(this, duration, Hide);
                })
                .BindToSizeDelta(imageBackgound.rectTransform)
                .AddTo(gameObject);
#endif
        }

        private void Hide()
        {
            PlaySoundClose();
            localeTextMessage.gameObject.SetActive(false);
#if PANCAKE_LITMOTION
            LMotion.Create(imageBackgound.rectTransform.sizeDelta, new Vector2(-GetComponent<RectTransform>().rect.width + sizeYColapse, sizeYColapse), timeAnimate)
                // .WithOnComplete(() => gameObject.Return())
                .BindToSizeDelta(imageBackgound.rectTransform)
                .AddTo(gameObject);
#endif
        }

        private void PlaySoundOpen()
        {
            //if (enabledSound && audioOpen != null) audioOpen.PlaySfx();
        }

        private void PlaySoundClose()
        {
            //if (enabledSound && audioClose != null) audioClose.PlaySfx();
        }
    }
}