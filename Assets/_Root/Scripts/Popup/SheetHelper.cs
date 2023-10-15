using Pancake.UI;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public static class SheetHelper
    {
        public static AsyncProcessHandle Close(Transform transform, bool playAnimation = true)
        {
            var sheetContainer = SheetContainer.Of(transform);
            return sheetContainer.IsInTransition ? null : sheetContainer.Hide(playAnimation);
        }
    }
}