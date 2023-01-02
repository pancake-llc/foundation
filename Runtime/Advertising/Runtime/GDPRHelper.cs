#if PANCAKE_ADS
using UnityEngine;

namespace Pancake.Monetization
{
    // ReSharper disable once InconsistentNaming
    public class GDPRHelper : MonoBehaviour
    {
        public const string GDPR_NPA_KEY = "gdpr_npa_key";

        public static int GetValueGDPR() => Storage.GetInt(GDPR_NPA_KEY, -1);

        public static bool CheckStatusGDPR()
        {
            if (GetValueGDPR() == -1)
            {
                return false;
            }

            return true;
        }

        public static void ClearStatusGDPR() { Storage.SetInt(GDPR_NPA_KEY, -1); }

        public void OnButtonAcceptPressed()
        {
            Storage.SetInt(GDPR_NPA_KEY, 0);
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void OnButtonClosePressed()
        {
            Storage.SetInt(GDPR_NPA_KEY, 1);
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void OnButtonPrivacyPolicyPressed() { Application.OpenURL(AdSettings.AdCommonSettings.PrivacyPolicyUrl); }
    }
}
#endif