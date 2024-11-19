using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Pancake.AssetLoader;
using Pancake.Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    [RequireComponent(typeof(RectMask2D))]
    [EditorIcon("icon_popup")]
    public class PopupContainer : MonoBehaviour, IUIContainer
    {
        private static readonly Dictionary<int, PopupContainer> InstanceCacheByTransform = new();
        private static readonly Dictionary<string, PopupContainer> InstanceCacheByName = new();

        [SerializeField, LabelText("Name")] private string displayName;
        [SerializeField] private bool overrideBackdrop;
        [SerializeField, ShowIf(nameof(overrideBackdrop)), Indent] private EPopupBackdropStrategy backdropStrategy = EPopupBackdropStrategy.GeneratePerPopup;
        [SerializeField, ShowIf(nameof(overrideBackdrop)), Indent] private PopupBackdrop backdropPrefab;

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _assetLoadHandles = new();
        private readonly List<IPopupContainerCallbackReceiver> _callbackReceivers = new();
        private readonly Dictionary<string, Popup> _popups = new();
        private readonly List<string> _orderedPopupIds = new();
        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles = new();
        private IAssetLoader _assetLoader;
        private CanvasGroup _canvasGroup;

        public static List<PopupContainer> Instances { get; } = new();

        private IPopupBackdropHandler _backdropHandler;

        /// <summary>
        ///     By default, <see cref="IAssetLoader" /> in <see cref="DefaultNavigatorSetting" /> is used.
        ///     If this property is set, it is used instead.
        /// </summary>
        public IAssetLoader AssetLoader { get => _assetLoader ?? DefaultNavigatorSetting.AssetLoader; set => _assetLoader = value; }

        /// <summary>
        ///     True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }

        /// <summary>
        ///     List of PopupIds sorted in the order they are stacked.
        /// </summary>
        public IReadOnlyList<string> OrderedPopupIds => _orderedPopupIds;

        /// <summary>
        ///     Map of PopupId to Popup.
        /// </summary>
        public IReadOnlyDictionary<string, Popup> Popups => _popups;

        public bool Interactable { get => _canvasGroup.interactable; set => _canvasGroup.interactable = value; }

        private void Awake()
        {
            Instances.Add(this);

            _callbackReceivers.AddRange(GetComponents<IPopupContainerCallbackReceiver>());
            if (!string.IsNullOrWhiteSpace(displayName)) InstanceCacheByName.Add(displayName, this);

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _backdropHandler = overrideBackdrop
                ? PopupBackdropHandlerFactory.Create(backdropStrategy, backdropPrefab)
                : PopupBackdropHandlerFactory.Create(DefaultNavigatorSetting.PopupBackdropStrategy, DefaultNavigatorSetting.PopupBackdropPrefab);
        }

        private void OnDestroy()
        {
            foreach (var popupId in _orderedPopupIds)
            {
                var popup = _popups[popupId];
                var assetLoadHandle = _assetLoadHandles[popupId];

#if PANCAKE_UNITASK
                if (DefaultNavigatorSetting.CallCleanupWhenDestroy) popup.BeforeRelease();
#endif
                Destroy(popup.gameObject);
                AssetLoader.Release(assetLoadHandle);
            }

            _assetLoadHandles.Clear();
            _orderedPopupIds.Clear();

            InstanceCacheByName.Remove(displayName);
            var keysToRemove = new List<int>();
            foreach (var cache in InstanceCacheByTransform)
            {
                if (Equals(cache.Value))
                    keysToRemove.Add(cache.Key);
            }

            foreach (var keyToRemove in keysToRemove) InstanceCacheByTransform.Remove(keyToRemove);

            Instances.Remove(this);
        }

        /// <summary>
        ///     Get the <see cref="PopupContainer" /> that manages the popup to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static PopupContainer Of(Transform transform, bool useCache = true) { return Of((RectTransform) transform, useCache); }

        /// <summary>
        ///     Get the <see cref="PopupContainer" /> that manages the popup to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static PopupContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();
            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container)) return container;

            container = rectTransform.GetComponentInParent<PopupContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        /// <summary>
        ///     Find the <see cref="PopupContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static PopupContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance)) return instance;

            return null;
        }

        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IPopupContainerCallbackReceiver callbackReceiver) { _callbackReceivers.Add(callbackReceiver); }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IPopupContainerCallbackReceiver callbackReceiver) { _callbackReceivers.Remove(callbackReceiver); }

