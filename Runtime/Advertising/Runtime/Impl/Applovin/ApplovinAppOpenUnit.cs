using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinAppOpenUnit : AppOpenAdUnit
    {
        public ApplovinAppOpenUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }

        public ApplovinAppOpenUnit()
            : base("", "")
        {
        }
    }
}