#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class ScaleShowAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }

    public abstract class ScaleHideAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }
}
#endif