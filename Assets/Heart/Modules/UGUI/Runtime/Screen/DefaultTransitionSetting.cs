using Pancake.AssetLoader;
using PrimeTween;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("scriptable_setting")]
    public class DefaultTransitionSetting : ScriptableSettings<DefaultTransitionSetting>
    {
        [SerializeField] private UITransitionAnimationSO sheetEnterAnim;
        [SerializeField] private UITransitionAnimationSO sheetExitAnim;

        [Space] [SerializeField] private UITransitionAnimationSO pagePushEnterAnim;
        [SerializeField] private UITransitionAnimationSO pagePushExitAnim;
        [SerializeField] private UITransitionAnimationSO pagePopEnterAnim;
        [SerializeField] private UITransitionAnimationSO pagePopExitAnim;
        
        [Space] [SerializeField] private UITransitionAnimationSO popupEnterAnim;
        [SerializeField] private UITransitionAnimationSO popupExitAnim;
        
        [Space] [SerializeField] private UITransitionAnimationSO popupBackdropEnterAnim;
        [SerializeField] private UITransitionAnimationSO popupBackdropExitAnim;

        [SerializeField] private PopupBackdrop popupBackdropPrefab;
        [Space] [SerializeField] private AssetLoaderObject assetLoader;
        [SerializeField] private bool enableInteractionInTransition;
        [SerializeField] private bool controlInteractionAllContainer = true;

        private IAssetLoader _defaultAssetLoader;
        private PopupBackdrop _defaultPopupBackdrop;

        public static ITransitionAnimation GetDefaultPageTransition(bool push, bool enter)
        {
            if (push) return enter ? Instance.pagePushEnterAnim : Instance.pagePushExitAnim;
            return enter ? Instance.pagePopEnterAnim : Instance.pagePopExitAnim;
        }

        public static ITransitionAnimation GetDefaultPopupTransition(bool enter) { return enter ? Instance.popupEnterAnim : Instance.popupExitAnim; }

        public static ITransitionAnimation GetDefaultSheetTransition(bool enter) { return enter ? Instance.sheetEnterAnim : Instance.sheetExitAnim; }

        public static PopupBackdrop PopupBackdropPrefab
        {
            get
            {
                if (PopupBackdropPrefab != null) return Instance.popupBackdropPrefab;

                if (Instance._defaultPopupBackdrop == null) Instance._defaultPopupBackdrop = Resources.Load<PopupBackdrop>("DefaultPopupBackdrop");

                return Instance._defaultPopupBackdrop;
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

        public static ITransitionAnimation SheetEnterAnim =>
            Instance.sheetEnterAnim != null ? Instantiate(Instance.sheetEnterAnim) : SimpleUITransitionAnimationSO.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation SheetExitAnim =>
            Instance.sheetExitAnim != null ? Instantiate(Instance.sheetExitAnim) : SimpleUITransitionAnimationSO.CreateInstance(afterAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation PagePushEnterAnim =>
            Instance.pagePushEnterAnim != null
                ? Instantiate(Instance.pagePushEnterAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: Alignment.Right, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePushExitAnim =>
            Instance.pagePushExitAnim != null
                ? Instantiate(Instance.pagePushExitAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Left);

        public static ITransitionAnimation PagePopEnterAnim =>
            Instance.pagePopEnterAnim != null
                ? Instantiate(Instance.pagePopEnterAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: Alignment.Left, afterAlignment: Alignment.Center);

        public static ITransitionAnimation PagePopExitAnim =>
            Instance.pagePopExitAnim != null
                ? Instantiate(Instance.pagePopExitAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: Alignment.Center, afterAlignment: Alignment.Right);

        public static ITransitionAnimation PopupEnterAnim =>
            Instance.popupEnterAnim != null
                ? Instantiate(Instance.popupEnterAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeScale: Vector3.one * 0.3f, beforeAlpha: 0f);

        public static ITransitionAnimation PopupExitAnim =>
            Instance.popupExitAnim != null
                ? Instantiate(Instance.popupExitAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(afterScale: Vector3.one * 0.3f, afterAlpha: 0f);

        public static ITransitionAnimation PopupBackdropEnter =>
            Instance.popupBackdropEnterAnim != null
                ? Instantiate(Instance.popupBackdropEnterAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);

        public static ITransitionAnimation PopupBackdropExit =>
            Instance.popupBackdropExitAnim != null
                ? Instantiate(Instance.popupBackdropExitAnim)
                : SimpleUITransitionAnimationSO.CreateInstance(afterAlpha: 1f, easeType: Ease.Linear);

        public static bool EnableInteractionInTransition { get => Instance.enableInteractionInTransition; set => Instance.enableInteractionInTransition = value; }

        public static bool ControlInteractionAllContainer { get => Instance.controlInteractionAllContainer; set => Instance.controlInteractionAllContainer = value; }
    }
}