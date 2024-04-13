using System;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public sealed class PopupBackdropTransitionContainer
    {
        [SerializeField] private TransitionAnimation enterAnimation;
        [SerializeField] private TransitionAnimation exitAnimation;

        public ITransitionAnimation GetAnimation(bool enter)
        {
            var transitionAnimation = enter ? enterAnimation : exitAnimation;
            return transitionAnimation.GetAnimation();
        }
    }
}