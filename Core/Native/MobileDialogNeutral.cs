using UnityEngine;
using System;

namespace Pancake
{
    [AddComponentMenu("")]
    public class MobileDialogNeutral : MonoBehaviour
    {
        public Action acceptAction;
        public Action neutralAction;
        public Action declineAction;
        public string title;
        public string message;
        public string accept;
        public string neutral;
        public string decline;
        public bool touchOutSide;


        public static MobileDialogNeutral Create(
            string title,
            string message,
            string accept,
            string neutral,
            string decline,
            bool touchOutSide,
            Action acceptAction,
            Action neutralAction,
            Action declineAction)
        {
            var dialog = new GameObject("MobileDialogNeutral").AddComponent<MobileDialogNeutral>();
            dialog.title = title;
            dialog.message = message;
            dialog.accept = accept;
            dialog.neutral = neutral;
            dialog.decline = decline;
            dialog.touchOutSide = touchOutSide;
            dialog.acceptAction = acceptAction;
            dialog.neutralAction = neutralAction;
            dialog.declineAction = declineAction;

            dialog.Init();
            return dialog;
        }

        private void Init()
        {
            MobileNative.ShowDialogNeutral(title,
                message,
                accept,
                neutral,
                decline,
                touchOutSide);
        }


        public void OnAcceptCallback(string message)
        {
            acceptAction?.Invoke();
            Destroy(gameObject);
        }

        public void OnNeutralCallback(string message)
        {
            neutralAction?.Invoke();
            Destroy(gameObject);
        }

        public void OnDeclineCallback(string message)
        {
            declineAction?.Invoke();
            Destroy(gameObject);
        }
    }
}