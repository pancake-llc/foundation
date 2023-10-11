using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pancake.UI
{
    [Serializable]
    public sealed class PageTransitionContainer
    {
        [Serializable]
        public sealed class TransitionAnimation
        {
            [SerializeField] private string partnerIdRegex;
            [SerializeField] private AnimationAssetType type;

            [SerializeField, ShowIf(nameof(type), AnimationAssetType.MonoBehaviour), Label("Animation")]
            private UITransitionComponent uiTransitionComponent;

            [FormerlySerializedAs("uiTransitionAsset")] [SerializeField, ShowIf(nameof(type), AnimationAssetType.ScriptableObject), Label("Animation")]
            private UITransitionAnimationSO uiTransitionAnimationSo;

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
                    AnimationAssetType.ScriptableObject => UnityEngine.Object.Instantiate(uiTransitionAnimationSo),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        [SerializeField, Array] private List<TransitionAnimation> pushEnterAnimations = new List<TransitionAnimation>();
        [SerializeField, Array] private List<TransitionAnimation> pushExitAnimations = new List<TransitionAnimation>();
        [SerializeField, Array] private List<TransitionAnimation> popEnterAnimations = new List<TransitionAnimation>();
        [SerializeField, Array] private List<TransitionAnimation> popExitAnimations = new List<TransitionAnimation>();

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