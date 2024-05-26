#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public abstract class MoveShowAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }

    public abstract class MoveHideAnimation
    {
        public abstract UniTask AnimateAsync(RectTransform rectTransform);
    }
}
#endif