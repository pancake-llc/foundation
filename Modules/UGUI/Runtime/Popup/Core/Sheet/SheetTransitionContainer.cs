using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pancake.Apex;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.UI.Popup
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

        [Serializable]
        public class TransitionAnimation
        {
            [SerializeField] private string partnerIdRegex;
            [SerializeField] private AnimationAssetType type;

            [SerializeField] [ShowIf(nameof(type), AnimationAssetType.MonoBehaviour)]
            private UITransitionComponent uiTransitionComponent;

            [SerializeField] [ShowIf(nameof(type), AnimationAssetType.ScriptableObject)]
            private UITransitionAsset uiTransitionAsset;

            private Regex _partnerIdRegexCache;

            public bool IsValid(string partnerId)
            {
                if (GetAnimation() == null) return false;

                // If the partner identifier is not registered, the animation is always valid.
                if (string.IsNullOrEmpty(partnerIdRegex)) return true;

                if (string.IsNullOrEmpty(partnerId)) return false;

                if (_partnerIdRegexCache == null) _partnerIdRegexCache = new Regex(partnerIdRegex);

                return _partnerIdRegexCache.IsMatch(partnerId);
            }

            public ITransitionAnimation GetAnimation()
            {
                return type switch
                {
                    AnimationAssetType.MonoBehaviour => uiTransitionComponent,
                    AnimationAssetType.ScriptableObject => Object.Instantiate(uiTransitionAsset),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}