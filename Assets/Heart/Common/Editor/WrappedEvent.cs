using UnityEngine;

namespace PancakeEditor.Common
{
    public readonly struct WrappedEvent
    {
        private readonly Event _e;

        public bool IsNull => _e == null;
        public bool IsRepaint => IsNull ? default : _e.type == EventType.Repaint;
        public bool IsLayout => IsNull ? default : _e.type == EventType.Layout;
        public bool IsMouseLeaveWindow => IsNull ? default : _e.type == EventType.MouseLeaveWindow;
        public bool IsKeyDown => IsNull ? default : _e.type == EventType.KeyDown;
        public KeyCode KeyCode => IsNull ? default : _e.keyCode;
        public bool IsMouseDown => IsNull ? default : _e.type == EventType.MouseDown;
        public bool IsMouseUp => IsNull ? default : _e.type == EventType.MouseUp;
        public bool IsMouseMove => IsNull ? default : _e.type == EventType.MouseMove;
        public int ClickCount => IsNull ? default : _e.clickCount;
        public Vector2 MousePosition => IsNull ? default : _e.mousePosition;
        public Vector2 MousePositionScreenSpace => IsNull ? default : GUIUtility.GUIToScreenPoint(_e.mousePosition);
        public Vector2 MouseDelta => IsNull ? default : _e.delta;
        public EventModifiers Modifiers => IsNull ? default : _e.modifiers;
        public bool HoldingAnyModifierKey => Modifiers != EventModifiers.None;

        public bool HoldingShift => IsNull ? default : _e.shift;
        public bool HoldingCmdOrCtrl => IsNull ? default : _e.command || _e.control;

        public bool HoldingCtrlOnly => IsNull ? default : _e.modifiers == EventModifiers.Control;
        public bool HoldingCmdOnly => IsNull ? default : _e.modifiers == EventModifiers.Command;
        public void Use() => _e?.Use();

        public WrappedEvent(Event e) => _e = e;

        public override string ToString() => _e.ToString();
    }
}