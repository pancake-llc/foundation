using UnityEngine;

namespace Pancake.UI
{
    public abstract class UITransitionAsset : ScriptableObject, ITransitionAnimation
    {
        public RectTransform RectTransform { get; private set; }
        public RectTransform PartnerRectTransform { get; private set; }
        public abstract float Duration { get; }

        void ITransitionAnimation.SetPartner(RectTransform partnerRectTransform) { PartnerRectTransform = partnerRectTransform; }

        void ITransitionAnimation.Setup(RectTransform rectTransform)
        {
            RectTransform = rectTransform;
            Setup();
            SetTime(0f);
        }

        public abstract void SetTime(float time);

        public abstract void Setup();
    }
}