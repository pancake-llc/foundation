using UnityEngine;

namespace Pancake.UI
{
    public interface ITransitionAnimation : IAnimation
    {
        void SetPartner(RectTransform partner);
        void Setup(RectTransform rectTransform);
    }
}