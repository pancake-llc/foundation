using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Pancake.UI
{
    public sealed class SheetContainer : UIContainer<SheetContainer>, ISerializationCallbackReceiver
    {
        #region Properties

        [field: SerializeField] public List<Sheet> RegisterSheetsByPrefab { get; private set; } = new(); // List of sheets that can be created in the container
#if PANCAKE_ADDRESSABLE
        [field: SerializeField] public List<ComponentReference<Sheet>> RegisterSheetsByAddressable { get; private set; } =
            new(); // List of sheets that can be created in the container
#endif
        [field: SerializeField] public bool HasDefault { get; private set; } // Whether to activate the initial sheet at startup
        private Dictionary<Type, Sheet> Sheets { get; set; }

        public Sheet CurrentView { get; private set; }

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable)
                RegisterSheetsByPrefab = RegisterSheetsByAddressable.Select(x => x.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Sheet>()).ToList();
#endif

            // Register all sheets registered in sheets in Dictionary format with Type as the key value
            RegisterSheetsByPrefab = RegisterSheetsByPrefab.Select(x => x.IsRecycle ? Instantiate(x, transform) : x)
                .GroupBy(x => x.GetType())
                .Select(x => x.FirstOrDefault())
                .ToList();
            Sheets = RegisterSheetsByPrefab.ToDictionary(sheet => sheet.GetType(), sheet => sheet);

            RegisterSheetsByPrefab.Filter(x => x.IsRecycle)
                .ForEach(x =>
                {
                    x.UIContainer = this;
                    x.gameObject.SetActive(false);
                });
        }

        private void OnEnable()
        {
            // Activate initial sheet
            if (HasDefault && RegisterSheetsByPrefab.Any())
            {
                var nextSheet = Sheets[RegisterSheetsByPrefab.First().GetType()];

                if (CurrentView)
                {
                    CurrentView.HideAsync(false).Forget();
                    if (!CurrentView.IsRecycle) Destroy(CurrentView.gameObject);
                }

                CurrentView = nextSheet.IsRecycle ? nextSheet : Instantiate(nextSheet, transform);
                CurrentView.ShowAsync(false).Forget();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable) RegisterSheetsByAddressable.ForEach(x => x.ReleaseAsset());
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Activates the specified child sheet. <br/>
        /// The method of specifying a Sheet is to pass the desired Sheet type as a generic type. <br/>
        /// <br/>
        /// If there is a currently active Sheet, this method deactivates the previous Sheet and activates a new Sheet, and updates the FocusView. <br/>
        /// At this time, it is not executed when the existing sheet is being converted. <br/>
        /// <br/>
        /// If resetOnChangeSheet is true, the history of the previous sheet is initialized. <br/>
        /// </summary>
        /// <typeparam name="T"> Type of Sheet to be activated </typeparam>
        /// <returns></returns>
        public async UniTask<T> NextAsync<T>(Action<T> onPreInitialize = null, Action<T> onPostInitialize = null) where T : Sheet
        {
            if (Sheets.TryGetValue(typeof(T), out var sheet))
                return await NextAsync(sheet as T, onPreInitialize, onPostInitialize);

            Debug.LogError($"Sheet not found : {typeof(T)}");
            return null;
        }

        /// <summary>
        /// Activates the specified child sheet. <br/>
        /// <br/>
        /// If there is a currently active Sheet, this method deactivates the previous Sheet and activates a new Sheet, and updates the FocusView. <br/>
        /// At this time, it is not executed when the existing sheet is being converted. <br/>
        /// <br/>
        /// If resetOnChangeSheet is true, the history of the previous sheet is initialized. <br/>
        /// </summary>
        /// <param name="sheetName"> Class name of the sheet to be activated </param>
        /// <param name="onPreInitialize"></param>
        /// <param name="onPostInitialize"></param>
        /// <returns></returns>
        public async UniTask<Sheet> NextAsync(string sheetName, Action<Sheet> onPreInitialize = null, Action<Sheet> onPostInitialize = null)
        {
            var sheet = Sheets.Values.FirstOrDefault(x => x.GetType().Name == sheetName);
            if (sheet != null) return await NextAsync(sheet, onPreInitialize, onPostInitialize);

            Debug.LogError($"Sheet not found : {sheetName}");
            return null;
        }

        public async UniTask<Sheet> NextAsync(Type targetSheet, Action<Sheet> onPreInitialize = null, Action<Sheet> onPostInitialize = null)
        {
            if (Sheets.TryGetValue(targetSheet, out var nextSheet))
                return await NextAsync(nextSheet, onPreInitialize, onPostInitialize);

            Debug.LogError("Sheet not found");
            return null;
        }

        #endregion

        #region Private Methods

        private async UniTask<T> NextAsync<T>(T nextSheet, Action<T> onPreInitialize, Action<T> onPostInitialize) where T : Sheet
        {
            if (CurrentView != null && CurrentView.VisibleState is EVisibleState.Appearing or EVisibleState.Disappearing) return null;
            if (CurrentView != null && CurrentView == nextSheet) return null;

            var prevSheet = CurrentView;

            nextSheet.gameObject.SetActive(false);
            nextSheet = nextSheet.IsRecycle
                ? nextSheet
                :
#if PANCAKE_VCONTAINER
                VContainerSettings.Instance.RootLifetimeScope.Container.Instantiate(nextSheet, transform);
#else
                Instantiate(nextSheet, transform);
#endif

            nextSheet.UIContainer = this;

            nextSheet.OnPreInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPreInitialize?.Invoke(nextSheet)).AddTo(nextSheet);
            nextSheet.OnPostInitialize.Take(1).DefaultIfEmpty().Subscribe(_ => onPostInitialize?.Invoke(nextSheet)).AddTo(nextSheet);

            CurrentView = nextSheet;

            if (prevSheet != null)
            {
                prevSheet.HideAsync().Forget();
                if (!prevSheet.IsRecycle) Destroy(prevSheet.gameObject);
            }

            await CurrentView.ShowAsync();

            return CurrentView as T;
        }

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
#if PANCAKE_ADDRESSABLE
            if (InstantiateType == EInstantiateType.ByAddressable) RegisterSheetsByPrefab.Clear();
            else RegisterSheetsByAddressable.Clear();
#endif
        }

        #endregion
    }
}