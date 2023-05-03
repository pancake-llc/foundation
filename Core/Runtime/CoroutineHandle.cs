namespace Pancake
{
    using System.Collections;
    using System;
    using UnityEngine;

    public class CoroutineHandle : IEnumerator
    {
        public bool IsDone { get; private set; }

        public bool MoveNext() => !IsDone;

        public void Reset() { }

        public object Current { get; }

        private event Action<CoroutineHandle> CompletedInternal;

        public event Action<CoroutineHandle> Completed
        {
            add
            {
                if (IsDone) value(this);
                else CompletedInternal += value;
            }
            remove => CompletedInternal -= value;
        }

        public CoroutineHandle(MonoBehaviour owner, IEnumerator coroutine) { Current = owner.StartCoroutine(Wrap(coroutine)); }

        private IEnumerator Wrap(IEnumerator coroutine)
        {
            yield return coroutine;
            IsDone = true;
            CompletedInternal?.Invoke(this);
        }
    }
}