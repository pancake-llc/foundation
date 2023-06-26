using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [Serializable]
    public abstract class AccumulatorPipeline<REF, T, U> : ISerializationCallbackReceiver
        where REF : UnityEngine.Object where T : IEquatable<T> where U : Accumulator<REF, T>, new()
    {
        TargetsEnumerable targetsEnumerable;

        public TargetsEnumerable OutputTargets
        {
            get
            {
                if (targetsEnumerable == null)
                {
                    targetsEnumerable = new TargetsEnumerable(this);
                }

                return targetsEnumerable;
            }
        }

        OutputsEnumerable outputsEnumerable;

        public OutputsEnumerable Outputs
        {
            get
            {
                if (outputsEnumerable == null)
                {
                    outputsEnumerable = new OutputsEnumerable(this);
                }

                return outputsEnumerable;
            }
        }

        public event Action<T> OnAdd;
        public event Action<T, T> OnChange;
        public event Action<T> OnRemove;
        public event Action OnSome;
        public event Action OnNone;

        Dictionary<REF, U> inputToMap = new Dictionary<REF, U>();
        Dictionary<REF, U> outputToMap = new Dictionary<REF, U>();
        [SerializeField] List<U> saveAccumulators = new List<U>();

        [NonSerialized] HashSet<REF> toRemove = new HashSet<REF>();
        [NonSerialized] List<U> added = new List<U>();
        [NonSerialized] HashSet<U> changed = new HashSet<U>();
        [NonSerialized] List<T> removed = new List<T>();

        [SerializeField] int prevSignalCount;
        int timestamp = 0;

        public T GetOutput(REF go) { return outputToMap[go].Output; }

        public bool TryGetOutput(REF go, out T output)
        {
            if (outputToMap.TryGetValue(go, out var map))
            {
                output = map.Output;
                return true;
            }

            output = default;
            return false;
        }

        public List<REF> GetInputObjects(REF go, List<REF> storeIn)
        {
            if (outputToMap.TryGetValue(go, out var map))
            {
                foreach (var input in map.InputTargets)
                {
                    storeIn.Add(input);
                }
            }

            return storeIn;
        }

        public bool ContainsOutput(REF go) { return outputToMap.ContainsKey(go); }

        public void UpdateAllInputs(List<T> nextInputs)
        {
            toRemove.Clear();
            foreach (var input in inputToMap)
            {
                toRemove.Add(input.Key);
            }

            foreach (var signal in nextInputs)
            {
                toRemove.Remove(GetTarget(signal));
                UpdateInputInternal(signal);
            }

            foreach (var remaining in toRemove)
            {
                RemoveInputInternal(remaining);
            }

            PlayEvents();
        }

        public void UpdateInput(T signal)
        {
            UpdateInputInternal(signal);
            PlayEvents();
        }

        public void RemoveInput(REF forObject)
        {
            RemoveInputInternal(forObject);
            PlayEvents();
        }

        public void OnBeforeSerialize()
        {
            saveAccumulators.Clear();
            var accSet = new HashSet<U>();
            foreach (var acc in inputToMap)
            {
                accSet.Add(acc.Value);
            }

            foreach (var acc in outputToMap)
            {
                accSet.Add(acc.Value);
            }

            foreach (var acc in accSet)
            {
                saveAccumulators.Add(acc);
            }
        }

        public void OnAfterDeserialize()
        {
            inputToMap.Clear();
            outputToMap.Clear();
            foreach (var acc in saveAccumulators)
            {
                foreach (var input in acc.InputTargets)
                {
                    inputToMap[input] = acc;
                }

                outputToMap[acc.OutputTarget] = acc;
            }
        }

        protected abstract REF GetTarget(T item);
        protected abstract bool ProcessInput(in T input, out T processed);

        void UpdateInputInternal(T signal)
        {
            if (ReferenceEquals(GetTarget(signal), null))
            {
                return;
            }

            if (ProcessInput(in signal, out var processed))
            {
                UpdateProcessedInput(GetTarget(signal), processed);
            }
            else
            {
                RemoveInputInternal(GetTarget(signal));
            }
        }

        void RemoveInputInternal(REF forObject)
        {
            if (inputToMap.TryGetValue(forObject, out var prevSignalMap))
            {
                RemoveInputFromMap(forObject, prevSignalMap);
                inputToMap.Remove(forObject);
            }
        }

        void UpdateProcessedInput(REF inputTarget, T processed)
        {
            if (inputToMap.TryGetValue(inputTarget, out var prevSignalMap))
            {
                if (ReferenceEquals(prevSignalMap.OutputTarget, GetTarget(processed)))
                {
                    if (prevSignalMap.UpdateInput(inputTarget, processed, timestamp))
                    {
                        OnChangedEvent(prevSignalMap);
                    }
                }
                else
                {
                    RemoveInputFromMap(inputTarget, prevSignalMap);
                    NewProcessedInput(inputTarget, processed);
                }
            }
            else
            {
                NewProcessedInput(inputTarget, processed);
            }
        }

        void NewProcessedInput(REF inputTarget, T processed)
        {
            if (!outputToMap.TryGetValue(GetTarget(processed), out var map))
            {
                map = accumulatorCache.Get();
                map.Spawn(GetTarget(processed), timestamp);
                outputToMap[map.OutputTarget] = map;
                OnAddedEvent(map);
            }

            inputToMap[inputTarget] = map;
            map.UpdateInput(inputTarget, processed, timestamp);
        }

        void RemoveInputFromMap(REF inputObject, U map)
        {
            if (map.RemoveInput(inputObject, timestamp))
            {
                if (map.Inputs.Count > 0)
                {
                    OnChangedEvent(map);
                }
                else
                {
                    OnRemovedEvent(map);
                    outputToMap.Remove(map.OutputTarget);
                    accumulatorCache.Dispose(map);
                }
            }
        }

        void OnAddedEvent(U signal) { added.Add(signal); }

        void OnChangedEvent(U signal) { changed.Add(signal); }

        void OnRemovedEvent(U signal)
        {
            changed.Remove(signal);
            removed.Add(signal.PreviousOutput);
        }

        void PlayEvents()
        {
            foreach (var change in changed)
            {
                var previousOutput = change.PreviousOutput;
                if (GetTarget(previousOutput) != null)
                {
                    OnChange?.Invoke(previousOutput, change.Output);
                }
            }

            foreach (var remove in removed)
            {
                OnRemove?.Invoke(remove);
            }

            foreach (var add in added)
            {
                OnAdd?.Invoke(add.Output);
            }

            var signalCount = Outputs.Count;
            if (prevSignalCount == 0 && signalCount > 0)
            {
                OnSome?.Invoke();
            }
            else if (prevSignalCount > 0 && signalCount == 0)
            {
                OnNone?.Invoke();
            }

            prevSignalCount = signalCount;

            added.Clear();
            changed.Clear();
            removed.Clear();

            timestamp += 1;
        }

        AccumulatorCache accumulatorCache = new AccumulatorCache();

        class AccumulatorCache : ObjectCache<U>
        {
            public override void Dispose(U obj)
            {
                obj.Dispose();
                base.Dispose(obj);
            }
        }

        public class OutputsEnumerable : IEnumerable<T>, IEnumerable
        {
            AccumulatorPipeline<REF, T, U> source;
            public OutputsEnumerable(AccumulatorPipeline<REF, T, U> source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

            public int Count
            {
                get
                {
                    int n = 0;
                    foreach (var signalMap in source.outputToMap)
                    {
                        if (signalMap.Value.OutputTarget != null)
                        {
                            n += 1;
                        }
                    }

                    return n;
                }
            }

            public struct Enumerator : IEnumerator<T>, IEnumerator
            {
                AccumulatorPipeline<REF, T, U> source;
                Dictionary<REF, U>.Enumerator sourceEnumerator;

                public Enumerator(AccumulatorPipeline<REF, T, U> source)
                {
                    this.source = source;
                    sourceEnumerator = source.outputToMap.GetEnumerator();
                }

                public T Current { get { return sourceEnumerator.Current.Value.Output; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { sourceEnumerator.Dispose(); }

                public bool MoveNext()
                {
                    var result = sourceEnumerator.MoveNext();
                    if (result && source.GetTarget(Current) == null)
                    {
                        return MoveNext();
                    }

                    return result;
                }

                public void Reset() { sourceEnumerator = source.outputToMap.GetEnumerator(); }
            }
        }

        public class TargetsEnumerable : IEnumerable<REF>, IEnumerable
        {
            AccumulatorPipeline<REF, T, U> source;
            public TargetsEnumerable(AccumulatorPipeline<REF, T, U> source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<REF> IEnumerable<REF>.GetEnumerator() { return GetEnumerator(); }

            public int Count
            {
                get
                {
                    int n = 0;
                    foreach (var signalMap in source.outputToMap)
                    {
                        if (signalMap.Value.OutputTarget != null)
                        {
                            n += 1;
                        }
                    }

                    return n;
                }
            }

            public struct Enumerator : IEnumerator<REF>, IEnumerator
            {
                AccumulatorPipeline<REF, T, U> source;
                Dictionary<REF, U>.Enumerator sourceEnumerator;

                public Enumerator(AccumulatorPipeline<REF, T, U> source)
                {
                    this.source = source;
                    sourceEnumerator = source.outputToMap.GetEnumerator();
                }

                public REF Current { get { return sourceEnumerator.Current.Value.OutputTarget; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { sourceEnumerator.Dispose(); }

                public bool MoveNext()
                {
                    var result = sourceEnumerator.MoveNext();
                    if (result && Current == null)
                    {
                        return MoveNext();
                    }

                    return result;
                }

                public void Reset() { sourceEnumerator = source.outputToMap.GetEnumerator(); }
            }
        }
    }
}