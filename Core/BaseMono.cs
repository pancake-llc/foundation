using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    public class BaseMono : MonoBehaviour
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>() => GetComponent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Gets<T>() => GetComponents<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ChildrenGet<T>() => GetComponentInChildren<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ChildrenGets<T>() => GetComponentsInChildren<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ParentGet<T>() => GetComponentInParent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ParentGets<T>() => GetComponentsInParent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Find<T>() where T : Object => FindObjectOfType<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Finds<T>() where T : Object => FindObjectsOfType<T>();
    }
}