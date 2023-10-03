using UnityEngine;

namespace Pancake.UI
{
    // ReSharper disable once InconsistentNaming
    public abstract class UITransitionAnimationSO : ScriptableObject, ITransitionAnimation
    {
        public RectTransform RectTransform { get; private set; }
        public RectTransform PartnerRectTransform { get; private set; }
        public abstract float Duration { get; }

        void ITransitionAnimation.SetPartner(RectTransform partner) { PartnerRectTransform = partner; }

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