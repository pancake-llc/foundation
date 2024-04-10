using System;
using Pancake.Common;
using Pancake.Localization;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
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

        public void SetMessage(LocaleText localeMessage) { localeText.Variable = localeMessage; }
        public void SetAction(Action action) { _action = action; }
    }
}