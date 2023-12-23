using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Scriptable.ExLib
{
    public static class Extension
    {
        public static T Get<T>(this ScriptableListGameObject list) where T : Component
        {
            if (list is null || !list) return default(T);

            foreach (var gameObject in list)
            {
                if (!gameObject.TryGetComponent(out T value)) continue;
                return value;
            }

            return default(T);
        }

        public static T Get<T>(this ScriptableListGameObject list, Func<GameObject, bool> predicate) where T : Component
        {
            if (list is null || !list) return default(T);

            foreach (var gameObject in list)
            {
                if (!predicate(gameObject)) continue;
                if (!gameObject.TryGetComponent(out T value)) continue;
                return value;
            }

            return default(T);
        }

        public static IEnumerable<T> GetList<T>(this ScriptableListGameObject list) where T : Component
        {
            if (list is null || !list) return new List<T>();

            var results = new List<T>();
            foreach (var gameObject in list)
            {
                if (!gameObject.TryGetComponent(out T value)) continue;
                results.Add(value);
            }

            return results;
        }

        public static IEnumerable<T> GetList<T>(this ScriptableListGameObject list, Func<GameObject, bool> predicate) where T : Component
        {
            if (list is null || !list) return new List<T>();

            var results = new List<T>();
            foreach (var gameObject in list)
            {
                if (!predicate(gameObject)) continue;
                if (!gameObject.TryGetComponent(out T value)) continue;
                results.Add(value);
            }

            return results;
        }
    }
}