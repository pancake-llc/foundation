using System.Runtime.InteropServices;
using UnityEngine;

namespace Pancake
{
    internal static class MobileNative
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogNeutral(string title, string message, string accept, string neutral, string decline);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogConfirm(string title, string message, string yes, string no);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowDialogInfo(string title, string message, string ok);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowLongToast(string message);

        [DllImport("__Internal")]
        private static extern void _TAG_ShowShortToast(string message);

        [DllImport("__Internal")]
        private static extern void _TAG_DismissCurrentAlert();

#endif
    
        internal static void ShowDialogNeutral(string title, string message, string accept, string neutral, string decline, bool touchOutSide)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogNeutral(title, message, accept, neutral, decline);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.unitynative.Bridge");
            javaUnityClass.CallStatic("ShowDialogNeutral", title, message, accept, neutral, decline, touchOutSide);
#endif
        }

        /// <summary>
        /// Calls a Native Confirm Dialog on iOS and Android
        /// </summary>
        /// <param name="title">Dialog title text</param>
        /// <param name="message">Dialog message text</param>
        /// <param name="yes">Accept Button text</param>
        /// <param name="no">Cancel Button text</param>
        /// <param name="touchOutSide"></param>
        internal static void ShowDialogConfirm(string title, string message, string yes, string no, bool touchOutSide)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogConfirm(title, message, yes, no);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.unitynative.Bridge");
            javaUnityClass.CallStatic("ShowDialogConfirm", title, message, yes, no, touchOutSide);
#endif
        }

        internal static void ShowInfoPopup(string title, string message, string ok, bool touchOutSide)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_ShowDialogInfo(title, message, ok);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.unitynative.Bridge");
            javaUnityClass.CallStatic("ShowDialogInfo", title, message, ok, touchOutSide);
#endif
        }
        
        internal static void ShowToast(string message, int style)
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            if(style == 0) _TAG_ShowShortToast(message);
            else _TAG_ShowLongToast(message);
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.unitynative.Bridge");
            javaUnityClass.CallStatic("ShowToast", message, style);
#endif
        }

        internal static void DismissCurrentAlert()
        {
#if UNITY_EDITOR
#elif UNITY_IPHONE
            _TAG_DismissCurrentAlert();
#elif UNITY_ANDROID
            AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.pancake.unitynative.Bridge");
            javaUnityClass.CallStatic("DismissCurrentAlert");
#endif
        }
    }
}