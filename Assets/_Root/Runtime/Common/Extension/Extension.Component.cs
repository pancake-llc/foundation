using System.Collections;

namespace Pancake
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
#if UNITY_EDITOR
    using System.Reflection;

#endif


    public static partial class C
    {
        /// <summary>
        /// add blank button
        /// </summary>
        /// <param name="target"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static T AddBlankComponent<T>(this GameObject target, Action<T> setup) where T : MonoBehaviour
        {
            var button = target.AddComponent<T>();
            setup?.Invoke(button);
            return button;
        }
        
        /// <summary>
        /// Get the RectTransform component.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static RectTransform rectTransform(this Component target) { return target.transform as RectTransform; }

        /// <summary>
        /// Delay invoking (scaled time).
        /// </summary>
        public static void Delay(this MonoBehaviour behaviour, float delay, Action action)
        {
            behaviour.StartCoroutine(DelayedCoroutine());

            IEnumerator DelayedCoroutine()
            {
                yield return new WaitForSeconds(delay);
                action();
            }
        }
        
        
        /// <summary>
        /// Delay invoking (unscaled time).
        /// </summary>
        public static void DelayRealtime(this MonoBehaviour behaviour, float delay, Action action)
        {
            behaviour.StartCoroutine(DelayedCoroutine());

            IEnumerator DelayedCoroutine()
            {
                yield return new WaitForSecondsRealtime(delay);
                action();
            }
        }
        
        #region field info

#if UNITY_EDITOR
        public static readonly Dictionary<int, List<FieldInfo>> FieldInfoList = new Dictionary<int, List<FieldInfo>>();

        public static int GetFieldInfo(UnityEngine.Object target, out List<FieldInfo> fieldInfoList)
        {
            Type targetType = target.GetType();
            int targetTypeHashCode = targetType.GetHashCode();

            if (!FieldInfoList.TryGetValue(targetTypeHashCode, out fieldInfoList))
            {
                IList<Type> typeTree = targetType.GetBaseTypes();
                fieldInfoList = target.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();
                FieldInfoList.Add(targetTypeHashCode, fieldInfoList);
            }

            return fieldInfoList.Count;
        }
#endif
        public static IList<Type> GetBaseTypes(this Type t)
        {
            var types = new List<Type>();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }

        #endregion

        #region interface

        public struct ComponentOfInterface<T>
        {
            public readonly Component component;
            public readonly T @interface;

            public ComponentOfInterface(Component component, T @interface)
            {
                this.component = component;
                this.@interface = @interface;
            }
        }

        /// <summary>
        /// Find all Components of specified interface
        /// </summary>
        public static T[] FindObjectsOfInterface<T>() where T : class
        {
            var monoBehaviours = UnityEngine.Object.FindObjectsOfType<Transform>();

            return monoBehaviours.Select(behaviour => behaviour.GetComponent(typeof(T))).OfType<T>().ToArray();
        }

        /// <summary>
        /// Find all Components of specified interface along with Component itself
        /// </summary>
        public static ComponentOfInterface<T>[] FindObjectsOfInterfaceAsComponents<T>() where T : class
        {
            return UnityEngine.Object.FindObjectsOfType<Component>().Where(c => c is T).Select(c => new ComponentOfInterface<T>(c, c as T)).ToArray();
        }

        #endregion
    }
}