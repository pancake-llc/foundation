using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.UI.Popup
{
    [Serializable]
    public sealed class ModalBackdropTransitionContainer
    {
        [Serializable]
        public sealed class TransitionAnimation
        {
            [SerializeField] private AnimationAssetType type;

            [SerializeField, ShowIf(nameof(type), AnimationAssetType.MonoBehaviour), Label("Animation")]
            private UITransitionComponent uiTransitionComponent;

            [SerializeField, ShowIf(nameof(type), AnimationAssetType.ScriptableObject), Label("Animation")]
            private UITransitionAsset uiTransitionAsset;

            public ITransitionAnimation GetAnimation()
            {
                return type switch
                {
                    AnimationAssetType.MonoBehaviour => uiTransitionComponent,
                    AnimationAssetType.ScriptableObject => UnityEngine.Object.Instantiate(uiTransitionAsset),
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