using Pancake.AssetLoader;
using PrimeTween;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("scriptable_setting")]
    public class DefaultPopupSetting : ScriptableSettings<DefaultPopupSetting>
    {
        private const string DEFAULT_MODAL_BACKDROP_PREFAB_KEY = "default_modal_backdrop";

        [SerializeField] private UITransitionAsset sheetEnter;
        [SerializeField] private UITransitionAsset sheetExit;

        [SerializeField] private UITransitionAsset pagePushEnter;
        [SerializeField] private UITransitionAsset pagePushExit;
        [SerializeField] private UITransitionAsset pagePopEnter;
        [SerializeField] private UITransitionAsset pagePopExit;

        [SerializeField] private UITransitionAsset modalEnter;
        [SerializeField] private UITransitionAsset modalExit;

        [SerializeField] private UITransitionAsset modalBackdropEnter;
        [SerializeField] private UITransitionAsset modalBackdropExit;

        [SerializeField] private AssetLoaderObject assetLoader;
        [SerializeField] private bool enableInteractionInTransition;
        [SerializeField] private bool controlInteractionAllContainer = true;

        private IAssetLoader _defaultAssetLoader;

        public static ITransitionAnimation GetDefaultPageTransition(bool push, bool enter)
        {
            if (push) return enter ? Instance.pagePushEnter : Instance.pagePushExit;
            return enter ? Instance.pagePopEnter : Instance.pagePopExit;
        }

        public static ITransitionAnimation GetDefaultModalTransition(bool enter) { return enter ? Instance.modalEnter : Instance.modalExit; }

        public static ITransitionAnimation GetDefaultSheetTransition(bool enter) { return enter ? Instance.sheetEnter : Instance.sheetExit; }

        public static IAssetLoader AssetLoader
        {
            get
            {
                if (Instance.assetLoader != null) return Instance.assetLoader;

                if (Instance._defaultAssetLoader == null) Instance._defaultAssetLoader = CreateInstance<ResourcesAssetLoaderObject>();
                return Instance._defaultAssetLoader;
            }
        }

        public static ITransitionAnimation SheetEnter =>
            Instance.sheetEnter != null ? Instantiate(Instance.sheetEnter) : SimpleUITransitionAsset.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation SheetExit =>
            Instance.sheetExit != null ? Instantiate(Instance.sheetExit) : SimpleUITransitionAsset.CreateInstance(afterAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation PagePushEnter =>
            Instance.pagePushEnter != null
                ? Instantiate(Instance.pagePushEnter)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Right, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePushExit =>
            Instance.pagePushExit != null
                ? Instantiate(Instance.pagePushExit)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Left);

        public static ITransitionAnimation PagePopEnter =>
            Instance.pagePopEnter != null
                ? Instantiate(Instance.pagePopEnter)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Left, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePopExit =>
            Instance.pagePopExit != null
                ? Instantiate(Instance.pagePopExit)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Right);

        public static ITransitionAnimation ModalEnter =>
            Instance.modalEnter != null ? Instantiate(Instance.modalEnter) : SimpleUITransitionAsset.CreateInstance(beforeScale: Vector3.one * 0.3f, beforeAlpha: 0f);

        public static ITransitionAnimation ModalExit =>
            Instance.modalExit != null ? Instantiate(Instance.modalExit) : SimpleUITransitionAsset.CreateInstance(afterScale: Vector3.one * 0.3f, afterAlpha: 0f);

        public static bool EnableInteractionInTransition { get => Instance.enableInteractionInTransition; set => Instance.enableInteractionInTransition = value; }

        public static bool ControlInteractionAllContainer { get => Instance.controlInteractionAllContainer; set => Instance.controlInteractionAllContainer = value; }
    }
}