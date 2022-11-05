#if PANCAKE_PLAYFAB
using UnityEngine;

namespace Pancake.GameService
{
    public static class LoginResultModel
    {
        public static string playerId;
        public static string playerDisplayName;
        public static string countryCode = "US"; // set default country code is US
        public static bool facebookAuth;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Setup() { playerId = ""; }

        public static void Init(string playerId, string playerDisplayName, string countryCode, bool facebookAuth)
        {
            LoginResultModel.playerId = playerId;
            LoginResultModel.playerDisplayName = playerDisplayName;
            LoginResultModel.countryCode = countryCode;
            LoginResultModel.facebookAuth = facebookAuth;
        }
    }
}
#endif