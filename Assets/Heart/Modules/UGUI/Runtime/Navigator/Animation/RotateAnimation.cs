#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class RotateShowAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }

    public abstract class RotateHideAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }
}
#endif