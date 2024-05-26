#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class ViewShowAnimation
    {
        #region Fields

        [SerializeReference] public MoveShowAnimation moveAnimation;
        [SerializeReference] public RotateShowAnimation rotateAnimation;
        [SerializeReference] public ScaleShowAnimation scaleAnimation;
        [SerializeReference] public FadeShowAnimation fadeAnimation;

        #endregion

        #region Public Methods

        public async UniTask AnimateAsync(Transform transform, CanvasGroup canvasGroup) => await AnimateAsync((RectTransform) transform, canvasGroup);

        public async UniTask AnimateAsync(RectTransform rectTransform, CanvasGroup canvasGroup)
        {
            // ReSharper disable once HeapView.ObjectAllocation
            await UniTask.WhenAll(moveAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                rotateAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                scaleAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                fadeAnimation?.AnimateAsync(canvasGroup) ?? UniTask.CompletedTask);
        }

        #endregion
    }

    [Serializable]
    public class ViewHideAnimation
    {
        #region Fields

        [SerializeReference] private MoveHideAnimation moveAnimation;
        [SerializeReference] private RotateHideAnimation rotateAnimation;
        [SerializeReference] private ScaleHideAnimation scaleAnimation;
        [SerializeReference] private FadeHideAnimation fadeAnimation;

        #endregion

        #region Public Methods

        public async UniTask AnimateAsync(Transform transform, CanvasGroup canvasGroup) => await AnimateAsync((RectTransform) transform, canvasGroup);

        public async UniTask AnimateAsync(RectTransform rectTransform, CanvasGroup canvasGroup)
        {
            // ReSharper disable once HeapView.ObjectAllocation
            await UniTask.WhenAll(moveAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                rotateAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                scaleAnimation?.AnimateAsync(rectTransform) ?? UniTask.CompletedTask,
                fadeAnimation?.AnimateAsync(canvasGroup) ?? UniTask.CompletedTask);
        }

        #endregion
    }
}
#endif