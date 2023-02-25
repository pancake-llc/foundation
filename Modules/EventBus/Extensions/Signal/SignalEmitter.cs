#if PANCAKE_TIMELINE
using System;
using UnityEngine;
using UnityEngine.Timeline;
using Pancake.EventBus.Utils;

namespace Pancake.EventBus
{
    public class SignalEmitter : MonoBehaviour
    {
        [SerializeField]
        private EmitTarget  m_EmitTo = EmitTarget.Global;
        [SerializeField]
        private SignalAsset m_Signal;

        // =======================================================================
        [Serializable] [Flags]
        public enum EmitTarget
        {
            None   = 0,
            Global = 1,
            This   = 1 << 1,
            Parent = 1 << 2,
            Childs = 1 << 3,
        }

        // =======================================================================
        public void Invoke() => Invoke(m_Signal);
        public void Invoke(SignalAsset signal)
        {
            // emit the default signal if the argument is null, do not emit null signals
            signal ??= m_Signal;
            if (signal.IsNull())
                return;

            if (m_EmitTo.HasFlag(EmitTarget.Global))
                signal.Invoke();

            if (m_EmitTo.HasFlag(EmitTarget.This))
                GetComponent<IEventBus>()?.Send(in signal);
            
            if (m_EmitTo.HasFlag(EmitTarget.Parent))
                transform.parent?.GetComponentInParent<IEventBus>()?.Send(in signal);
            
            // childs can be destroyed throw execution
            if (m_EmitTo.HasFlag(EmitTarget.Childs))
                foreach (var child in GetComponentsInChildren<IEventBus>())
                    child?.Send(in signal);
        }
        
        [ContextMenu("Invoke", false, 0)]
        private void _invokeContextMenu() => Invoke();
    }
}
#endif