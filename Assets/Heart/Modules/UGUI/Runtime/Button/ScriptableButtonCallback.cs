using System;
using UnityEngine;

namespace Pancake.UI
{
    [CreateAssetMenu(menuName = "Pancake/Scriptable/Variables/button callback")]
    [EditorIcon("so_blue_variable")]
    [Serializable]
    public class ScriptableButtonCallback : ScriptableObject, ISerializationCallbackReceiver
    {
        private event Action OnPointerUp;
        private event Action OnPointerDown;
        private event Action OnClick;
        private event Action OnDoubleClick;
        private event Action OnLongClick;
        private event Action<float> OnHold;

        public void BindPointerUp(Action onPointerUp) { OnPointerUp = onPointerUp; }
        public void BindPointerDown(Action onPointerDown) { OnPointerDown = onPointerDown; }
        public void BindClick(Action onClick) { OnClick = onClick; }
        public void BindDoubleClick(Action onDoubleClick) { OnDoubleClick = onDoubleClick; }
        public void BindLongClick(Action onLongClick) { OnLongClick = onLongClick; }
        public void BindHold(Action<float> onHold) { OnHold = onHold; }

        public void ClearPointerUp() { OnPointerUp = null; }
        public void ClearPointerDown() { OnPointerDown = null; }
        public void ClearClick() { OnClick = null; }
        public void ClearDoubleClick() { OnDoubleClick = null; }
        public void ClearLongClick() { OnLongClick = null; }
        public void ClearHold() { OnHold = null; }

        private void ClearAll()
        {
            ClearPointerUp();
            ClearPointerDown();
            ClearClick();
            ClearDoubleClick();
            ClearLongClick();
            ClearHold();
        }

        public void InvokePointerUp() { OnPointerUp?.Invoke(); }
        public void InvokePointerDown() { OnPointerDown?.Invoke(); }

        public void InvokeClick() { OnClick?.Invoke(); }
        public void InvokeDoubleClick() { OnDoubleClick?.Invoke(); }
        public void InvokeLongClick() { OnLongClick?.Invoke(); }
        public void InvokeHold(float time) { OnHold?.Invoke(time); }

        public void OnBeforeSerialize() { ClearAll(); }

        public void OnAfterDeserialize() { ClearAll(); }
    }
}