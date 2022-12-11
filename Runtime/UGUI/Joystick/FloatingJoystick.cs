#if PANCAKE_INPUTSYSTEM

namespace Pancake.UI
{
    public class FloatingJoystick : Joystick
    {
        protected override void Start() { gameObject.SetActive(false); }

        public override void OnFingerUp() { gameObject.SetActive(false); }

        public override void OnFingerDown()
        {
            gameObject.SetActive(true);
            ClampPosition(this.Finger.screenPosition);
        }
    }
}
#endif