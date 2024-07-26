using Sisus.Init;
using UnityEngine.EventSystems;

namespace Pancake.MobileInput
{
    [EditorIcon("icon_default")]
    public class BlockInputOnPointerDown : MonoBehaviour<TouchInput>, IPointerDownHandler
    {
        private TouchInput _touchInput;

        public void OnPointerDown(PointerEventData eventData) { _touchInput.OnEventTriggerPointerDown(eventData); }

        protected override void Init(TouchInput argument) { _touchInput = argument; }
    }
}