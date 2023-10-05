using Pancake.UI;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public static class PopupHelper
    {
        public static void Close(Transform transform, bool playAnimation = true)
        {
            var popupContainer = PopupContainer.Of(transform);
            if (popupContainer.IsInTransition) return;
            popupContainer.Pop(playAnimation);
        }
    }
}