using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public static class PopupHelper
    {
        public static async UniTask Close(Transform transform, bool playAnimation = true)
        {
            var popupContainer = PopupContainer.Of(transform);
            if (popupContainer.IsInTransition) return;
            await popupContainer.PopAsync(playAnimation);
        }
    }
}