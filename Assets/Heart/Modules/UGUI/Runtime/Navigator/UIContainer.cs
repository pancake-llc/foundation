using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.UI
{
    [DefaultExecutionOrder(-1)]
    public abstract class UIContainer : MonoBehaviour
    {
        private static readonly Dictionary<int, UIContainer> Cached = new();
        [SerializeField] private bool isDontDestroyOnLoad;

        #region Properties

        [field: SerializeField] public string ContainerName { get; private set; }
        [field: SerializeField] public EInstantiateType InstantiateType { get; private set; } = EInstantiateType.ByPrefab;
        [field: SerializeField] public ViewShowAnimation ShowAnimation { get; private set; } = new();
        [field: SerializeField] public ViewHideAnimation HideAnimation { get; private set; } = new();

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (isDontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the closest Container of the Transform received as an argument value.
        /// Basically, it is used for caching, and if there is no cached value, a new value is created and returned.
        /// Depending on the settings, you can decide whether to cache or not.
        /// </summary>
        /// <param name="transform"> Criteria to find Container Transform </param>
        /// <param name="useCache"> Whether to use caching </param>
        /// <returns></returns>
        public static UIContainer Of(Transform transform, bool useCache = true) => Of((RectTransform) transform, useCache);

        /// <summary>
        /// Returns the Container closest to the RectTransform received as the argument value.
        /// Basically, it is used for caching, and if there is no cached value, a new value is created and returned.
        /// Depending on the settings, you can decide whether to cache or not.
        /// </summary>
        /// <param name="rectTransform"> Criteria to find Container RectTransform </param>
        /// <param name="useCache"> Whether to use caching </param>
        /// <returns></returns>
        public static UIContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var hashCode = rectTransform.GetInstanceID();

            if (useCache && Cached.TryGetValue(hashCode, out var container))
            {
                return container;
            }

            container = rectTransform.GetComponentInParent<UIContainer>();
            if (container != null)
            {
                Cached.Add(hashCode, container);
                return container;
            }

            return null;
        }

        public static async UniTask<bool> BackAsync()
        {
            if (UIContext.FocusContext)
            {
                await ((IHasHistory) UIContext.FocusContext.UIContainer).PrevAsync();
                return true;
            }

            return false;
        }

        #endregion
    }

    public abstract class UIContainer<T> : UIContainer where T : UIContainer<T>
    {
        #region Fields

        private static readonly Dictionary<string, T> Containers = new();

        #endregion

        #region Properties

        public static T Main
        {
            get
            {
                var main = MainUIContainers.In.GetMain<T>();
                if (main) return main;

                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded)
                    {
                        main = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<T>()).FirstOrDefault(x => x);
                        MainUIContainers.In.SetMain(main);
                        if (main) return main;
                    }
                }

                return null;
            }
        }

        private static readonly Dictionary<int, T> Cached = new();

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            if (!string.IsNullOrEmpty(ContainerName))
            {
                Containers[ContainerName] = (T) this;
            }
        }

        protected virtual void OnDestroy() { Containers.Remove(ContainerName); }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the closest Container of the Transform received as an argument value.
        /// Basically, it is used for caching, and if there is no cached value, a new value is created and returned.
        /// Depending on the settings, you can decide whether to cache or not.
        /// </summary>
        /// <param name="transform"> Criteria to find Container Transform </param>
        /// <param name="useCache"> Whether to use caching </param>
        /// <returns></returns>
        public new static T Of(Transform transform, bool useCache = true) => Of((RectTransform) transform, useCache);

        /// <summary>
        /// Returns the Container closest to the RectTransform received as the argument value.
        /// Basically, it is used for caching, and if there is no cached value, a new value is created and returned.
        /// Depending on the settings, you can decide whether to cache or not.
        /// </summary>
        /// <param name="rectTransform"> Criteria to find Container RectTransform </param>
        /// <param name="useCache"> Whether to use caching </param>
        /// <returns></returns>
        public new static T Of(RectTransform rectTransform, bool useCache = true)
        {
            var hashCode = rectTransform.GetInstanceID();

            if (useCache && Cached.TryGetValue(hashCode, out var container))
            {
                return container;
            }

            container = rectTransform.GetComponentInParent<T>();
            if (container != null)
            {
                Cached.Add(hashCode, container);
                return container;
            }

            return null;
        }

        /// <summary>
        /// Finds and returns the Container with that name from the list of Containers whose names are cached as keys.
        /// The name is determined in the inspector.
        /// </summary>
        /// <param name="containerName">The name of the container you are looking for</param>
        /// <returns></returns>
        public static T Find(string containerName)
        {
            if (Containers.TryGetValue(containerName, out var container)) return container;
            DebugEditor.LogError($"Container with name {containerName} not found");
            return null;
        }

        #endregion
    }
}