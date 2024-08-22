using System;

namespace Pancake.SignIn
{
    public static class SignInEvent
    {
        public static bool status;
        public static string ServerCode { get; internal set; }
        internal static event Action LoginEvent;
        internal static event Action GetNewServerCodeEvent;
#if UNITY_IOS
        public static string UserId { get; internal set; }
#endif

        public static void Login() { LoginEvent?.Invoke(); }
        public static void GetNewServerCode() { GetNewServerCodeEvent?.Invoke(); }
    }
}