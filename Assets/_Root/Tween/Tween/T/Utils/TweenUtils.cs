using System;
using UnityEngine;

namespace Pancake.Core.Tween
{
    public static class TweenUtils
    {
        public static bool CanAddTween(ITween toAddAt, ITween tweenToAdd)
        {
            Tween castedTween = tweenToAdd as Tween;

            if (castedTween == null)
            {
                return false;
            }

            if (castedTween.IsPlaying)
            {
                return false;
            }

            if (castedTween.IsPlaying)
            {
                return false;
            }

            if (castedTween.IsNested)
            {
                return false;
            }

            return true;
        }
    }
}