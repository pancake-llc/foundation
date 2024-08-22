using System;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class NotificationView : View
    {
        [SerializeField] private LocaleTextComponent localeText;
        [SerializeField] private Button buttonOk;

        private Action _action;

        protected override UniTask Initialize()
        {
            buttonOk.onClick.AddListener(OnButtonOkPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonOkPressed()
        {
            C.CallActionClean(ref _action);
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        public void Setup(LocaleText message, Action action)
        {
            localeText.Variable = message;
            _action = action;
        }
    }
}