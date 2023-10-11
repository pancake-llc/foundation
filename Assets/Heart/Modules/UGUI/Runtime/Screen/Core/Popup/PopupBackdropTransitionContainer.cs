using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public sealed class PopupBackdropTransitionContainer
    {
        [Serializable]
        public sealed class TransitionAnimation
        {
            [SerializeField] private AnimationAssetType type;

            [SerializeField, ShowIf(nameof(type), AnimationAssetType.MonoBehaviour), Label("Animation")]
            private UITransitionComponent transitionAnimationComponent;

            [SerializeField, ShowIf(nameof(type), AnimationAssetType.ScriptableObject), Label("Animation")]
            private UITransitionAnimationSO transitionAnimationSo;

            public ITransitionAnimation GetAnimation()
            {
                return type switch
                {
                    AnimationAssetType.MonoBehaviour => transitionAnimationComponent,
                    AnimationAssetType.ScriptableObject => UnityEngine.Object.Instantiate(transitionAnimationSo),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        [SerializeField] private TransitionAnimation enterAnimation;
        [SerializeField] private TransitionAnimation exitAnimation;

        public ITransitionAnimation GetAnimation(bool enter)
        {
            var transitionAnimation = enter ? enterAnimation : exitAnimation;
            return transitionAnimation.GetAnimation();
        }
    }
}