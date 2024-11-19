#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Pancake.UI
{
    public static class PopupHelper
    {
#if PANCAKE_UNITASK
        public static async UniTask Close(Transform transform, bool playAnimation = true)
        {
            var popupContainer = PopupContainer.Of(transform);
            if (popupContainer.IsInTransition) return;
            await popupContainer.PopAsync(playAnimation);
        }
#endif
    }
}