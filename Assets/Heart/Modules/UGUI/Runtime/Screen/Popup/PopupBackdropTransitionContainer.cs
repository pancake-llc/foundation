using System;
using Alchemy.Inspector;
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

#if UNITY_EDITOR
            private bool IsMonobehaviour => type == AnimationAssetType.MonoBehaviour;
            private bool IsScriptableObject => type == AnimationAssetType.MonoBehaviour;
#endif
            
            [SerializeField, ShowIf("IsMonobehaviour"), LabelText("Animation")]
            private UITransitionComponent transitionAnimationComponent;

            [SerializeField, ShowIf("IsScriptableObject"), LabelText("Animation")]
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