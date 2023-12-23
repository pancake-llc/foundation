using UnityEngine;
using System;

namespace Pancake
{
    [AddComponentMenu("")]
    public class MobileDialogConfirm : MonoBehaviour
    {
        public Action yesAction;
        public Action noAction;
        public string title;
        public string message;
        public string yes;
        public string no;
        public bool touchOutSide;

        // Constructor
        public static MobileDialogConfirm Create(
            string title,
            string message,
            string yes,
            string no,
            bool touchOutSide,
            Action yesAction,
            Action noAction)
        {
            var dialog = new GameObject("MobileDialogConfirm").AddComponent<MobileDialogConfirm>();
            dialog.title = title;
            dialog.message = message;
            dialog.yes = yes;
            dialog.no = no;
            dialog.yesAction = yesAction;
            dialog.noAction = noAction;
            dialog.touchOutSide = touchOutSide;
            dialog.Init();
            return dialog;
        }

        private void Init()
        {
            MobileNative.ShowDialogConfirm(title,
                message,
                yes,
                no,
                touchOutSide);
        }


        public void OnYesCallback(string message)
        {
            yesAction?.Invoke();
            Destroy(gameObject);
        }

        public void OnNoCallback(string message)
        {
            noAction?.Invoke();
            Destroy(gameObject);
        }
    }
}