using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;
using UnityEngine.Events;

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

        public event UnityAction<Vector2> OnMoveInputChanged { add => _moveInputChangedEvent.AddListener(value); remove => _moveInputChangedEvent.RemoveListener(value); }

        protected override void Init(IJoystick joystick, MoveInputChangedEvent moveInputChangedEvent)
        {
            _joystick = joystick;
            _moveInputChangedEvent = moveInputChangedEvent;
        }
    }
}