using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class IronSourceAppOpenUnit : AppOpenAdUnit
    {
        public ScreenOrientation orientation = ScreenOrientation.Portrait;

        public IronSourceAppOpenUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }

        public IronSourceAppOpenUnit()
            : base("", "")
        {
            orientation = ScreenOrientation.Portrait;
        }
    }
}