#if PANCAKE_UNITASK
        /// <summary>
        ///     Push new popup.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="playAnimation"></param>
        /// <param name="popupId"></param>
        /// <param name="loadAsync"></param>
        /// <param name="onLoad"></param>
        /// <returns></returns>
        public async UniTask PushAsync(
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, Popup popup)> onLoad = null)
        {
            await Push(typeof(Popup),
                resourceKey,
                playAnimation,
                onLoad,
                loadAsync,
                popupId);
        }

        /// <summary>
        ///     Push new popup.
        /// </summary>
        /// <param name="popupType"></param>
        /// <param name="resourceKey"></param>
        /// <param name="playAnimation"></param>
        /// <param name="popupId"></param>
        /// <param name="loadAsync"></param>
        /// <param name="onLoad"></param>
        /// <returns></returns>
        public async UniTask PushAsync(
            Type popupType,
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, Popup popup)> onLoad = null)
        {
            await Push(popupType,
                resourceKey,
                playAnimation,
                onLoad,
                loadAsync,
                popupId);
        }

        /// <summary>
        ///     Push new popup.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="playAnimation"></param>
        /// <param name="popupId"></param>
        /// <param name="loadAsync"></param>
        /// <param name="onLoad"></param>
        /// <typeparam name="TPopup"></typeparam>
        /// <returns></returns>
        public async UniTask PushAsync<TPopup>(
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, TPopup popup)> onLoad = null) where TPopup : Popup
        {
            await Push(typeof(TPopup),
                resourceKey,
                playAnimation,
                x => onLoad?.Invoke((x.popupId, (TPopup) x.popup)),
                loadAsync,
                popupId);
        }

        /// <summary>
        ///     Pop popups.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <param name="popCount"></param>
        /// <returns></returns>
        public async UniTask PopAsync(bool playAnimation, int popCount = 1) { await Pop(playAnimation, popCount); }

        /// <summary>
        ///     Pop popups.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <param name="destinationPopupId"></param>
        /// <returns></returns>
        public async UniTask PopAsync(bool playAnimation, string destinationPopupId)
        {
            var popCount = 0;
            for (var i = _orderedPopupIds.Count - 1; i >= 0; i--)
            {
                var popupId = _orderedPopupIds[i];
                if (popupId == destinationPopupId) break;

                popCount++;
            }

            if (popCount == _orderedPopupIds.Count) throw new Exception($"The popup with id '{destinationPopupId}' is not found.");

            await Pop(playAnimation, popCount);
        }

        private async UniTask Push(
            Type popupType,
            string resourceKey,
            bool playAnimation,
            Action<(string popupId, Popup popup)> onLoad = null,
            bool loadAsync = true,
            string popupId = null)
        {
            if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

            if (IsInTransition) throw new InvalidOperationException("Cannot transition because the screen is already in transition.");

            IsInTransition = true;

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = false;
                    foreach (var popupContainer in Instances) popupContainer.Interactable = false;
                    foreach (var sheetContainer in SheetContainer.Instances) sheetContainer.Interactable = false;
                }
                else
                {
                    Interactable = false;
                }
            }

            var assetLoadHandle = loadAsync ? AssetLoader.LoadAsync<GameObject>(resourceKey) : AssetLoader.Load<GameObject>(resourceKey);
            while (!assetLoadHandle.IsDone)
            {
                await UniTask.Yield();
            }

            if (assetLoadHandle.Status == AssetLoadStatus.Failed) throw assetLoadHandle.OperationException;

            var instance = Instantiate(assetLoadHandle.Result);
            if (!instance.TryGetComponent(popupType, out var c)) c = instance.AddComponent(popupType);
            var enterPopup = (Popup) c;

            if (popupId == null) popupId = Guid.NewGuid().ToString();
            _assetLoadHandles.Add(popupId, assetLoadHandle);
            onLoad?.Invoke((popupId, enterPopup));
            await enterPopup.AfterLoadAsync((RectTransform) transform);

            var exitPopupId = _orderedPopupIds.Count == 0 ? null : _orderedPopupIds[_orderedPopupIds.Count - 1];
            var exitPopup = exitPopupId == null ? null : _popups[exitPopupId];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforePush(enterPopup, exitPopup);

            var preprocessHandles = new List<UniTask>();
            if (exitPopup != null) preprocessHandles.Add(exitPopup.BeforeExitAsync(true, enterPopup));

            preprocessHandles.Add(enterPopup.BeforeEnterAsync(true, exitPopup));

            foreach (var handle in preprocessHandles)
            {
                await handle;
            }

            // Play Animation
            int enterPopupIndex = _popups.Count;
            var animationHandles = new List<UniTask>();
            animationHandles.Add(_backdropHandler.BeforePopupEnterAsync(enterPopup, enterPopupIndex, playAnimation));

            if (exitPopup != null) animationHandles.Add(exitPopup.ExitAsync(true, playAnimation, enterPopup));

            animationHandles.Add(enterPopup.EnterAsync(true, playAnimation, exitPopup));

            foreach (var coroutineHandle in animationHandles)
            {
                await coroutineHandle;
            }

            _backdropHandler.AfterPopupEnter(enterPopup, enterPopupIndex, true);

            // End Transition
            _popups.Add(popupId, enterPopup);
            _orderedPopupIds.Add(popupId);
            IsInTransition = false;

            // Postprocess
            if (exitPopup != null) exitPopup.AfterExit(true, enterPopup);

            enterPopup.AfterEnter(true, exitPopup);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterPush(enterPopup, exitPopup);

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    // If there's a container in transition, it should restore Interactive to true when the transition is finished.
                    // So, do nothing here if there's a transitioning container.
                    if (PageContainer.Instances.All(x => !x.IsInTransition) && Instances.All(x => !x.IsInTransition) &&
                        SheetContainer.Instances.All(x => !x.IsInTransition))
                    {
                        foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = true;
                        foreach (var popupContainer in Instances) popupContainer.Interactable = true;
                        foreach (var sheetContainer in SheetContainer.Instances) sheetContainer.Interactable = true;
                    }
                }
                else
                {
                    Interactable = true;
                }
            }
        }

        private async UniTask Pop(bool playAnimation, int popCount = 1)
        {
            Assert.IsTrue(popCount >= 1);

            if (_orderedPopupIds.Count < popCount)
                throw new InvalidOperationException("Cannot transition because the popup count is less than the pop count.");

            if (IsInTransition)
                throw new InvalidOperationException("Cannot transition because the screen is already in transition.");

            IsInTransition = true;

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = false;
                    foreach (var popupContainer in Instances) popupContainer.Interactable = false;
                    foreach (var sheetContainer in SheetContainer.Instances) sheetContainer.Interactable = false;
                }
                else
                {
                    Interactable = false;
                }
            }

            var exitPopupId = _orderedPopupIds[_orderedPopupIds.Count - 1];
            var exitPopup = _popups[exitPopupId];

            var unusedPopupIds = new List<string>();
            var unusedPopups = new List<Popup>();
            var unusedPopupIndices = new List<int>();
            for (var i = _orderedPopupIds.Count - 1; i >= _orderedPopupIds.Count - popCount; i--)
            {
                var unusedPopupId = _orderedPopupIds[i];
                unusedPopupIds.Add(unusedPopupId);
                unusedPopups.Add(_popups[unusedPopupId]);
                unusedPopupIndices.Add(i);
            }

            var enterPopupIndex = _orderedPopupIds.Count - popCount - 1;
            var enterPopupId = enterPopupIndex < 0 ? null : _orderedPopupIds[enterPopupIndex];
            var enterPopup = enterPopupId == null ? null : _popups[enterPopupId];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforePop(enterPopup, exitPopup);

            var preprocessHandles = new List<UniTask> {exitPopup.BeforeExitAsync(false, enterPopup)};
            if (enterPopup != null) preprocessHandles.Add(enterPopup.BeforeEnterAsync(false, exitPopup));

            foreach (var handle in preprocessHandles)
            {
                await handle;
            }

            // Play Animation
            var animationHandles = new List<UniTask>();
            for (var i = unusedPopupIds.Count - 1; i >= 0; i--)
            {
                var unusedPopupId = unusedPopupIds[i];
                var unusedPopup = _popups[unusedPopupId];
                var unusedPopupIndex = unusedPopupIndices[i];
                var partnerPopupId = i == 0 ? enterPopupId : unusedPopupIds[i - 1];
                var partnerPopup = partnerPopupId == null ? null : _popups[partnerPopupId];
                animationHandles.Add(_backdropHandler.BeforePopupExitAsync(unusedPopup, unusedPopupIndex, playAnimation));
                animationHandles.Add(unusedPopup.ExitAsync(false, playAnimation, partnerPopup));
            }

            if (enterPopup != null) animationHandles.Add(enterPopup.EnterAsync(false, playAnimation, exitPopup));

            foreach (var handle in animationHandles)
            {
                await handle;
            }

            // End Transition
            for (var i = 0; i < unusedPopupIds.Count; i++)
            {
                var unusedPopupId = unusedPopupIds[i];
                _popups.Remove(unusedPopupId);
                _orderedPopupIds.RemoveAt(_orderedPopupIds.Count - 1);
            }

            IsInTransition = false;

            // Postprocess
            exitPopup.AfterExit(false, enterPopup);
            if (enterPopup != null) enterPopup.AfterEnter(false, exitPopup);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterPop(enterPopup, exitPopup);

            // Unload Unused Page
            await exitPopup.BeforeReleaseAsync();

            for (var i = 0; i < unusedPopupIds.Count; i++)
            {
                var unusedPopupId = unusedPopupIds[i];
                var unusedPopup = unusedPopups[i];
                var unusedPopupIndex = unusedPopupIndices[i];
                var loadHandle = _assetLoadHandles[unusedPopupId];
                Destroy(unusedPopup.gameObject);
                AssetLoader.Release(loadHandle);
                _assetLoadHandles.Remove(unusedPopupId);
                _backdropHandler.AfterPopupExit(exitPopup, unusedPopupIndex, playAnimation);
            }

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    // If there's a container in transition, it should restore Interactive to true when the transition is finished.
                    // So, do nothing here if there's a transitioning container.
                    if (PageContainer.Instances.All(x => !x.IsInTransition) && Instances.All(x => !x.IsInTransition) &&
                        SheetContainer.Instances.All(x => !x.IsInTransition))
                    {
                        foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = true;
                        foreach (var popupContainer in Instances) popupContainer.Interactable = true;
                        foreach (var sheetContainer in SheetContainer.Instances) sheetContainer.Interactable = true;
                    }
                }
                else
                {
                    Interactable = true;
                }
            }
        }

        public async UniTask PreloadAsync(string resourceKey, bool loadAsync = true)
        {
            if (_preloadedResourceHandles.ContainsKey(resourceKey))
                throw new InvalidOperationException($"The resource with key \"${resourceKey}\" has already been preloaded.");

            var assetLoadHandle = loadAsync ? AssetLoader.LoadAsync<GameObject>(resourceKey) : AssetLoader.Load<GameObject>(resourceKey);
            _preloadedResourceHandles.Add(resourceKey, assetLoadHandle);

            while (!assetLoadHandle.IsDone)
            {
                await UniTask.Yield();
            }

            if (assetLoadHandle.Status == AssetLoadStatus.Failed) throw assetLoadHandle.OperationException;
        }
#endif

        public bool IsPreloadRequested(string resourceKey) { return _preloadedResourceHandles.ContainsKey(resourceKey); }

        public bool IsPreloaded(string resourceKey)
        {
            if (!_preloadedResourceHandles.TryGetValue(resourceKey, out var handle)) return false;

            return handle.Status == AssetLoadStatus.Success;
        }

        public void ReleasePreloaded(string resourceKey)
        {
            if (!_preloadedResourceHandles.ContainsKey(resourceKey))
                throw new InvalidOperationException($"The resource with key \"${resourceKey}\" is not preloaded.");

            var handle = _preloadedResourceHandles[resourceKey];
            AssetLoader.Release(handle);
        }
    }
}