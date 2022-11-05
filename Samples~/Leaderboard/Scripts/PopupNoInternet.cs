using System;
using Pancake.UI;
using UnityEngine;

namespace Pancake.GameService
{
    public class PopupNoInternet : MonoBehaviour
    {
        [SerializeField] private UIButton btnOk;

        private Action _actionOk;

        public void Ok(Action actionOk)
        {
            _actionOk = actionOk;
            btnOk.onClick.RemoveListener(InvokeActionOk);
            btnOk.onClick.AddListener(InvokeActionOk);
        }

        private void InvokeActionOk() { _actionOk?.Invoke(); }
    }
}