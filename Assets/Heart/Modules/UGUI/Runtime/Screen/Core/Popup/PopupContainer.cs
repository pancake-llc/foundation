using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pancake.Apex;
using Pancake.AssetLoader;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectMask2D))]
    public class PopupContainer : GameComponent
    {
        private static readonly Dictionary<int, PopupContainer> InstanceCacheByTransform = new Dictionary<int, PopupContainer>();
        private static readonly Dictionary<string, PopupContainer> InstanceCacheByName = new Dictionary<string, PopupContainer>();

        [SerializeField, Label("Name")] private string displayName;
        [SerializeField] private PopupBackdrop overridePopupPrefab;

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _assetLoadHandles = new Dictionary<string, AssetLoadHandle<GameObject>>();
        private readonly List<PopupBackdrop> _backdrops = new List<PopupBackdrop>();
        private readonly List<IPopupContainerCallbackReceiver> _callbackReceivers = new List<IPopupContainerCallbackReceiver>();
        private readonly Dictionary<string, Popup> _popups = new Dictionary<string, Popup>();
        private readonly List<string> _orderedPopupIds = new List<string>();
        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles = new Dictionary<string, AssetLoadHandle<GameObject>>();
        private IAssetLoader _assetLoader;
        private PopupBackdrop _backdropPrefab;
        private CanvasGroup _canvasGroup;

        public static List<PopupContainer> Instances { get; } = new List<PopupContainer>();

        /// <summary>
        ///     By default, <see cref="IAssetLoader" /> in <see cref="DefaultTransitionSetting" /> is used.
        ///     If this property is set, it is used instead.
        /// </summary>
        public IAssetLoader AssetLoader { get => _assetLoader ?? DefaultTransitionSetting.AssetLoader; set => _assetLoader = value; }

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

            _backdropPrefab = overridePopupPrefab ? overridePopupPrefab : DefaultTransitionSetting.PopupBackdropPrefab;

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            foreach (var popupId in _orderedPopupIds)
            {
                var popup = _popups[popupId];
                var assetLoadHandle = _assetLoadHandles[popupId];

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

        /// <summary>
        ///     Push new popup.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="playAnimation"></param>
        /// <param name="popupId"></param>
        /// <param name="loadAsync"></param>
        /// <param name="onLoad"></param>
        /// <returns></returns>
        public AsyncProcessHandle Push(
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, Popup popup)> onLoad = null)
        {
            return App.StartCoroutine(PushRoutine(typeof(Popup),
                resourceKey,
                playAnimation,
                onLoad,
                loadAsync,
                popupId));
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
        public AsyncProcessHandle Push(
            Type popupType,
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, Popup popup)> onLoad = null)
        {
            return App.StartCoroutine(PushRoutine(popupType,
                resourceKey,
                playAnimation,
                onLoad,
                loadAsync,
                popupId));
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
        public AsyncProcessHandle Push<TPopup>(
            string resourceKey,
            bool playAnimation,
            string popupId = null,
            bool loadAsync = true,
            Action<(string popupId, TPopup popup)> onLoad = null) where TPopup : Popup
        {
            return App.StartCoroutine(PushRoutine(typeof(TPopup),
                resourceKey,
                playAnimation,
                x => onLoad?.Invoke((x.popupId, (TPopup) x.popup)),
                loadAsync,
                popupId));
        }

        /// <summary>
        ///     Pop popups.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <param name="popCount"></param>
        /// <returns></returns>
        public AsyncProcessHandle Pop(bool playAnimation, int popCount = 1) { return App.StartCoroutine(PopRoutine(playAnimation, popCount)); }

        /// <summary>
        ///     Pop popups.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <param name="destinationPopupId"></param>
        /// <returns></returns>
        public AsyncProcessHandle Pop(bool playAnimation, string destinationPopupId)
        {
            var popCount = 0;
            for (var i = _orderedPopupIds.Count - 1; i >= 0; i--)
            {
                var popupId = _orderedPopupIds[i];
                if (popupId == destinationPopupId) break;

                popCount++;
            }

            if (popCount == _orderedPopupIds.Count)
                throw new Exception($"The popup with id '{destinationPopupId}' is not found.");

            return App.StartCoroutine(PopRoutine(playAnimation, popCount));
        }

        private IEnumerator PushRoutine(
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

            if (!DefaultTransitionSetting.EnableInteractionInTransition)
            {
                if (DefaultTransitionSetting.ControlInteractionAllContainer)
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
            if (!assetLoadHandle.IsDone) yield return new WaitUntil(() => assetLoadHandle.IsDone);

            if (assetLoadHandle.Status == AssetLoadStatus.Failed) throw assetLoadHandle.OperationException;

            var backdrop = Instantiate(_backdropPrefab);
            backdrop.Setup((RectTransform) transform);
            _backdrops.Add(backdrop);

            var instance = Instantiate(assetLoadHandle.Result);
            if (!instance.TryGetComponent(popupType, out var c)) c = instance.AddComponent(popupType);
            var enterPopup = (Popup) c;

            if (popupId == null) popupId = Guid.NewGuid().ToString();
            _assetLoadHandles.Add(popupId, assetLoadHandle);
            onLoad?.Invoke((popupId, enterPopup));
            var afterLoadHandle = enterPopup.AfterLoad((RectTransform) transform);
            while (!afterLoadHandle.IsTerminated) yield return null;

            var exitPopupId = _orderedPopupIds.Count == 0 ? null : _orderedPopupIds[_orderedPopupIds.Count - 1];
            var exitPopup = exitPopupId == null ? null : _popups[exitPopupId];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforePush(enterPopup, exitPopup);

            var preprocessHandles = new List<AsyncProcessHandle>();
            if (exitPopup != null) preprocessHandles.Add(exitPopup.BeforeExit(true, enterPopup));

            preprocessHandles.Add(enterPopup.BeforeEnter(true, exitPopup));

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated) yield return coroutineHandle;
            }

            // Play Animation
            var animationHandles = new List<AsyncProcessHandle>();
            animationHandles.Add(backdrop.Enter(playAnimation));

            if (exitPopup != null) animationHandles.Add(exitPopup.Exit(true, playAnimation, enterPopup));

            animationHandles.Add(enterPopup.Enter(true, playAnimation, exitPopup));

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated) yield return coroutineHandle;
            }

            // End Transition
            _popups.Add(popupId, enterPopup);
            _orderedPopupIds.Add(popupId);
            IsInTransition = false;

            // Postprocess
            if (exitPopup != null) exitPopup.AfterExit(true, enterPopup);

            enterPopup.AfterEnter(true, exitPopup);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterPush(enterPopup, exitPopup);

            if (!DefaultTransitionSetting.EnableInteractionInTransition)
            {
                if (DefaultTransitionSetting.ControlInteractionAllContainer)
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

        private IEnumerator PopRoutine(bool playAnimation, int popCount = 1)
        {
            Assert.IsTrue(popCount >= 1);

            if (_orderedPopupIds.Count < popCount)
                throw new InvalidOperationException("Cannot transition because the popup count is less than the pop count.");

            if (IsInTransition)
                throw new InvalidOperationException("Cannot transition because the screen is already in transition.");

            IsInTransition = true;

            if (!DefaultTransitionSetting.EnableInteractionInTransition)
            {
                if (DefaultTransitionSetting.ControlInteractionAllContainer)
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
            var unusedBackdrops = new List<PopupBackdrop>();
            for (var i = _orderedPopupIds.Count - 1; i >= _orderedPopupIds.Count - popCount; i--)
            {
                var unusedPopupId = _orderedPopupIds[i];
                unusedPopupIds.Add(unusedPopupId);
                unusedPopups.Add(_popups[unusedPopupId]);
                unusedBackdrops.Add(_backdrops[i]);
            }

            var enterPopupIndex = _orderedPopupIds.Count - popCount - 1;
            var enterPopupId = enterPopupIndex < 0 ? null : _orderedPopupIds[enterPopupIndex];
            var enterPopup = enterPopupId == null ? null : _popups[enterPopupId];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforePop(enterPopup, exitPopup);

            var preprocessHandles = new List<AsyncProcessHandle> {exitPopup.BeforeExit(false, enterPopup)};
            if (enterPopup != null) preprocessHandles.Add(enterPopup.BeforeEnter(false, exitPopup));

            foreach (var coroutineHandle in preprocessHandles)
                while (!coroutineHandle.IsTerminated)
                    yield return coroutineHandle;

            // Play Animation
            var animationHandles = new List<AsyncProcessHandle>();
            for (var i = unusedPopupIds.Count - 1; i >= 0; i--)
            {
                var unusedPopupId = unusedPopupIds[i];
                var unusedPopup = _popups[unusedPopupId];
                var unusedBackdrop = unusedBackdrops[i];
                var partnerPopupId = i == 0 ? enterPopupId : unusedPopupIds[i - 1];
                var partnerPopup = partnerPopupId == null ? null : _popups[partnerPopupId];
                animationHandles.Add(unusedPopup.Exit(false, playAnimation, partnerPopup));
                animationHandles.Add(unusedBackdrop.Exit(playAnimation));
            }

            if (enterPopup != null) animationHandles.Add(enterPopup.Enter(false, playAnimation, exitPopup));

            foreach (var coroutineHandle in animationHandles)
                while (!coroutineHandle.IsTerminated)
                    yield return coroutineHandle;

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
            var beforeReleaseHandle = exitPopup.BeforeRelease();
            while (!beforeReleaseHandle.IsTerminated) yield return null;

            for (var i = 0; i < unusedPopupIds.Count; i++)
            {
                var unusedPopupId = unusedPopupIds[i];
                var unusedPopup = unusedPopups[i];
                var loadHandle = _assetLoadHandles[unusedPopupId];
                Destroy(unusedPopup.gameObject);
                AssetLoader.Release(loadHandle);
                _assetLoadHandles.Remove(unusedPopupId);
            }

            foreach (var unusedBackdrop in unusedBackdrops)
            {
                _backdrops.Remove(unusedBackdrop);
                Destroy(unusedBackdrop.gameObject);
            }

            if (!DefaultTransitionSetting.EnableInteractionInTransition)
            {
                if (DefaultTransitionSetting.ControlInteractionAllContainer)
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

        public AsyncProcessHandle Preload(string resourceKey, bool loadAsync = true) { return App.StartCoroutine(PreloadRoutine(resourceKey, loadAsync)); }

        private IEnumerator PreloadRoutine(string resourceKey, bool loadAsync = true)
        {
            if (_preloadedResourceHandles.ContainsKey(resourceKey))
                throw new InvalidOperationException($"The resource with key \"${resourceKey}\" has already been preloaded.");

            var assetLoadHandle = loadAsync ? AssetLoader.LoadAsync<GameObject>(resourceKey) : AssetLoader.Load<GameObject>(resourceKey);
            _preloadedResourceHandles.Add(resourceKey, assetLoadHandle);

            if (!assetLoadHandle.IsDone) yield return new WaitUntil(() => assetLoadHandle.IsDone);

            if (assetLoadHandle.Status == AssetLoadStatus.Failed) throw assetLoadHandle.OperationException;
        }

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