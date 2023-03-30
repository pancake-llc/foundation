using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Tracking
{
    [HideMono]
    [EditorIcon("scriptable_ios14")]
    public class IOS14AdSupportSetting : ScriptableSettings<IOS14AdSupportSetting>
    {
        [SerializeField, Range(0, 63), LabelText("SkAd Conversion Value")] private int skAdConversionValue = 63;

        public static int SkAdConversionValue => Instance.skAdConversionValue;
    }
}