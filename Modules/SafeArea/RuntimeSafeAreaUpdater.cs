using Pancake.Apex;
using UnityEngine;

namespace Pancake.SafeArea
{
    [EditorIcon("script_manager")]
    [HideMonoScript]
    public class RuntimeSafeAreaUpdater : CacheGameComponent<ISafeAreaUpdatable>
    {
        private Rect _safeArea;

        private void Start()
        {
            _safeArea = Screen.safeArea;
            component.UpdateRect();
        }

        protected override void Tick()
        {
            if (_safeArea == Screen.safeArea) return;
            _safeArea = Screen.safeArea;
            component.UpdateRect();
        }
    }
}