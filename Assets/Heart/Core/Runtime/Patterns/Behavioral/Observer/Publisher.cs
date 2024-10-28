using System.Collections.Generic;
using Pancake.Common;

namespace Pancake.Pattern
{
    /// <summary> Publisher. </summary>
    public abstract class Publisher
    {
        private readonly List<IObserver> _observers = new();

        /// <summary> Add an observer. </summary>
        /// <param name="observer">Observer</param>
        public void AddObserver(IObserver observer) => _observers.Add(observer);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(IObserver[] observers) => _observers.AddRange(observers);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(List<IObserver> observers) => _observers.AddRange(observers);

        /// <summary> Remove an observer. </summary>
        /// <param name="observer">Observer</param>
        public void RemoveObserver(IObserver observer) => _observers.Remove(observer);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(IObserver[] observers) => _observers.Removes(observers);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(List<IObserver> observers) => _observers.Removes(observers);

        /// <summary> Notifies all its observers. </summary>
        protected void Notify()
        {
            for (var i = 0; i < _observers.Count; ++i)
                _observers[i].OnNotify();
        }
    }

    /// <summary> Publisher. </summary>
    public abstract class Publisher<T>
    {
        private readonly List<IObserver<T>> _observers = new();

        /// <summary> Add an observer. </summary>
        /// <param name="observer">Observer</param>
        public void AddObserver(IObserver<T> observer) => _observers.Add(observer);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(List<IObserver<T>> observers) => _observers.AddRange(observers);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(IObserver<T>[] observers) => _observers.AddRange(observers);

        /// <summary> Remove an observer. </summary>
        /// <param name="observer">Observer</param>
        public void RemoveObserver(IObserver<T> observer) => _observers.Remove(observer);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(List<IObserver<T>> observers) => _observers.Removes(observers);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(IObserver<T>[] observers) => _observers.Removes(observers);

        /// <summary> Notifies all its observers. </summary>
        protected void Notify(T value)
        {
            for (var i = 0; i < _observers.Count; ++i)
                _observers[i].OnNotify(value);
        }
    }

    /// <summary> Publisher. </summary>
    public abstract class Publisher<T0, T1>
    {
        private readonly List<IObserver<T0, T1>> _observers = new();

        /// <summary> Add an observer. </summary>
        /// <param name="observer">Observer</param>
        public void AddObserver(IObserver<T0, T1> observer) => _observers.Add(observer);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(List<IObserver<T0, T1>> observers) => _observers.AddRange(observers);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(IObserver<T0, T1>[] observers) => _observers.AddRange(observers);

        /// <summary> Remove an observer. </summary>
        /// <param name="observer">Observer</param>
        public void RemoveObserver(IObserver<T0, T1> observer) => _observers.Remove(observer);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(List<IObserver<T0, T1>> observers) => _observers.Removes(observers);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(IObserver<T0, T1>[] observers) => _observers.Removes(observers);

        /// <summary> Notifies all its observers. </summary>
        protected void Notify(T0 value0, T1 value1)
        {
            for (var i = 0; i < _observers.Count; ++i)
                _observers[i].OnNotify(value0, value1);
        }
    }

    /// <summary> Publisher. </summary>
    public abstract class Publisher<T0, T1, T2>
    {
        private readonly List<IObserver<T0, T1, T2>> _observers = new();

        /// <summary> Add an observer. </summary>
        /// <param name="observer">Observer</param>
        public void AddObserver(IObserver<T0, T1, T2> observer) => _observers.Add(observer);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(List<IObserver<T0, T1, T2>> observers) => _observers.AddRange(observers);

        /// <summary> Add observers. </summary>
        /// <param name="observers">Array of observers</param>
        public void AddObservers(IObserver<T0, T1, T2>[] observers) => _observers.AddRange(observers);

        /// <summary> Remove an observer. </summary>
        /// <param name="observer">Observer</param>
        public void RemoveObserver(IObserver<T0, T1, T2> observer) => _observers.Remove(observer);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(List<IObserver<T0, T1, T2>> observers) => _observers.Removes(observers);

        /// <summary> Remove observers. </summary>
        /// <param name="observers">Observers</param>
        public void RemoveObservers(IObserver<T0, T1, T2>[] observers) => _observers.Removes(observers);

        /// <summary> Notifies all its observers. </summary>
        protected void Notify(T0 value0, T1 value1, T2 value2)
        {
            for (var i = 0; i < _observers.Count; ++i)
                _observers[i].OnNotify(value0, value1, value2);
        }
    }
}