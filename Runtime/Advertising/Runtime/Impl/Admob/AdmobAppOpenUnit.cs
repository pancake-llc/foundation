using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobAppOpenUnit : AppOpenAdUnit
    {
        public ScreenOrientation orientation = ScreenOrientation.Portrait;

        public AdmobAppOpenUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }

        public AdmobAppOpenUnit()
            : base("", "")
        {
            orientation = ScreenOrientation.Portrait;
        }
    }
}