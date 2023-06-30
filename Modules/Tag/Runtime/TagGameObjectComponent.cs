using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Tag
{
    public interface ITaggedGameObject
    {
        void InvokeIfRequired();
    }

    public class TagGameObjectComponent : MonoBehaviour
    {
        private static GameObject instance;
        internal static List<ITaggedGameObject> TaggedGameObjectsWithEvents = new List<ITaggedGameObject>();

        public static void Init()
        {
            if (instance == null)
            {
                instance = new GameObject("TaggedGameObjectComponent", typeof(TagGameObjectComponent)) {hideFlags = HideFlags.HideAndDontSave};
            }
        }

        internal static bool AddIfListening(ITaggedGameObject taggedGO, Action<GameObject> action)
        {
            if (action != null)
            {
                if (!TaggedGameObjectsWithEvents.Contains(taggedGO)) TaggedGameObjectsWithEvents.Add(taggedGO);
                return true;
            }

            return false;
        }

        internal static void RemoveIfListening(ITaggedGameObject taggedGO)
        {
            if (TaggedGameObjectsWithEvents.Contains(taggedGO)) TaggedGameObjectsWithEvents.Remove(taggedGO);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < TaggedGameObjectsWithEvents.Count; ++i)
            {
                TaggedGameObjectsWithEvents[i].InvokeIfRequired();
            }

            TagStatic.CheckQueuedGlobalQueriesLateUpdate();
#if UNITY_EDITOR
            TagStatic.ResetWarnCount();
#endif
        }
    }
}