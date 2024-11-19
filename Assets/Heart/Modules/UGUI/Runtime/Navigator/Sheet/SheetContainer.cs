using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Pancake.AssetLoader;
using Pancake.Common;
using UnityEngine;
using UnityEngine.UI;

#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    [RequireComponent(typeof(RectMask2D))]
    [EditorIcon("icon_popup")]
    public sealed class SheetContainer : MonoBehaviour, IUIContainer
    {
        private static readonly Dictionary<int, SheetContainer> InstanceCacheByTransform = new();
        private static readonly Dictionary<string, SheetContainer> InstanceCacheByName = new();

        [SerializeField, LabelText("Name")] private string displayName;

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _assetLoadHandles = new();
        private readonly List<ISheetContainerCallbackReceiver> _callbackReceivers = new();
        private readonly Dictionary<string, string> _sheetNameToId = new();
        private readonly Dictionary<string, Sheet> _sheets = new();
        private IAssetLoader _assetLoader;
        private CanvasGroup _canvasGroup;

        public static List<SheetContainer> Instances { get; } = new();

        /// <summary>
        ///     By default, <see cref="IAssetLoader" /> in <see cref="DefaultNavigatorSetting" /> is used.
        ///     If this property is set, it is used instead.
        /// </summary>
        public IAssetLoader AssetLoader { get => _assetLoader ?? DefaultNavigatorSetting.AssetLoader; set => _assetLoader = value; }

        public string ActiveSheetId { get; private set; }

        public Sheet ActiveSheet
        {
            get
            {
                if (ActiveSheetId == null) return null;

                return _sheets[ActiveSheetId];
            }
        }

        /// <summary>
        ///     True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }

        /// <summary>
        ///     Registered sheets.
        /// </summary>
        public IReadOnlyDictionary<string, Sheet> Sheets => _sheets;

        public bool Interactable { get => _canvasGroup.interactable; set => _canvasGroup.interactable = value; }

        private void Awake()
        {
            Instances.Add(this);

            _callbackReceivers.AddRange(GetComponents<ISheetContainerCallbackReceiver>());

            if (!string.IsNullOrWhiteSpace(displayName)) InstanceCacheByName.Add(displayName, this);
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            UnregisterAll();

            InstanceCacheByName.Remove(displayName);
            var keysToRemove = new List<int>();
            foreach (var cache in InstanceCacheByTransform)
            {
                if (Equals(cache.Value)) keysToRemove.Add(cache.Key);
            }

            foreach (var keyToRemove in keysToRemove) InstanceCacheByTransform.Remove(keyToRemove);

            Instances.Remove(this);
        }

        /// <summary>
        ///     Get the <see cref="SheetContainer" /> that manages the sheet to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static SheetContainer Of(Transform transform, bool useCache = true) { return Of((RectTransform) transform, useCache); }

        /// <summary>
        ///     Get the <see cref="SheetContainer" /> that manages the sheet to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static SheetContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var hashCode = rectTransform.GetInstanceID();

            if (useCache && InstanceCacheByTransform.TryGetValue(hashCode, out var container)) return container;

            container = rectTransform.GetComponentInParent<SheetContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(hashCode, container);
                return container;
            }

            return null;
        }

        /// <summary>
        ///     Find the <see cref="SheetContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static SheetContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance)) return instance;

            return null;
        }

        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(ISheetContainerCallbackReceiver callbackReceiver) { _callbackReceivers.Add(callbackReceiver); }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(ISheetContainerCallbackReceiver callbackReceiver) { _callbackReceivers.Remove(callbackReceiver); }

