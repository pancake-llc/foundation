using System;
using UnityEngine;

namespace Pancake.DebugView
{
    [Serializable]
    public sealed class KeyboardShortcut
    {
        [SerializeField] private bool _enabled = true;

        [SerializeField] [Tooltip("Windows: Control / Mac: Command")]
        private bool _control = true;

        [SerializeField] [Tooltip("Windows: Alt / Mac: Option")] private bool _alt;

        [SerializeField] [Tooltip("Windows: Shift / Mac: Shift")] private bool _shift = true;

        [SerializeField] private KeyCode _key = KeyCode.D;

        public bool Enabled { get => _enabled; set => _enabled = value; }

        public bool Control { get => _control; set => _control = value; }

        public bool Alt { get => _alt; set => _alt = value; }

        public bool Shift { get => _shift; set => _shift = value; }

        public KeyCode Key { get => _key; set => _key = value; }

        public bool Evaluate()
        {
            if (!_enabled)
                return false;

            if (_control && !GetControlKey())
                return false;

            if (_alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                return false;

            if (_shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return false;

            return Input.GetKeyDown(_key);
        }

        private bool GetControlKey()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
            return Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
#endif
        }
    }
}