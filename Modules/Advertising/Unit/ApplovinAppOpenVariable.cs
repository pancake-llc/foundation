using System;

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinAppOpenVariable : AdUnitVariable
    {
        public override void Load() { }
        public override bool IsReady() { throw new NotImplementedException(); }

        protected override void ShowImpl() { throw new NotImplementedException(); }

        public override void Destroy() { throw new NotImplementedException(); }
    }
}