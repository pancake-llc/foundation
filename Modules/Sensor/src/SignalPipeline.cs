using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [Serializable]
    public class SignalAccumulator : Accumulator<GameObject, Signal>
    {
        protected override Signal Combine(Signal a, Signal b)
        {
            a.Combine(b);
            return a;
        }
    }

    [Serializable]
    public class SignalPipeline : AccumulatorPipeline<GameObject, Signal, SignalAccumulator>
    {
        public SignalFilter Filter;

        public List<ISignalProcessor> SignalProcessors { get { return signalProcessors; } }
        List<ISignalProcessor> signalProcessors = new List<ISignalProcessor>();

        protected override GameObject GetTarget(Signal signal) { return signal.Object; }

        protected override bool ProcessInput(in Signal signal, out Signal processed)
        {
            var processedSignal = signal;
            foreach (var processor in signalProcessors)
            {
                if (!processor.ProcessOutput(ref processedSignal) || ReferenceEquals(processedSignal.Object, null))
                {
                    processed = new Signal();
                    return false;
                }

                if (Filter != null && !Filter.IsPassingIgnoreList(processedSignal.Object))
                {
                    processed = new Signal();
                    return false;
                }
            }

            if (Filter != null && !Filter.IsPassingTagFilter(processedSignal.Object))
            {
                processed = new Signal();
                return false;
            }

            processed = processedSignal;
            return true;
        }
    }
}