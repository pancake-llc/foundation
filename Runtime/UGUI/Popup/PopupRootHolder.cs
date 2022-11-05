using UnityEngine;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class PopupRootHolder : MonoBehaviour
    {
        public static PopupRootHolder instance;

        private void Awake() { instance = this; }
    }
}