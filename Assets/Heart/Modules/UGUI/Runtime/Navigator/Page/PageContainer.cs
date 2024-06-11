using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Pancake.Linq;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Pancake.UI
{
    public sealed class PageContainer : UIContainer<PageContainer>, IHasHistory, ISerializationCallbackReceiver
    {
        #region Properties

        [field: SerializeField] public List<Page> RegisterPagesByPrefab { get; private set; } = new(); // List of pages that can be created in the container
#if PANCAKE_ADDRESSABLE
        [field: SerializeField] public List<ComponentReference<Page>> RegisterPagesByAddressable {get; private set; } =
 new(); // List of pages that can be created in the container
#endif
        [field: SerializeField] public bool HasDefault { get; private set; } // Whether to activate the initial sheet at startup
        internal Page DefaultPage { get; private set; }

        private Dictionary<Type, Page> Pages { get; set; }

        /// <summary>
        /// This is the History list of Page UI Views. <br/>
        /// History is managed in each container. <br/>
        /// </summary>
        private Stack<Page> History { get; } = new();

        public Page CurrentView => History.TryPeek(out var currentView) ? currentView : null;
        private bool IsRemainHistory => DefaultPage ? History.Count > 1 : History.Count > 0;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

#if PANCAKE_ADDRESSABLE
            if(InstantiateType == EInstantiateType.ByAddressable) RegisterPagesByPrefab =
 RegisterPagesByAddressable.Select(x => x.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Page>()).ToList();
#endif

            // Register all pages registered in pages in Dictionary format with Type as the key value
            RegisterPagesByPrefab = RegisterPagesByPrefab.Select(x => x.IsRecycle ? Instantiate(x, transform) : x)
                .GroupBy(x => x.GetType())
                .Select(x => x.FirstOrDefault())
                .ToList();
            Pages = RegisterPagesByPrefab.ToDictionary(page => page.GetType(), page => page);

            RegisterPagesByPrefab.Filter(x => x.IsRecycle)
                .ForEach(x =>
                {
                    x.UIContainer = this;
                    x.gameObject.SetActive(false);
                });

            if (HasDefault && RegisterPagesByPrefab.Any()) DefaultPage = Pages[RegisterPagesByPrefab.First().GetType()];
        }

        private void OnEnable()
        {
            if (DefaultPage && Pages.TryGetValue(DefaultPage.GetType(), out var nextPage))
            {
                if (CurrentView)
                {
                    CurrentView.HideAsync(false).Forget();
                    if (!CurrentView.IsRecycle) Destroy(CurrentView.gameObject);
                }

                nextPage = nextPage.IsRecycle ? nextPage : Instantiate(nextPage, transform);
                nextPage.ShowAsync(false).Forget();
                History.Push(nextPage);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if PANCAKE_ADDRESSABLE
            if(InstantiateType == EInstantiateType.ByAddressable) RegisterPagesByAddressable.ForEach(x => x.ReleaseAsset());
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activates the specified child page and stores it in History. <br/>
        /// The method of specifying a Page is to pass the type of the desired Page as a generic type. <br/>
        /// <br/>
        /// If there is a currently active Page, this method disables the previous Page and activates the new Page, and updates the FocusView. <br/>
        /// At this time, it is not executed when the existing Page is in a transition state. <br/>
        /// </summary>
        /// <typeparam name="T"> Type of Page to be activated </typeparam>
        /// <returns></returns>
        public async UniTask<T> NextAsync<T>(Action<T> onPreInitialize = null, Action<T> onPostInitialize = null) where T : Page
        {
            if (Pages.TryGetValue(typeof(T), out var page))
                return await NextAsync(page as T, onPreInitialize, onPostInitialize);

            Debug.LogError($"Page not found : {typeof(T)}");
            return null;
        }

        /// <summary>
        /// Activates the specified child page and stores it in History. <br/>
        /// The method of specifying a Page is to pass the type of the desired Page as a generic type. <br/>
        /// <br/>
        /// If there is a currently active Page, this method disables the previous Page and activates the new Page, and updates the FocusView. <br/>
        /// At this time, it is not executed when the existing Page is in a transition state. <br/>
        /// </summary>
        /// <param name="nextPageName"> Class name of the Page to be activated </param>
        /// <param name="onPreInitialize"></param>
        /// <param name="onPostInitialize"></param>
        /// <returns></returns>
        public async UniTask<Page> NextAsync(string nextPageName, Action<Page> onPreInitialize = null, Action<Page> onPostInitialize = null)
        {
            var page = Pages.Values.FirstOrDefault(x => x.GetType().Name == nextPageName);
            if (page != null) return await NextAsync(page, onPreInitialize, onPostInitialize);

            Debug.LogError($"Page not found : {nextPageName}");
            return null;
        }

        /// <summary>
        /// This is a method to terminate a specific UI View. <br/>
        /// When the corresponding UI View is terminated, all views activated after the corresponding view in the History are also terminated. <br/>
        /// <br/>
        /// Find the parent sheet that has the history of the corresponding UI View and compare the history step by step from the top to find the corresponding view. <br/>
        /// And put those views in the queue to be removed later. <br/>
        /// If the relevant View is found, the relevant View is removed. At this time, if the relevant UI View is a Sheet, <br/>
        /// Depending on whether ResetOnPop is set, the CurrentSheet of the Sheet's parent container is initialized to null or InitialSheet is set to CurrentSheet. <br/>
        /// <br/>
        /// When the specified UI View is terminated, all Views stored in the Queue are terminated and the Modal's Backdrop is additionally removed and the Modal is also destroyed. <br/>
        /// At this time, the reason for not using the PopRoutineAsync method is not only to remove it immediately, but also because PopRoutineAsync is not defined in the case of Sheet. <br/>
        /// </summary>
        public async UniTask<Page> NextAsync(Type nextPageType, Action<Page> onPreInitialize = null, Action<Page> onPostInitialize = null)
        {
            if (Pages.TryGetValue(nextPageType, out var page))
                return await NextAsync(page, onPreInitialize, onPostInitialize);

            Debug.LogError($"Page not found : {nextPageType.Name}");
            return null;
        }

        public async UniTask PrevAsync(int count = 1)
        {
            count = Mathf.Clamp(count, 1, History.Count);
            if (!IsRemainHistory) return;

            if (CurrentView.VisibleState is EVisibleState.Appearing or EVisibleState.Disappearing) return;

            CurrentView.HideAsync().Forget();

            for (int i = 0; i < count; i++)
            {
                if (!CurrentView.IsRecycle) Destroy(CurrentView.gameObject);
                History.Pop();
            }

            if (!CurrentView) return;

            await CurrentView.ShowAsync();
        }

        public async UniTask ResetAsync() { await PrevAsync(History.Count); }

        #endregion

        #region Private Methods

        private async UniTask<T> NextAsync<T>(T nextPage, Action<T> onPreInitialize, Action<T> onPostInitialize) where T : Page
        {
            if (CurrentView && CurrentView.VisibleState is EVisibleState.Appearing or EVisibleState.Disappearing) return null;
            if (CurrentView && CurrentView == nextPage) return null;

            nextPage.gameObject.SetActive(false);
            nextPage = nextPage.IsRecycle
                ? nextPage
                :
#if PANCAKE_VCONTAINER
                VContainerSettings.Instance.RootLifetimeScope.Container.Instantiate(nextPage, transform);
#else
                Instantiate(nextPage, transform);
#endif
            nextPage.UIContainer = this;

            nextPage.OnPreInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPreInitialize?.Invoke(nextPage)).AddTo(nextPage);
            nextPage.OnPostInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPostInitialize?.Invoke(nextPage)).AddTo(nextPage);

            if (CurrentView) CurrentView.HideAsync().Forget();

            History.Push(nextPage);

            await CurrentView.ShowAsync();

            return CurrentView as T;
        }

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable)
                RegisterPagesByPrefab.Clear();
            else
                RegisterPagesByAddressable.Clear();
#endif
        }

        #endregion
    }
}