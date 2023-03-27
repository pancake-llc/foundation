using System;

namespace Pancake
{
    public static class NativePopup
    {
        public static void ShowMessage(string title, string message, string ok, Action okAction = null, bool touchOutSide = false)
        {
            MobileDialogInfo.Create(title,
                message,
                ok,
                touchOutSide,
                okAction);
        }

        public static void ShowQuestion(
            string title,
            string message,
            string yes,
            string no,
            Action yesAction = null,
            Action noAction = null,
            bool cancelable = true,
            bool touchOutSide = false)
        {
            MobileDialogConfirm.Create(title,
                message,
                yes,
                no,
                cancelable,
                touchOutSide,
                yesAction,
                noAction);
        }

        public static void ShowNeutral(
            string title,
            string message,
            string accept,
            string neutral,
            string decline,
            Action acceptAction = null,
            Action neutralAction = null,
            Action declineAction = null,
            bool touchOutSide = false)
        {
            MobileDialogNeutral.Create(title,
                message,
                accept,
                neutral,
                decline,
                touchOutSide,
                acceptAction,
                neutralAction,
                declineAction);
        }
    }
}