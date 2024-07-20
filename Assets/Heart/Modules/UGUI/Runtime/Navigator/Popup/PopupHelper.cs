using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    public static class PopupHelper
    {
        public static AsyncProcessHandle Close(Transform transform, bool playAnimation = true)
        {
            var popupContainer = PopupContainer.Of(transform);
            return popupContainer.IsInTransition ? null : popupContainer.Pop(playAnimation);
        }
    }
}