using Pancake.Attribute;
using Pancake.Monetization;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildValidateAdvertising : EditorPreBuildCondition
    {
        public override (bool, string) Validate()
        {
#if !PANCAKE_ADVERTISING
            return (true, "");
#else
            var advertisingSetting = Resources.Load<AdSettings>(nameof(AdSettings));
            if (advertisingSetting == null)
            {
                return (false, "Advertiting enabled but AdSettings not found!");
            }
            
            if (AdSettings.AdmobSettings.Enable)
            { 
                static bool IsAdmobSdkImported() { return AssetDatabase.FindAssets("l:gvhp_exportpath-GoogleMobileAds/GoogleMobileAds.dll").Length >= 1; }
                if (!IsAdmobSdkImported())
                {
                    return (false, "Advertiting enabled but Admob SDK not found!");
                }
            }
#if PANCAKE_ADMOB
            if (AdSettings.AdmobSettings.Enable)
            {
                if (!AdmobHelper.BridgeCheckGoogleMobileAdsSettingExist())
                {
                    return (false, "Advertiting enabled but GoogleMobileAdsSettings not found!");
                }
    
                if (AdmobHelper.IsMissingAdmobApplicationId())
                {
                    return (false, "GoogleMobileAds Application Id can not be empty!");
                }
            }
#endif

#if !PANCAKE_APPLOVIN
            
#endif
            return (true, "");
#endif
        }
    }
}