#if PANCAKE_UNITASK
        /// <summary>
        ///     Show a sheet.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public async UniTask ShowByResourceKeyAsync(string resourceKey, bool playAnimation) { await ShowByResourceKey(resourceKey, playAnimation); }

        /// <summary>
        ///     Register a sheet.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="onLoad"></param>
        /// <param name="loadAsync"></param>
        /// <param name="sheetId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async UniTask<string> RegisterAsync(string resourceKey, Action<(string sheetId, Sheet sheet)> onLoad = null, bool loadAsync = true, string sheetId = null)
        {
            string id = await Register(typeof(Sheet),
                resourceKey,
                onLoad,
                loadAsync,
                sheetId);
            return id;
        }

        /// <summary>
        ///     Register a sheet.
        /// </summary>
        /// <param name="sheetType"></param>
        /// <param name="resourceKey"></param>
        /// <param name="onLoad"></param>
        /// <param name="loadAsync"></param>
        /// <param name="sheetId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async UniTask<string> RegisterAsync(
            Type sheetType,
            string resourceKey,
            Action<(string sheetId, Sheet sheet)> onLoad = null,
            bool loadAsync = true,
            string sheetId = null)
        {
            string id = await Register(sheetType,
                resourceKey,
                onLoad,
                loadAsync,
                sheetId);
            return id;
        }

        /// <summary>
        ///     Register a sheet.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="onLoad"></param>
        /// <param name="loadAsync"></param>
        /// <param name="sheetId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async UniTask<string> RegisterAsync<TSheet>(
            string resourceKey,
            Action<(string sheetId, TSheet sheet)> onLoad = null,
            bool loadAsync = true,
            string sheetId = null) where TSheet : Sheet
        {
            string id = await Register(typeof(TSheet),
                resourceKey,
                x => onLoad?.Invoke((x.sheetId, (TSheet) x.sheet)),
                loadAsync,
                sheetId);
            return id;
        }

        private async UniTask<string> Register(
            Type sheetType,
            string resourceKey,
            Action<(string sheetId, Sheet sheet)> onLoad = null,
            bool loadAsync = true,
            string sheetId = null)
        {
            if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

            var assetLoadHandle = loadAsync ? AssetLoader.LoadAsync<GameObject>(resourceKey) : AssetLoader.Load<GameObject>(resourceKey);
            while (!assetLoadHandle.IsDone) await UniTask.Yield();

            if (assetLoadHandle.Status == AssetLoadStatus.Failed) throw assetLoadHandle.OperationException;

            var instance = Instantiate(assetLoadHandle.Result);
            if (!instance.TryGetComponent(sheetType, out var c))
                c = instance.AddComponent(sheetType);
            var sheet = (Sheet) c;

            sheetId ??= Guid.NewGuid().ToString();
            _sheets.Add(sheetId, sheet);
            _sheetNameToId[resourceKey] = sheetId;
            _assetLoadHandles.Add(sheetId, assetLoadHandle);
            onLoad?.Invoke((sheetId, sheet));
            await sheet.AfterLoadAsync((RectTransform) transform);

            return sheetId;
        }

        private async UniTask ShowByResourceKey(string resourceKey, bool playAnimation)
        {
            string sheetId = _sheetNameToId[resourceKey];
            await ShowAsync(sheetId, playAnimation);
        }

        /// <summary>
        ///     Show a sheet.
        /// </summary>
        /// <param name="sheetId"></param>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public async UniTask ShowAsync(string sheetId, bool playAnimation)
        {
            if (IsInTransition) throw new InvalidOperationException("Cannot transition because the screen is already in transition.");

            if (ActiveSheetId != null && ActiveSheetId == sheetId) throw new InvalidOperationException("Cannot transition because the sheet is already active.");

            IsInTransition = true;

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = false;
                    foreach (var popupContainer in PopupContainer.Instances) popupContainer.Interactable = false;
                    foreach (var sheetContainer in Instances) sheetContainer.Interactable = false;
                }
                else
                {
                    Interactable = false;
                }
            }

            var enterSheet = _sheets[sheetId];
            var exitSheet = ActiveSheetId != null ? _sheets[ActiveSheetId] : null;

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforeShow(enterSheet, exitSheet);

            var preprocessHandles = new List<UniTask>();
            if (exitSheet != null) preprocessHandles.Add(exitSheet.BeforeExitAsync(enterSheet));

            preprocessHandles.Add(enterSheet.BeforeEnterAsync(exitSheet));
            foreach (var handle in preprocessHandles)
            {
                await handle;
            }

            // Play Animation
            var animationHandles = new List<UniTask>();
            if (exitSheet != null) animationHandles.Add(exitSheet.ExitAsync(playAnimation, enterSheet));

            animationHandles.Add(enterSheet.EnterAsync(playAnimation, exitSheet));

            foreach (var handle in animationHandles)
            {
                await handle;
            }

            // End Transition
            ActiveSheetId = sheetId;
            IsInTransition = false;

            // Postprocess
            if (exitSheet != null) exitSheet.AfterExit(enterSheet);

            enterSheet.AfterEnter(exitSheet);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterShow(enterSheet, exitSheet);

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    // If there's a container in transition, it should restore Interactive to true when the transition is finished.
                    // So, do nothing here if there's a transitioning container.
                    if (PageContainer.Instances.All(x => !x.IsInTransition) && PopupContainer.Instances.All(x => !x.IsInTransition) &&
                        Instances.All(x => !x.IsInTransition))
                    {
                        foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = true;
                        foreach (var popupContainer in PopupContainer.Instances) popupContainer.Interactable = true;
                        foreach (var sheetContainer in Instances) sheetContainer.Interactable = true;
                    }
                }
                else
                {
                    Interactable = true;
                }
            }
        }

        /// <summary>
        ///     Hide a sheet.
        /// </summary>
        /// <param name="playAnimation"></param>
        public async UniTask HideAsync(bool playAnimation)
        {
            if (IsInTransition) throw new InvalidOperationException("Cannot transition because the screen is already in transition.");

            if (ActiveSheetId == null) throw new InvalidOperationException("Cannot transition because there is no active sheets.");

            IsInTransition = true;

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = false;
                    foreach (var popupContainer in PopupContainer.Instances) popupContainer.Interactable = false;
                    foreach (var sheetContainer in Instances) sheetContainer.Interactable = false;
                }
                else
                {
                    Interactable = false;
                }
            }

            var exitSheet = _sheets[ActiveSheetId];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforeHide(exitSheet);

            await exitSheet.BeforeExitAsync(null);

            // Play Animation
            await exitSheet.ExitAsync(playAnimation, null);

            // End Transition
            ActiveSheetId = null;
            IsInTransition = false;

            // Postprocess
            exitSheet.AfterExit(null);
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterHide(exitSheet);

            if (!DefaultNavigatorSetting.EnableInteractionInTransition)
            {
                if (DefaultNavigatorSetting.ControlInteractionAllContainer)
                {
                    // If there's a container in transition, it should restore Interactive to true when the transition is finished.
                    // So, do nothing here if there's a transitioning container.
                    if (PageContainer.Instances.All(x => !x.IsInTransition) && PopupContainer.Instances.All(x => !x.IsInTransition) &&
                        Instances.All(x => !x.IsInTransition))
                    {
                        foreach (var pageContainer in PageContainer.Instances) pageContainer.Interactable = true;
                        foreach (var popupContainer in PopupContainer.Instances) popupContainer.Interactable = true;
                        foreach (var sheetContainer in Instances) sheetContainer.Interactable = true;
                    }
                }
                else
                {
                    Interactable = true;
                }
            }
        }
#endif

        /// <summary>
        ///     Destroy and release all sheets.
        /// </summary>
        public void UnregisterAll()
        {
            foreach (var sheet in _sheets.Values)
            {
#if PANCAKE_UNITASK
                if (DefaultNavigatorSetting.CallCleanupWhenDestroy) sheet.BeforeRelease();
#endif
                Destroy(sheet.gameObject);
            }

            foreach (var assetLoadHandle in _assetLoadHandles.Values) AssetLoader.Release(assetLoadHandle);

            _assetLoadHandles.Clear();
        }
    }
}