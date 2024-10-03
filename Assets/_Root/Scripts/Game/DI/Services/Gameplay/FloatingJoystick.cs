using UnityEngine.EventSystems;

namespace Pancake.Game
{
    public class FloatingJoystick : Joystick
    {
        private bool _pointedDown;

        public override void Initialize()
        {
            base.Initialize();
            visual.gameObject.SetActive(false);
            _pointedDown = false;
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _pointedDown = false;
            visual.gameObject.SetActive(false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_pointedDown) return;
            _pointedDown = true;
            visual.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            visual.gameObject.SetActive(true);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            _pointedDown = false;
            visual.gameObject.SetActive(false);
            base.OnPointerUp(eventData);
        }
    }
}