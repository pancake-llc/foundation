using System;

namespace Pancake.Monetization
{
    public interface IRewarded
    {
        void Register(string key, Action action);
    }
}