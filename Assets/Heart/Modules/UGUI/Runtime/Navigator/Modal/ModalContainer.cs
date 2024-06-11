using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Pancake.UI
{
    public sealed class ModalContainer : UIContainer<ModalContainer>, IHasHistory, ISerializationCallbackReceiver
    {
        #region Fields

        [SerializeField] private Backdrop modalBackdrop; // Layer to be placed behind the created modal

        #endregion

        #region Properties

        [field: SerializeField] public List<Modal> RegisterModalsByPrefab { get; private set; } = new(); // List of pages that can be created in the container
#if PANCAKE_ADDRESSABLE
        [field: SerializeField] public List<ComponentReference<Modal>> RegisterModalsByAddressable { get; private set; } =
 new(); // List of pages that can be created in the container
#endif
        private Dictionary<Type, Modal> Modals { get; set; }

        /// <summary>
        /// This is the History list of Page UI Views. <br/>
        /// History is managed in each container. <br/>
        /// </summary>
        private Stack<Modal> History { get; } = new();

        public Modal CurrentView => History.TryPeek(out var currentView) ? currentView : null;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

#if PANCAKE_ADDRESSABLE
            if(InstantiateType == EInstantiateType.ByAddressable) RegisterModalsByPrefab =
 RegisterModalsByAddressable.Select(x => x.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Modal>()).ToList();
#endif

            // Register all modals registered in modals in Dictionary format with Type as the key value
            Modals = RegisterModalsByPrefab.GroupBy(x => x.GetType()).Select(x => x.FirstOrDefault()).ToDictionary(modal => modal.GetType(), modal => modal);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable) RegisterModalsByAddressable.ForEach(x => x.ReleaseAsset());
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the specified child Modal and stores it in History. <br/>
        /// The method of specifying a Modal is to pass the type of the desired Modal as a generic type. <br/>
        /// <br/>
        /// This method creates a new modal while leaving the existing modal as is and updates the FocusView. <br/>
        /// At this time, it is not executed when an existing modal is being created. <br/>
        /// </summary>
        /// <typeparam name="T"> Type of Modal to be created </typeparam>
        /// <returns></returns>
        public async UniTask<T> NextAsync<T>(Action<T> onPreInitialize = null, Action<T> onPostInitialize = null) where T : Modal
        {
            if (Modals.TryGetValue(typeof(T), out var modal))
                return await NextAsync(modal as T, onPreInitialize, onPostInitialize);

            DebugEditor.LogError($"Modal not found : {typeof(T)}");
            return null;
        }

        /// <summary>
        /// Creates the specified child Modal and stores it in History. <br/>
        /// The method of specifying a Modal is to pass the type of the desired Modal as a generic type. <br/>
        /// <br/>
        /// This method creates a new modal while leaving the existing modal as is and updates the FocusView. <br/>
        /// At this time, it is not executed when an existing modal is being created. <br/>
        /// </summary>
        /// <param name="nextModalName"> Class name of Modal to be created </param>
        /// <returns></returns>
        public async UniTask<Modal> NextAsync(string nextModalName, Action<Modal> onPreInitialize = null, Action<Modal> onPostInitialize = null)
        {
            var modal = Modals.Values.FirstOrDefault(x => x.GetType().Name == nextModalName);
            if (modal != null) return await NextAsync(modal, onPreInitialize, onPostInitialize);

            DebugEditor.LogError($"Modal not found : {nextModalName}");
            return null;
        }

        public async UniTask<Modal> NextAsync(Type nextModalType, Action<Modal> onPreInitialize = null, Action<Modal> onPostInitialize = null)
        {
            if (Modals.TryGetValue(nextModalType, out var modal))
                return await NextAsync(modal, onPreInitialize, onPostInitialize);

            DebugEditor.LogError($"Modal not found : {nextModalType.Name}");
            return null;
        }

        public async UniTask PrevAsync(int count = 1)
        {
            count = Mathf.Clamp(count, 1, History.Count);

            if (!CurrentView) return;
            if (CurrentView.VisibleState is EVisibleState.Appearing or EVisibleState.Disappearing) return;

            await UniTask.WhenAll(Enumerable.Range(0, count).Select(_ => HideViewAsync()));
        }

        private async UniTask HideViewAsync()
        {
            var currentView = History.Pop();
            if (currentView.BackDrop)
            {
                await UniTask.WhenAll(LMotion.Create(currentView.BackDrop.alpha, 0, 0.2f).BindToCanvasGroupAlpha(currentView.BackDrop).ToUniTask(),
                    currentView.HideAsync());
            }
            else await currentView.HideAsync();

            if (currentView.BackDrop) Destroy(currentView.BackDrop.gameObject);
            Destroy(currentView.gameObject);
        }

        #endregion

        #region Private Methods

        private async UniTask<T> NextAsync<T>(T nextModal, Action<T> onPreInitialize, Action<T> onPostInitialize) where T : Modal
        {
            if (CurrentView != null && CurrentView.VisibleState is EVisibleState.Appearing or EVisibleState.Disappearing) return null;

            var backdrop = await ShowBackdrop();

            nextModal.gameObject.SetActive(false);
            nextModal =
#if PANCAKE_VCONTAINER
                VContainerSettings.Instance.RootLifetimeScope.Container.Instantiate(nextModal, transform);
#else
                Instantiate(nextModal, transform);
#endif

            nextModal.UIContainer = this;

            nextModal.OnPreInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPreInitialize?.Invoke(nextModal)).AddTo(nextModal);
            nextModal.OnPostInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPostInitialize?.Invoke(nextModal)).AddTo(nextModal);

            if (backdrop)
            {
                nextModal.BackDrop = backdrop;
                if (!nextModal.BackDrop.TryGetComponent<Button>(out var button))
                    button = nextModal.BackDrop.gameObject.AddComponent<Button>();

                button.OnClickAsObservable().Subscribe(_ => PrevAsync().Forget());
            }

            History.Push(nextModal);

#pragma warning disable 4014
            if (nextModal.BackDrop) LMotion.Create(CurrentView.BackDrop.alpha, 1, 0.2f).BindToCanvasGroupAlpha(CurrentView.BackDrop);
#pragma warning restore 4014

            await CurrentView.ShowAsync();

            return CurrentView as T;
        }

        private async UniTask<CanvasGroup> ShowBackdrop()
        {
            if (!modalBackdrop) return null;

            var backdrop = Instantiate(modalBackdrop.gameObject, transform, true);
            if (!backdrop.TryGetComponent<CanvasGroup>(out var canvasGroup))
                canvasGroup = backdrop.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            var rectTransform = (RectTransform) backdrop.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            await UniTask.Yield();
            rectTransform.anchoredPosition = Vector2.zero;
            return canvasGroup;
        }

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable)
                RegisterModalsByPrefab.Clear();
            else
                RegisterModalsByAddressable.Clear();
#endif
        }

        #endregion
    }
}