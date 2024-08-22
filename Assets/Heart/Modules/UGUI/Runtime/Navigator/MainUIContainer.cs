using Pancake.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("icon_popup")]
    public sealed class MainUIContainer : MonoBehaviour
    {
        [SerializeField] private PopupContainer popupContainer;
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private SheetContainer sheetContainer;

        private static MainUIContainer instance;

        public static MainUIContainer In =>
            instance = instance != null ? instance : FindFirstObjectByType<MainUIContainer>() ?? new GameObject(nameof(MainUIContainer)).AddComponent<MainUIContainer>();

        private void Awake()
        {
            if (instance) Destroy(gameObject);

            if (!popupContainer && !pageContainer && !sheetContainer)
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetActiveScene();
                    if (scene.isLoaded)
                    {
                        var rootObj = scene.GetRootGameObjects();
                        if (!popupContainer) popupContainer = rootObj.Map(r => r.GetComponentInChildren<PopupContainer>()).FirstOrDefault(x => x != null);
                        if (!pageContainer) pageContainer = rootObj.Map(r => r.GetComponentInChildren<PageContainer>()).FirstOrDefault(x => x != null);
                        if (!sheetContainer) sheetContainer = rootObj.Map(r => r.GetComponentInChildren<SheetContainer>()).FirstOrDefault(x => x != null);
                    }
                }
            }
        }

        public T GetMain<T>() where T : class, IUIContainer
        {
            if (typeof(T) == typeof(PopupContainer)) return popupContainer as T;
            if (typeof(T) == typeof(PageContainer)) return pageContainer as T;
            if (typeof(T) == typeof(SheetContainer)) return sheetContainer as T;
            return null;
        }

        public void SetMain<T>(T container) where T : class, IUIContainer
        {
            if (typeof(T) == typeof(PopupContainer)) popupContainer = container as PopupContainer;
            if (typeof(T) == typeof(PageContainer)) pageContainer = container as PageContainer;
            if (typeof(T) == typeof(SheetContainer)) sheetContainer = container as SheetContainer;
        }
    }
}