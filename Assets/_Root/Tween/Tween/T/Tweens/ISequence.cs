using System;

namespace Pancake.Core
{
    public interface ISequence : ITween
    {
        void Append(ITween tween);
        void Join(ITween tween);
        void AppendCallback(Action callback, bool callIfCompletingInstantly = true);
        void JoinCallback(Action callback, bool callIfCompletingInstantly = true);
        void AppendResetableCallback(Action callback, Action reset, bool callIfCompletingInstantly = true);
        void JoinResetableCallback(Action callback, Action reset, bool callIfCompletingInstantly = true);
    }
}