using Pancake.AssetLoader;
using Pancake.UI.Popup;
using PrimeTween;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("scriptable_setting")]
    public class DefaultPopupSetting : ScriptableSettings<DefaultPopupSetting>
    {
        [SerializeField] private UITransitionAsset sheetEnterAnim;
        [SerializeField] private UITransitionAsset sheetExitAnim;

        [SerializeField] private UITransitionAsset pagePushEnterAnim;
        [SerializeField] private UITransitionAsset pagePushExitAnim;
        [SerializeField] private UITransitionAsset pagePopEnterAnim;
        [SerializeField] private UITransitionAsset pagePopExitAnim;

        [SerializeField] private UITransitionAsset modalEnterAnim;
        [SerializeField] private UITransitionAsset modalExitAnim;

        [SerializeField] private UITransitionAsset modalBackdropEnterAnim;
        [SerializeField] private UITransitionAsset modalBackdropExitAnim;

        [SerializeField] private ModalBackdrop modalBackdropPrefab;

        [SerializeField] private AssetLoaderObject assetLoader;
        [SerializeField] private bool enableInteractionInTransition;
        [SerializeField] private bool controlInteractionAllContainer = true;

        private IAssetLoader _defaultAssetLoader;
        private ModalBackdrop _defaultModalBackdrop;

        public static ITransitionAnimation GetDefaultPageTransition(bool push, bool enter)
        {
            if (push) return enter ? Instance.pagePushEnterAnim : Instance.pagePushExitAnim;
            return enter ? Instance.pagePopEnterAnim : Instance.pagePopExitAnim;
        }

        public static ITransitionAnimation GetDefaultModalTransition(bool enter) { return enter ? Instance.modalEnterAnim : Instance.modalExitAnim; }

        public static ITransitionAnimation GetDefaultSheetTransition(bool enter) { return enter ? Instance.sheetEnterAnim : Instance.sheetExitAnim; }

        public static ModalBackdrop ModalBackdropPrefab
        {
            get
            {
                if (ModalBackdropPrefab != null) return Instance.modalBackdropPrefab;

                if (Instance._defaultModalBackdrop == null) Instance._defaultModalBackdrop = Resources.Load<ModalBackdrop>("DefaultModalBackdropPrefab");

                return Instance._defaultModalBackdrop;
            }
        }

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
            Instance.sheetEnterAnim != null ? Instantiate(Instance.sheetEnterAnim) : SimpleUITransitionAsset.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation SheetExit =>
            Instance.sheetExitAnim != null ? Instantiate(Instance.sheetExitAnim) : SimpleUITransitionAsset.CreateInstance(afterAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation PagePushEnter =>
            Instance.pagePushEnterAnim != null
                ? Instantiate(Instance.pagePushEnterAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Right, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePushExit =>
            Instance.pagePushExitAnim != null
                ? Instantiate(Instance.pagePushExitAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Left);

        public static ITransitionAnimation PagePopEnter =>
            Instance.pagePopEnterAnim != null
                ? Instantiate(Instance.pagePopEnterAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Left, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePopExit =>
            Instance.pagePopExitAnim != null
                ? Instantiate(Instance.pagePopExitAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Right);

        public static ITransitionAnimation ModalEnter =>
            Instance.modalEnterAnim != null
                ? Instantiate(Instance.modalEnterAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeScale: Vector3.one * 0.3f, beforeAlpha: 0f);

        public static ITransitionAnimation ModalExit =>
            Instance.modalExitAnim != null ? Instantiate(Instance.modalExitAnim) : SimpleUITransitionAsset.CreateInstance(afterScale: Vector3.one * 0.3f, afterAlpha: 0f);

        public static ITransitionAnimation ModalBackdropEnter =>
            Instance.modalBackdropEnterAnim != null
                ? Instantiate(Instance.modalBackdropEnterAnim)
                : SimpleUITransitionAsset.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation ModalBackdropExit =>
            Instance.modalBackdropExitAnim != null
                ? Instantiate(Instance.modalBackdropExitAnim)
                : SimpleUITransitionAsset.CreateInstance(afterAlpha: 1f, easeType: Ease.Linear);


        public static bool EnableInteractionInTransition { get => Instance.enableInteractionInTransition; set => Instance.enableInteractionInTransition = value; }

        public static bool ControlInteractionAllContainer { get => Instance.controlInteractionAllContainer; set => Instance.controlInteractionAllContainer = value; }
    }
}