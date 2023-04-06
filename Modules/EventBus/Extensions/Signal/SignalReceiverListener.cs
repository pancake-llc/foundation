#if PANCAKE_TIMELINE
using UnityEngine;
using UnityEngine.Timeline;

namespace Pancake.EventBus
{
    [RequireComponent(typeof(SignalReceiver))]
    public class SignalReceiverListener : Subscriber, IListener<SignalAsset>
    {
        private SignalReceiver m_SignalReceiver;

        // =======================================================================
        protected override void Awake()
        {
            base.Awake();
            m_SignalReceiver = GetComponent<SignalReceiver>();
        }

        public void React(in SignalAsset e)
        {
            m_SignalReceiver.GetReaction(e)?.Invoke();
        }
    }
}
#endif