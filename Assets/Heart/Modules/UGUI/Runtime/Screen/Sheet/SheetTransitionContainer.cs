using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class SheetTransitionContainer
    {
        [SerializeField] private List<TransitionAnimation> enterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> exitAnimations = new List<TransitionAnimation>();

        public ITransitionAnimation GetAnimation(bool enter, string partnerTransitionIdentifier)
        {
            var anims = enter ? enterAnimations : exitAnimations;
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerTransitionIdentifier));
            var result = anim?.GetAnimation();
            return result;
        }
    }
}