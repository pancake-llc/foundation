#if PANCAKE_TIMELINE
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace Pancake.EventBus
{
    public static class SignalExtensions
    {
        private static Dictionary<(SignalAsset, Action), GenericListener<SignalAsset>> s_SignalListeners    = new Dictionary<(SignalAsset, Action), GenericListener<SignalAsset>>(32);
        private const  string                                                          k_SignalListenerName = "GSL";

        // =======================================================================
        public static void Invoke(this SignalAsset signal)
        {
            GlobalBus.Send(in signal);
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/SignalAsset/Invoke")]
        private static void InvokeSignalMenu(MenuCommand menuCommand)
        {
            if (Application.isPlaying == false)
                return;

            if (menuCommand.context is SignalAsset sa)
                sa.Invoke();
        }
#endif
        
        // questionable
        private static void Subscribe(this SignalAsset signal, Action action, int order)
        {
            var signalListener = new GenericListener<SignalAsset>(s =>
            {
                if (s == signal)
                    action.Invoke();
            }, order, k_SignalListenerName);

            GlobalBus.Subscribe(signalListener);
            s_SignalListeners.Add((signal, action), signalListener);
        }

        private static void UnSubscribe(this SignalAsset signal, Action action)
        {
            if (s_SignalListeners.Remove((signal, action), out var signalListener))
                GlobalBus.UnSubscribe(signalListener);
        }
    }
}
#endif