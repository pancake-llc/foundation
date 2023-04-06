using UnityEngine;
using System;


namespace Pancake
{
    [AddComponentMenu("")]
    public class MobileDialogInfo : MonoBehaviour
    {
        public Action okAction;
        public string title;
        public string message;
        public string ok;
        public bool touchOutSide;

        public static MobileDialogInfo Create(string title, string message, string ok, bool touchOutSide, Action okAction)
        {
            var dialog = new GameObject("MobileDialogInfo").AddComponent<MobileDialogInfo>();
            dialog.hideFlags = HideFlags.HideInHierarchy;
            dialog.title = title;
            dialog.message = message;
            dialog.ok = ok;
            dialog.touchOutSide = touchOutSide;
            dialog.okAction = okAction;

            dialog.Init();
            return dialog;
        }

        private void Init() { MobileNative.ShowInfoPopup(title, message, ok, touchOutSide); }


        public void OnOkCallback(string message)
        {
            okAction?.Invoke();
            Destroy(gameObject);
        }
    }
}