using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class AppUnit : AdUnit
    {
        public AppUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }

        public AppUnit()
            : base("", "")
        {
        }
    }
}