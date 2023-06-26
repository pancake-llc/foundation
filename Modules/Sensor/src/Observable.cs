using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake.Sensor
{
    [Serializable]
    public class ObservableGameObject : Observable<GameObject>
    {
    }

    [Serializable]
    public class ObservableTransform : Observable<Transform>
    {
    }

    [Serializable]
    public class ObservableVector3 : Observable<Vector3>
    {
    }

    [Serializable]
    public class ObservableBool : Observable<bool>
    {
    }

    [Serializable]
    public class ObservableInt : Observable<int>
    {
    }

    [Serializable]
    public class ObservableFloat : Observable<float>
    {
    }

    public interface IObservable<T> : IObservable
    {
        event Action<T, T> OnChangedValues;
        T Value { get; }
    }

    [Serializable]
    public class Observable<T> : Observable, IObservable<T>, IEquatable<Observable<T>>
    {
        [SerializeField] T value;

        // When the observables value is changed in the inspector or due to an UNDO operation we can compare
        // it to this variable to see if an event should be fired.
        T prevValue;
        bool prevValueInitialized = false;

        /// <summary>
        /// Fires when the value is changed.
        /// </summary>
        public override event Action OnChanged;

        /// <summary>
        /// Fires when the value is changed, but also includes the previous value. The first parameter is the previous value, the second parameter is the new value.
        /// </summary>
        public event Action<T, T> OnChangedValues;

        protected override string ValuePropName { get { return "value"; } }

        public Observable() { }

        public Observable(T value) { this.value = value; }

        public T Value
        {
            get { return value; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, this.value))
                {
                    return;
                }

                prevValue = value;
                prevValueInitialized = true;
                var storePrev = this.value;
                this.value = value;
                if (OnChanged != null)
                {
                    OnChanged();
                }

                if (OnChangedValues != null)
                {
                    OnChangedValues(storePrev, value);
                }
            }
        }

        public void SetValue(T value) { Value = value; }

        public static explicit operator T(Observable<T> observable) { return observable.value; }

        public override string ToString() { return value.ToString(); }

        public bool Equals(Observable<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return other.value.Equals(value);
        }

        public override bool Equals(object other) { return other != null && other is Observable<T> && ((Observable<T>) other).value.Equals(value); }

        public override int GetHashCode() { return value.GetHashCode(); }

        protected override void OnBeginGui()
        {
            prevValue = value;
            prevValueInitialized = true;
        }

        public override void OnValidate()
        {
            if (prevValueInitialized)
            {
                var nextValue = value;
                value = prevValue;
                Value = nextValue;
            }
            else
            {
                prevValue = value;
                prevValueInitialized = true;
            }
        }
    }
}