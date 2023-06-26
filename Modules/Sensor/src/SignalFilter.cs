using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class TagSelectorAttribute : PropertyAttribute
    {
    }

    [Serializable]
    public class SignalFilter
    {
        [Tooltip("Any GameObject in this list will not be detected by this sensor.")]
        public List<GameObject> IgnoreList = new List<GameObject>();

        [Tooltip("When set to true the sensor will only detect objects whose tags are in the 'withTag' array.")]
        public bool EnableTagFilter;

        [Tooltip("Array of tags that will be detected by the sensor.")] [TagSelector]
        public string[] AllowedTags;

        public bool IsNull()
        {
            foreach (var go in IgnoreList)
            {
                if (go != null)
                {
                    return false;
                }
            }

            if (AllowedTags == null)
            {
                return true;
            }

            foreach (var tag in AllowedTags)
            {
                if (tag != null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TestCollider(Collider col) => TestCollider(col.gameObject, col.attachedRigidbody?.gameObject);
        public bool TestCollider(Collider2D col) => TestCollider(col.gameObject, col.attachedRigidbody?.gameObject);

        bool TestCollider(GameObject go, GameObject rbGo)
        {
            if (IsNull())
            {
                return true;
            }

            if (!IsPassingIgnoreList(go) || (rbGo != null && !IsPassingIgnoreList(rbGo)))
            {
                return false;
            }

            return true;
        }

        public bool IsPassingTagFilter(GameObject go)
        {
            if (EnableTagFilter)
            {
                var tagFound = false;
                for (int i = 0; i < AllowedTags.Length; i++)
                {
                    if (AllowedTags[i] != "" && go != null && go.CompareTag(AllowedTags[i]))
                    {
                        tagFound = true;
                        break;
                    }
                }

                if (!tagFound)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsPassingIgnoreList(GameObject go)
        {
            for (int i = 0; i < IgnoreList.Count; i++)
            {
                if (ReferenceEquals(IgnoreList[i], go))
                {
                    return false;
                }
            }

            return true;
        }
    }
}