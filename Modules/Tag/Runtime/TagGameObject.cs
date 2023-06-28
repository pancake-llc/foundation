using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;

namespace Pancake.BTag
{
    [EditorIcon("tag_icon")]
    public class TagGameObject : MonoBehaviour, ITaggedGameObject
    {
        // The Tag - replaces Component.tag mostly to avoid confusion and
        // as I believe it is rarely used
        public new Tag tag;

        // If changing the tag at runtime please use 'SetTag()' to ensure the new tag is registered
        public void SetTag(Tag newTag)
        {
            if (newTag == tag) return;
            if (tag != null && !tag.IsDefault) BTag.Unregister(gameObject, tag);
            tag = newTag;
            if (tag != null && !tag.IsDefault) BTag.Register(gameObject, tag);
            initialized = Application.isPlaying;
        }

        public string TagLabel() => (tag == null ? "None" : tag.name);


        bool initialized = false;

        void Awake()
        {
            TagGameObjectMasterUpdate.Init();
            //SetTag could have been called before awake
            if (!initialized)
            {
                BTag.AwakeComplete = false;
                Init();
            }
        }

        public void Init()
        {
            if (initialized) return;

            initialized = Application.isPlaying;
            // If the tag is set to default / empty then don't tag this GameObject
            if (tag == null || tag.IsDefault) return;
            BTag.Register(gameObject, tag);
        }

        void Start()
        {
            BTag.AwakeComplete = true;
            triggerOnStart = TagGameObjectMasterUpdate.AddIfListening(this, tag?.OnGameObjectStart);
            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Start);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        bool triggerOnStart = false;

        internal void TriggerOnStart()
        {
            triggerOnStart = false;
            tag?.OnGameObjectStart?.Invoke(gameObject);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        bool triggerOnEnable = false;

        void OnEnable()
        {
            triggerOnEnable = TagGameObjectMasterUpdate.AddIfListening(this, tag?.OnGameObjectEnabled);
            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Enabled);
        }

        internal void TriggerOnEnable()
        {
            triggerOnEnable = false;
            tag?.OnGameObjectEnabled?.Invoke(gameObject);
        }

        public void InvokeIfRequired()
        {
            if (triggerOnEnable) TriggerOnEnable();
            if (triggerOnStart) TriggerOnStart();
        }

        void OnDisable()
        {
            if (tag != null) tag.OnGameObjectDisabled?.Invoke(gameObject);
            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Disabled);
        }

        void OnDestroy()
        {
            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Destroyed);
            if (tag != null)
            {
                tag.OnGameObjectDestroyed?.Invoke(gameObject);
                // When this GameObject is destroyed, remove its reference from the lookup list
                BTag.Unregister(gameObject, tag);
            }

            TagGameObjectMasterUpdate.RemoveIfListening(this);
        }
    }
}