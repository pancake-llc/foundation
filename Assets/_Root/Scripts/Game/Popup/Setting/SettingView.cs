using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.Localization;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public class SettingView : View
    {
        [SerializeField] private LocaleTextComponent textVersion;
        [SerializeField] private LocaleTextComponent textBgm;
        [SerializeField] private LocaleTextComponent textSfx;
        [SerializeField] private LocaleTextComponent textVibrate;
        [SerializeField] private TextMeshProUGUI textUserId;

        [SerializeField, Space] private Button buttonMailbox;
        [SerializeField] private Button buttonCopyId;
        [SerializeField] private Button buttonGiftCode;
        [SerializeField] private UIButtonText buttonBgm;
        [SerializeField] private UIButtonText buttonSfx;
        [SerializeField] private UIButtonText buttonVibrate;
        [SerializeField] private Button buttonFacebook;
        [SerializeField] private Button buttonTerm;
        [SerializeField] private Button buttonPrivacy;
        [SerializeField] private Button buttonCredit;
        [SerializeField] private Button buttonBackup;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Dictionary<SystemLanguage, UIButtonText> _buttonsLanguage = new();
        [SerializeField] private Dictionary<EQuality, UIButtonText> _buttonsQuality = new();

        [SerializeField, Space] private Color colorTurnOnInGroup;
        [SerializeField] private Color colorTurnOn;
        [SerializeField] private Color colorText;

        [SerializeField, Space] private LocaleText localeTextOn;
        [SerializeField] private LocaleText localeTextOff;

        [SerializeField, Space] private string groupFacebookURL;
        [SerializeField, PopupPickup] private string creditPopupKey;
        [SerializeField, PopupPickup] private string backupDataPopupKey;

        protected override UniTask Initialize()
        {
            textUserId.text = UserData.UserId;
            DeactiveAllButtonLanguage();
            InitializeLanguage();
            InitializeQuality();
            InitializeBgm();
            InitializeSfx();
            InitializeVibrate();

            buttonMailbox.onClick.AddListener(OnButtonMailboxPressed);
            buttonGiftCode.onClick.AddListener(OnButtonGifcodePressed);
            buttonCopyId.onClick.AddListener(OnButtonCopyIdPressed);
            buttonFacebook.onClick.AddListener(OnButtonGroupFaceookPressed);
            buttonPrivacy.onClick.AddListener(OnButtonPrivacyPressed);
            buttonCredit.onClick.AddListener(OnButtonCreditPressed);
            buttonBackup.onClick.AddListener(OnButtonBackupPressed);

            textVersion.UpdateArgs(Application.version);

            buttonTerm.onClick.AddListener(OnButtonTermPressed);
            buttonClose.onClick.AddListener(OnButtoClosePressed);

            return UniTask.CompletedTask;
        }

        private void OnButtonBackupPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(backupDataPopupKey, true).Forget(); }

        private void OnButtonCreditPressed() { MainUIContainer.In.GetMain<PopupContainer>().PushAsync(creditPopupKey, true).Forget(); }

        private void OnButtonGifcodePressed()
        {
            // todo: show popup enter gitcode
        }

        private void OnButtonMailboxPressed()
        {
            // todo: show mailbox
        }

        private void OnButtonCopyIdPressed() { textUserId.text.CopyToClipboard(); }

        private async void OnButtoClosePressed()
        {
            PlaySoundClose();
            await PopupHelper.Close(transform);
        }

        private void OnButtonGroupFaceookPressed() { Application.OpenURL(groupFacebookURL); }

        private void OnButtonTermPressed() { Application.OpenURL(HeartSettings.TermOfServiceURL); }

        private void OnButtonPrivacyPressed() { Application.OpenURL(HeartSettings.PrivacyURL); }

        private void InitializeBgm()
        {
            RefreshVisualBgm();
            buttonBgm.onClick.AddListener(ChangeStatusBgm);
        }

        private void ChangeStatusBgm()
        {
            bool previous = UserData.GetMusic();
            UserData.SetMusic(!previous);
            RefreshVisualBgm();
        }

        private void RefreshVisualBgm()
        {
            if (UserData.GetMusic())
            {
                buttonBgm.image.color = colorTurnOn;
                buttonBgm.Label.color = Color.white;
                textBgm.Variable = localeTextOn;
            }
            else
            {
                buttonBgm.image.color = Color.white;
                buttonBgm.Label.color = colorText;
                textBgm.Variable = localeTextOff;
            }
        }

        private void InitializeSfx()
        {
            RefreshVisualSfx();
            buttonSfx.onClick.AddListener(ChangeStatusSfx);
        }

        private void ChangeStatusSfx()
        {
            bool previous = UserData.GetSfx();
            UserData.SetSfx(!previous);
            RefreshVisualSfx();
        }

        private void RefreshVisualSfx()
        {
            if (UserData.GetSfx())
            {
                buttonSfx.image.color = colorTurnOn;
                buttonSfx.Label.color = Color.white;
                textSfx.Variable = localeTextOn;
            }
            else
            {
                buttonSfx.image.color = Color.white;
                buttonSfx.Label.color = colorText;
                textSfx.Variable = localeTextOff;
            }
        }

        private void InitializeVibrate()
        {
            RefreshVisualVibrate();
            buttonVibrate.onClick.AddListener(ChangeStatusVibrate);
        }

        private void ChangeStatusVibrate()
        {
            bool previous = UserData.GetVibrate();
            UserData.SetVibrate(!previous);
            RefreshVisualVibrate();
        }

        private void RefreshVisualVibrate()
        {
            if (UserData.GetVibrate())
            {
                buttonVibrate.image.color = colorTurnOn;
                buttonVibrate.Label.color = Color.white;
                textVibrate.Variable = localeTextOn;
            }
            else
            {
                buttonVibrate.image.color = Color.white;
                buttonVibrate.Label.color = colorText;
                textVibrate.Variable = localeTextOff;
            }
        }

        private void InitializeLanguage()
        {
            foreach (var button in _buttonsLanguage)
            {
                if (Locale.CurrentLanguage == button.Key)
                {
                    button.Value.image.color = colorTurnOnInGroup;
                    button.Value.Label.color = Color.white;
                }
                else
                {
                    button.Value.image.color = Color.white;
                    button.Value.Label.color = colorText;
                }

                switch (button.Key)
                {
                    case SystemLanguage.English:
                        button.Value.onClick.AddListener(SelectEnglish);
                        break;
                    case SystemLanguage.Vietnamese:
                        button.Value.onClick.AddListener(SelectVietnamese);
                        break;
                }
            }
        }

        private void InitializeQuality()
        {
            foreach (var button in _buttonsQuality)
            {
                if (UserData.GetCurrentQuality() == button.Key)
                {
                    button.Value.image.color = colorTurnOnInGroup;
                    button.Value.Label.color = Color.white;
                }
                else
                {
                    button.Value.image.color = Color.white;
                    button.Value.Label.color = colorText;
                }

                switch (button.Key)
                {
                    case EQuality.Low:
                        button.Value.onClick.AddListener(SelectLowQuality);
                        break;
                    case EQuality.Medium:
                        button.Value.onClick.AddListener(SelectMediumQuality);
                        break;
                    case EQuality.High:
                        button.Value.onClick.AddListener(SelectHighQuality);
                        break;
                }
            }
        }

        public void SelectEnglish()
        {
            DeactiveAllButtonLanguage();
            Locale.CurrentLanguage = Language.English;
            var button = _buttonsLanguage[SystemLanguage.English];
            button.image.color = colorTurnOnInGroup;
            button.Label.color = Color.white;
            UserData.SetCurrentLanguage(Locale.CurrentLanguage);
        }

        public void SelectVietnamese()
        {
            DeactiveAllButtonLanguage();
            Locale.CurrentLanguage = Language.Vietnamese;
            var button = _buttonsLanguage[SystemLanguage.Vietnamese];
            button.image.color = colorTurnOnInGroup;
            button.Label.color = Color.white;
            UserData.SetCurrentLanguage(Locale.CurrentLanguage);
        }

        public void SelectLowQuality()
        {
            DeactiveAllButtonQuality();
            // todo: change quality
            var button = _buttonsQuality[EQuality.Low];
            button.image.color = colorTurnOnInGroup;
            button.Label.color = Color.white;
        }

        public void SelectMediumQuality()
        {
            DeactiveAllButtonQuality();
            // todo: change quality
            var button = _buttonsQuality[EQuality.Medium];
            button.image.color = colorTurnOnInGroup;
            button.Label.color = Color.white;
        }

        public void SelectHighQuality()
        {
            DeactiveAllButtonQuality();
            // todo: change quality
            var button = _buttonsQuality[EQuality.High];
            button.image.color = colorTurnOnInGroup;
            button.Label.color = Color.white;
        }

        private void DeactiveAllButtonLanguage()
        {
            foreach (var button in _buttonsLanguage)
            {
                button.Value.image.color = Color.white;
                button.Value.Label.color = colorText;
            }
        }

        private void DeactiveAllButtonQuality()
        {
            foreach (var button in _buttonsQuality)
            {
                button.Value.image.color = Color.white;
                button.Value.Label.color = colorText;
            }
        }
    }
}