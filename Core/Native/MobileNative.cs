using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pancake
{
    public static class MobileNative
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogNeutral(string title, string message, string accept, string neutral, string decline);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogConfirm(string title, string message, string yes, string no);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogInfo(string title, string message, string ok);

        [DllImport("__Internal")]
        private static extern void _TAG_DismissCurrentAlert();
	
        [DllImport ("__Internal")]
        private static extern void _TAG_ShowDatePicker(int mode, double unix);

#endif

        public static void ShowDialogNeutral(string title, string message, string accept, string neutral, string decline)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogNeutral(title, message, accept, neutral, decline);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.nativepopup.Bridge");
            javaUnityClass.CallStatic("ShowDialogNeutral", title, message, accept, neutral, decline);
#endif
        }

        /// <summary>
        /// Calls a Native Confirm Dialog on iOS and Android
        /// </summary>
        /// <param name="title">Dialog title text</param>
        /// <param name="message">Dialog message text</param>
        /// <param name="yes">Accept Button text</param>
        /// <param name="no">Cancel Button text</param>
        /// <param name="cancelable">Android only. Allows setting the cancelable property of the dialog</param>
        public static void ShowDialogConfirm(string title, string message, string yes, string no, bool cancelable = true)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogConfirm(title, message, yes, no);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.nativepopup.Bridge");
            javaUnityClass.CallStatic("ShowDialogConfirm", title, message, yes, no, cancelable);
#endif
        }

        public static void ShowInfoPopup(string title, string message, string ok)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogInfo(title, message, ok);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.nativepopup.Bridge");
            javaUnityClass.CallStatic("ShowDialogInfo", title, message, ok);
#endif
        }

        public static void DismissCurrentAlert()
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_DismissCurrentAlert();
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.nativepopup.Bridge");
            javaUnityClass.CallStatic("DismissCurrentAlert");
#endif
        }
    }
}