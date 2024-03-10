using UnityEngine;

namespace Pancake.DebugView
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class CanvasGroupDrawerBackdrop : DrawerBackdrop
    {
        private CanvasGroup _canvasGroup;

        protected override void OnStart()
        {
            base.OnStart();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void SetProgressInternal(float visibility)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = visibility;
        }

        protected override void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null) _canvasGroup.interactable = interactable;
        }
    }
}