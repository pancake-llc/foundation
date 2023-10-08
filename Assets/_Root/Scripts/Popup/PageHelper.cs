using Pancake.UI;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public static class PageHelper
    {
        public static AsyncProcessHandle Close(Transform transform, bool playAnimation = true)
        {
            var pageContainer = PageContainer.Of(transform);
            return pageContainer.Pop(playAnimation);
        }
    }
}