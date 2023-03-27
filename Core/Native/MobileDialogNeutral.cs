using UnityEngine;
using System;

namespace Pancake
{
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


        public static MobileDialogNeutral Create(
            string title,
            string message,
            string accept,
            string neutral,
            string decline,
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
            dialog.acceptAction = acceptAction;
            dialog.neutralAction = neutralAction;
            dialog.declineAction = declineAction;

            dialog.Init();
            return dialog;
        }

        public void Init()
        {
            MobileNative.ShowDialogNeutral(title,
                message,
                accept,
                neutral,
                decline);
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