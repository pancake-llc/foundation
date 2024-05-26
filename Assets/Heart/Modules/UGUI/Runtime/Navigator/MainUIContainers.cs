using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.UI
{
    public sealed class MainUIContainers : MonoBehaviour
    {
        #region Field

        private static MainUIContainers instance;
        [SerializeField] private SheetContainer mainSheetContainer;
        [SerializeField] private PageContainer mainPageContainer;
        [SerializeField] private ModalContainer mainModalContainer;

        #endregion

        #region Property

        public static MainUIContainers In =>
            instance = instance ? instance : FindFirstObjectByType<MainUIContainers>() ?? new GameObject(nameof(MainUIContainers)).AddComponent<MainUIContainers>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance) Destroy(gameObject);

            if (!mainSheetContainer && !mainPageContainer && !mainModalContainer)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded)
                    {
                        if (!mainSheetContainer)
                            mainSheetContainer = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<SheetContainer>()).FirstOrDefault(x => x);
                        if (!mainPageContainer)
                            mainPageContainer = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<PageContainer>()).FirstOrDefault(x => x);
                        if (!mainModalContainer)
                            mainModalContainer = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<ModalContainer>()).FirstOrDefault(x => x);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public T GetMain<T>() where T : UIContainer<T>
        {
            if (typeof(T) == typeof(SheetContainer)) return mainSheetContainer as T;
            if (typeof(T) == typeof(PageContainer)) return mainPageContainer as T;
            if (typeof(T) == typeof(ModalContainer)) return mainModalContainer as T;
            return null;
        }

        public void SetMain<T>(T container) where T : UIContainer<T>
        {
            if (typeof(T) == typeof(SheetContainer)) mainSheetContainer = container as SheetContainer;
            if (typeof(T) == typeof(PageContainer)) mainPageContainer = container as PageContainer;
            if (typeof(T) == typeof(ModalContainer)) mainModalContainer = container as ModalContainer;
        }

        #endregion
    }
}