using System;
using Pancake.Common;
using Pancake.Localization;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class QuestionView : View
    {
        [SerializeField] private LocaleTextComponent localeMessage;
        [SerializeField] private LocaleTextComponent localeYes;
        [SerializeField] private Button buttonYes;
        [SerializeField] private Button buttonNo;

        private Action _actionYes;
        private Action _actionNo;

        protected override UniTask Initialize()
        {
            buttonYes.onClick.AddListener(OnButtonYesPressed);
            buttonNo.onClick.AddListener(OnButtonNoPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonNoPressed()
        {
            C.CallActionClean(ref _actionNo);
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnButtonYesPressed()
        {
            C.CallActionClean(ref _actionYes);
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        public void SetMessage(LocaleText localeMessage) { this.localeMessage.Variable = localeMessage; }

        public void SetLocaleButtonYes(LocaleText localeYes) { this.localeYes.Variable = localeYes; }

        public void SetAction(Action yes, Action no)
        {
            _actionYes = yes;
            _actionNo = no;
        }
    }
}