using System;

namespace Pancake.UI
{
    [Serializable]
    public class ButtonCallbackData
    {
        private event Action OnPointerUp;
        private event Action OnPointerDown;
        private event Action OnClick;
        private event Action OnDoubleClick;
        private event Action OnLongClick;
        private event Action<float> OnHold;

        public void AddListenerPointerUp(Action onPointerUp) { OnPointerUp += onPointerUp; }
        public void AddListenerPointerDown(Action onPointerDown) { OnPointerDown += onPointerDown; }
        public void AddListenerClick(Action onClick) { OnClick += onClick; }
        public void AddListenerDoubleClick(Action onDoubleClick) { OnDoubleClick += onDoubleClick; }
        public void AddListenerLongClick(Action onLongClick) { OnLongClick += onLongClick; }
        public void AddListenerHold(Action<float> onHold) { OnHold += onHold; }

        public void RemoveListenerUp(Action onPointerUp) { OnPointerUp -= onPointerUp; }
        public void RemoveListenerDown(Action onPointerDown) { OnPointerDown -= onPointerDown; }
        public void RemoveListenerClick(Action onClick) { OnClick -= onClick; }
        public void RemoveListenerDoubleClick(Action onDoubleClick) { OnDoubleClick -= onDoubleClick; }
        public void RemoveListenerLongClick(Action onLongClick) { OnLongClick -= onLongClick; }
        public void RemoveListenerHold(Action<float> onHold) { OnHold -= onHold; }

        public void InvokePointerUp() { OnPointerUp?.Invoke(); }
        public void InvokePointerDown() { OnPointerDown?.Invoke(); }
        public void InvokeClick() { OnClick?.Invoke(); }
        public void InvokeDoubleClick() { OnDoubleClick?.Invoke(); }
        public void InvokeLongClick() { OnLongClick?.Invoke(); }
        public void InvokeHold(float time) { OnHold?.Invoke(time); }
    }
}