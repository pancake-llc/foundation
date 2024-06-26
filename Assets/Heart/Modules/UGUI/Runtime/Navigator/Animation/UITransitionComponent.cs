using UnityEngine;

namespace Pancake.UI
{
    public abstract class UITransitionComponent : GameComponent, ITransitionAnimation
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

        public abstract void Setup();
        public abstract void SetTime(float time);
    }
}