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

        public static MobileDialogInfo Create(string title, string message, string ok, Action okAction)
        {
            var dialog = new GameObject("MobileDialogInfo").AddComponent<MobileDialogInfo>();
            dialog.hideFlags = HideFlags.HideInHierarchy;
            dialog.title = title;
            dialog.message = message;
            dialog.ok = ok;
            dialog.okAction = okAction;

            dialog.Init();
            return dialog;
        }

        private void Init() { MobileNative.ShowInfoPopup(title, message, ok); }


        public void OnOkCallback(string message)
        {
            okAction?.Invoke();
            Destroy(gameObject);
        }
    }
}