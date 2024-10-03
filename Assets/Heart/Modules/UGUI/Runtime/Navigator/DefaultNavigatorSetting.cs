using System;
#if PANCAKE_LITMOTION
using LitMotion;
#endif
using Pancake.AssetLoader;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("so_blue_setting")]
    public class DefaultNavigatorSetting : ScriptableSettings<DefaultNavigatorSetting>
    {
        [SerializeField] private UITransitionAnimationSO sheetEnterAnim;
        [SerializeField] private UITransitionAnimationSO sheetExitAnim;
        [SerializeField] private UITransitionAnimationSO pagePushEnterAnim;
        [SerializeField] private UITransitionAnimationSO pagePushExitAnim;
        [SerializeField] private UITransitionAnimationSO pagePopEnterAnim;
        [SerializeField] private UITransitionAnimationSO pagePopExitAnim;
        [SerializeField] private UITransitionAnimationSO popupEnterAnim;
        [SerializeField] private UITransitionAnimationSO popupExitAnim;
        [SerializeField] private UITransitionAnimationSO popupBackdropEnterAnim;
        [SerializeField] private UITransitionAnimationSO popupBackdropExitAnim;
        [SerializeField] private EPopupBackdropStrategy popupBackdropStrategy;
        [SerializeField] private PopupBackdrop popupBackdropPrefab;
        [SerializeField] private AssetLoaderObject assetLoader;
        [SerializeField] private bool enableInteractionInTransition;
        [SerializeField] private bool controlInteractionAllContainer = true;
        [SerializeField] private bool callCleanupWhenDestroy = true;

        private IAssetLoader _defaultAssetLoader;

        public static ITransitionAnimation GetDefaultPageTransition(bool push, bool enter)
        {
            if (push) return enter ? Instance.pagePushEnterAnim : Instance.pagePushExitAnim;
            return enter ? Instance.pagePopEnterAnim : Instance.pagePopExitAnim;
        }

        public static ITransitionAnimation GetDefaultPopupTransition(bool enter) { return enter ? Instance.popupEnterAnim : Instance.popupExitAnim; }

        public static ITransitionAnimation GetDefaultSheetTransition(bool enter) { return enter ? Instance.sheetEnterAnim : Instance.sheetExitAnim; }

        public static EPopupBackdropStrategy PopupBackdropStrategy => Instance.popupBackdropStrategy;

        public static PopupBackdrop PopupBackdropPrefab
        {
            get
            {
                if (Instance.popupBackdropPrefab == null) throw new NullReferenceException(nameof(Instance.popupBackdropPrefab));
                return Instance.popupBackdropPrefab;
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
            Instance.sheetEnterAnim != null
                ? Instantiate(Instance.sheetEnterAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);
#else
                null;
#endif

        public static ITransitionAnimation SheetExitAnim =>
            Instance.sheetExitAnim != null
                ? Instantiate(Instance.sheetExitAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(afterAlpha: 0f, easeType: Ease.Linear);
#else
                null;
#endif

        public static ITransitionAnimation PagePushEnterAnim =>
            Instance.pagePushEnterAnim != null
                ? Instantiate(Instance.pagePushEnterAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: EAlignment.Right, afterAlignment: EAlignment.Center);
#else
                null;
#endif

        public static ITransitionAnimation PagePushExitAnim =>
            Instance.pagePushExitAnim != null
                ? Instantiate(Instance.pagePushExitAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: EAlignment.Center, afterAlignment: EAlignment.Left);
#else
                null;
#endif

        public static ITransitionAnimation PagePopEnterAnim =>
            Instance.pagePopEnterAnim != null
                ? Instantiate(Instance.pagePopEnterAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: EAlignment.Left, afterAlignment: EAlignment.Center);
#else
                null;
#endif

        public static ITransitionAnimation PagePopExitAnim =>
            Instance.pagePopExitAnim != null
                ? Instantiate(Instance.pagePopExitAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlignment: EAlignment.Center, afterAlignment: EAlignment.Right);
#else
                null;
#endif

        public static ITransitionAnimation PopupEnterAnim =>
            Instance.popupEnterAnim != null
                ? Instantiate(Instance.popupEnterAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeScale: Vector3.one * 0.3f, beforeAlpha: 0f);
#else
                null;
#endif

        public static ITransitionAnimation PopupExitAnim =>
            Instance.popupExitAnim != null
                ? Instantiate(Instance.popupExitAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(afterScale: Vector3.one * 0.3f, afterAlpha: 0f);
#else
                null;
#endif

        public static ITransitionAnimation PopupBackdropEnter =>
            Instance.popupBackdropEnterAnim != null
                ? Instantiate(Instance.popupBackdropEnterAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(beforeAlpha: 0f, easeType: Ease.Linear);
#else
                null;
#endif

        public static ITransitionAnimation PopupBackdropExit =>
            Instance.popupBackdropExitAnim != null
                ? Instantiate(Instance.popupBackdropExitAnim)
                :
#if PANCAKE_LITMOTION
                SimpleUITransitionAnimationSO.CreateInstance(afterAlpha: 1f, easeType: Ease.Linear);
#else
                null;
#endif

        public static bool EnableInteractionInTransition { get => Instance.enableInteractionInTransition; set => Instance.enableInteractionInTransition = value; }

        public static bool ControlInteractionAllContainer { get => Instance.controlInteractionAllContainer; set => Instance.controlInteractionAllContainer = value; }

        public static bool CallCleanupWhenDestroy => Instance.callCleanupWhenDestroy;
    }
}