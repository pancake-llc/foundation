using UnityEngine;

namespace Pancake.Tracking
{
    //[HideMonoScript]
    [EditorIcon("scriptable_ios14")]
    public class IOS14AdSupportSetting : ScriptableSettings<IOS14AdSupportSetting>
    {
        //[LabelText("SkAd Conversion Value")]
        [SerializeField, Range(0, 63)] private int skAdConversionValue = 63;

        public static int SkAdConversionValue => Instance.skAdConversionValue;
    }
}