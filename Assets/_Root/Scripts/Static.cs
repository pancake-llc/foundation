using System.Collections.Generic;
using Pancake.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Pancake.SceneFlow
{
    public static class Static
    {
        public static Dictionary<string, AsyncOperationHandle<SceneInstance>> sceneHolder = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

        public static PageContainer MainPageContainer => PageContainer.Find(Constant.MAIN_PAGE_CONTAINER_ID);
        public static PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER_ID);
    }
}