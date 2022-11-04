using System;
using Pancake.UI;
using TMPro;
using UnityEngine;

namespace Pancake.GameService
{
    public class PopupNotification : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtMessage;
        [SerializeField] private UIButton btnOk;

        private Action _actionOk;
        public void Message(string message) { txtMessage.text = message; }

        public void Ok(Action actionOk)
        {
            _actionOk = actionOk;
            btnOk.onClick.RemoveListener(InvokeActionOk);
            btnOk.onClick.AddListener(InvokeActionOk);
        }

        private void InvokeActionOk() { _actionOk?.Invoke(); }
    }
}