using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public sealed class PageTransitionContainer
    {
        [SerializeField] private List<TransitionAnimation> pushEnterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> pushExitAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> popEnterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> popExitAnimations = new List<TransitionAnimation>();

        public ITransitionAnimation GetAnimation(bool push, bool enter, string partnerId)
        {
            var anims = GetAnimations(push, enter);
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerId));
            var result = anim?.GetAnimation();
            return result;
        }

        private IReadOnlyList<TransitionAnimation> GetAnimations(bool push, bool enter)
        {
            if (push) return enter ? pushEnterAnimations : pushExitAnimations;

            return enter ? popEnterAnimations : popExitAnimations;
        }
    }
}