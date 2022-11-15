using UnityEngine;

namespace Pancake.Monetization
{
    // ReSharper disable once InconsistentNaming
    public class GDPRHelper : MonoBehaviour
    {
        public const string GDPR_NPA_KEY = "gdpr_npa_key";

        public static int GetValueGDPR() => StorageUtil.GetInt(GDPR_NPA_KEY, -1);

        public static bool CheckStatusGDPR()
        {
            if (GetValueGDPR() == -1)
            {
                return false;
            }

            return true;
        }

        public static void ClearStatusGDPR() { StorageUtil.SetInt(GDPR_NPA_KEY, -1); }

        public void OnButtonAcceptPressed()
        {
            StorageUtil.SetInt(GDPR_NPA_KEY, 0);
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void OnButtonClosePressed()
        {
            StorageUtil.SetInt(GDPR_NPA_KEY, 1);
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void OnButtonPrivacyPolicyPressed() { Application.OpenURL(Settings.AdSettings.PrivacyPolicyUrl); }
    }
}