using System;

namespace Pancake.Monetization
{
    public interface IInterstitial
    {
        void Register(string key, Action action);
    }
}