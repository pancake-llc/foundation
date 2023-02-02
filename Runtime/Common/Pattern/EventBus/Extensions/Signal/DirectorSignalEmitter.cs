#if PANCAKE_TIMELINE
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Pancake.EventBus.Utils;

namespace Pancake.EventBus
{
    public class DirectorSignalEmitter : MonoBehaviour, INotificationReceiver
    {
        [SerializeField]
        private EmitTarget m_EmitTo = EmitTarget.Global;

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
        public void Invoke(SignalAsset signal)
        {
            // emit the default signal if the argument is null, do not emit null signals
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
        
        public void OnNotify(Playable origin, INotification notification, object context)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            if (notification is UnityEngine.Timeline.SignalEmitter signal)
                Invoke(signal.asset);
        }
    }
}
#endif