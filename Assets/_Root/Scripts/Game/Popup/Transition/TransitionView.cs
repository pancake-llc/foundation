using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class TransitionView : View
    {
        [SerializeField] private Image imageTransition;
        [SerializeField] private float duration;

        protected override UniTask Initialize() { return UniTask.CompletedTask; }

        public void Setup(float duration = -1f, bool isOpen = false)
        {
            float d = duration >= 0 ? duration : this.duration;
            App.Delay(imageTransition, d, () => PopupHelper.Close(transform, false));
            LMotion.Create(isOpen ? 1f : 0f, isOpen ? 0f : 1f, d).WithEase(Ease.OutQuad).BindToColorA(imageTransition);
        }
    }
}