using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.UI
{
    [Serializable]
    public class TransitionAnimation
    {
        [SerializeField] private string partnerIdRegex;
        [SerializeField] private EAnimationAssetType type;

        [SerializeField] [ShowIf("IsAssetTypeIsMono")] private UITransitionComponent animMono;

        [SerializeField] [ShowIf("IsAssetTypeIsSo")] private UITransitionAnimationSO animScriptable;

        private Regex _partnerIdRegexCache;

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private bool IsAssetTypeIsMono => type == EAnimationAssetType.MonoBehaviour;

        // ReSharper disable once UnusedMember.Local
        private bool IsAssetTypeIsSo => type == EAnimationAssetType.ScriptableObject;
#endif

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
                EAnimationAssetType.MonoBehaviour => animMono,
                EAnimationAssetType.ScriptableObject => Object.Instantiate(animScriptable),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}