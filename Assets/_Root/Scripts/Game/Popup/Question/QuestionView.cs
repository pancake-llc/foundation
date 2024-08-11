using System;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.Localization;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public sealed class QuestionView : View
    {
        [SerializeField] private LocaleTextComponent localeMessage;
        [SerializeField] private LocaleTextComponent localeButtonOk;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonOk;

        private Action _actionOk;
        private Action _actionClose;

        protected override UniTask Initialize()
        {
            buttonOk.onClick.AddListener(OnButtonOkPressed);
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonClosePressed()
        {
            C.CallActionClean(ref _actionClose);
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnButtonOkPressed()
        {
            C.CallActionClean(ref _actionOk);
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        public void Setup(LocaleText localeMessage = null, LocaleText localeButtonOk = null, Action actionOk = null, Action actionClose = null)
        {
            if (localeMessage != null) this.localeMessage.Variable = localeMessage;
            if (localeButtonOk != null) this.localeButtonOk.Variable = localeButtonOk;
            _actionOk = actionOk;
            _actionClose = actionClose;
        }
    }
}