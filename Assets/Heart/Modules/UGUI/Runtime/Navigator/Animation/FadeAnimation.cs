#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class FadeShowAnimation
    {
        public abstract UniTask AnimateAsync(CanvasGroup canvasGroup);
    }

    public abstract class FadeHideAnimation
    {
        public abstract UniTask AnimateAsync(CanvasGroup canvasGroup);
    }
}

#endif