using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    public class PlayerInputManager : MonoBehaviour<IJoystick, MoveInputChangedEvent>
    {
        private MoveInputChangedEvent _moveInputChangedEvent;
        private IJoystick _joystick;
        private Vector2 _previousInput;

        public void Update()
        {
            var input = _joystick.Direction;
            if (_previousInput == input) return;

            _previousInput = input;
            _moveInputChangedEvent.Trigger(input);
        }

        protected override void Init(IJoystick joystick, MoveInputChangedEvent moveInputChangedEvent)
        {
            _joystick = joystick;
            _moveInputChangedEvent = moveInputChangedEvent;
        }
    }
}