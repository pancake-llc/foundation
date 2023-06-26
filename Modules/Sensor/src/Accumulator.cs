using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [Serializable]
    public abstract class Accumulator<REF, T> : IDisposable where T : IEquatable<T> where REF : UnityEngine.Object
    {
        [SerializeField] REF outputTarget;
        public REF OutputTarget => outputTarget;
        [SerializeField] List<REF> inputTargets = new List<REF>();
        public List<REF> InputTargets => inputTargets;
        [SerializeField] List<T> inputs = new List<T>();
        public List<T> Inputs => inputs;
        [SerializeField] T output;
        public T RawOutput => output;
        [SerializeField] bool isDirty;
        public T PreviousOutput { get; private set; }

        int Timestamp = -1;

        public T Output
        {
            get
            {
                if (isDirty)
                {
                    Combine();
                    isDirty = false;
                }

                return output;
            }
        }

        public void Spawn(REF target, int timestamp)
        {
            Timestamp = timestamp;
            outputTarget = target;
        }

        public void Dispose()
        {
            inputs.Clear();
            outputTarget = null;
            PreviousOutput = default;
            output = default(T);
            isDirty = false;
        }

        public bool UpdateInput(REF target, T input, int timestamp)
        {
            if (TryGetInput(target, out var found, out var index))
            {
                if (found.Equals(input))
                {
                    return false;
                }

                SetTimestamp(timestamp);
                inputs[index] = input;
                isDirty = true;
                return true;
            }
            else
            {
                SetTimestamp(timestamp);
                inputs.Add(input);
                inputTargets.Add(target);
                isDirty = true;
                return true;
            }
        }

        public bool RemoveInput(REF target, int timestamp)
        {
            if (TryGetInput(target, out var found, out var index))
            {
                SetTimestamp(timestamp);
                inputs.RemoveAt(index);
                inputTargets.RemoveAt(index);
                isDirty = true;
                return true;
            }

            return false;
        }

        protected abstract T Combine(T a, T b);

        void SetTimestamp(int timestamp)
        {
            if (Timestamp != timestamp)
            {
                Timestamp = timestamp;
                PreviousOutput = Output;
            }
        }

        bool TryGetInput(REF target, out T input, out int index)
        {
            for (int i = 0; i < inputTargets.Count; i++)
            {
                var t = inputTargets[i];
                if (ReferenceEquals(t, target))
                {
                    input = inputs[i];
                    index = i;
                    return true;
                }
            }

            input = default(T);
            index = -1;
            return false;
        }

        void Combine()
        {
            bool isFirst = true;
            foreach (var input in inputs)
            {
                if (isFirst)
                {
                    output = input;
                    isFirst = false;
                }
                else
                {
                    output = Combine(output, input);
                }
            }
        }
    }
}