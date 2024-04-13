using System;
using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class PopupTransitionContainer
    {
        [SerializeField] private List<TransitionAnimation> enterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> exitAnimations = new List<TransitionAnimation>();

        public ITransitionAnimation GetAnimation(bool enter, string partnerId)
        {
            var anims = enter ? enterAnimations : exitAnimations;
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerId));
            var result = anim?.GetAnimation();
            return result;
        }
    }
}