using System;
using UnityEngine;

namespace Pancake.DebugView
{
    [Serializable]
    public sealed class KeyboardShortcut
    {
        [SerializeField] private bool _enabled = true;

        [SerializeField] [Tooltip("Windows: Control / Mac: Command")]
        private bool control = true;

        [SerializeField] [Tooltip("Windows: Alt / Mac: Option")] private bool alt;

        [SerializeField] [Tooltip("Windows: Shift / Mac: Shift")] private bool shift = true;

        [SerializeField] private KeyCode key = KeyCode.D;

        public bool Enabled { get => _enabled; set => _enabled = value; }

        public bool Control { get => control; set => control = value; }

        public bool Alt { get => alt; set => alt = value; }

        public bool Shift { get => shift; set => shift = value; }

        public KeyCode Key { get => key; set => key = value; }

        public bool Evaluate()
        {
            if (!_enabled)
                return false;

            if (control && !GetControlKey()) return false;

            if (alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt)) return false;

            if (shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return false;

            return Input.GetKeyDown(key);
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