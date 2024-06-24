using System;
using UnityEngine;

namespace Pancake.Sound
{
    public interface IAutoResetWaitable
    {
        WaitUntil Until(Func<bool> predicate);
        WaitWhile While(Func<bool> condition);
        WaitForSeconds ForSeconds(float seconds);
    }
}