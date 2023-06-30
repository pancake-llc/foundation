using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;

namespace Pancake.Tag
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
            if (tag != null && !tag.IsDefault) TagStatic.Unregister(gameObject, tag);
            tag = newTag;
            if (tag != null && !tag.IsDefault) TagStatic.Register(gameObject, tag);
            initialized = Application.isPlaying;
        }

        public string TagLabel() => (tag == null ? "None" : tag.name);


        private bool initialized = false;

        private void Awake()
        {
            TagGameObjectComponent.Init();
            //SetTag could have been called before awake
            if (!initialized)
            {
                TagStatic.AwakeComplete = false;
                Init();
            }
        }

        public void Init()
        {
            if (initialized) return;

            initialized = Application.isPlaying;
            // If the tag is set to default / empty then don't tag this GameObject
            if (tag == null || tag.IsDefault) return;
            TagStatic.Register(gameObject, tag);
        }

        private void Start()
        {
            TagStatic.AwakeComplete = true;
            triggerOnStart = TagGameObjectComponent.AddIfListening(this, tag?.OnGameObjectStart);
            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Start);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        private bool triggerOnStart = false;

        internal void TriggerOnStart()
        {
            triggerOnStart = false;
            tag?.OnGameObjectStart?.Invoke(gameObject);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        private bool triggerOnEnable = false;

        private void OnEnable()
        {
            triggerOnEnable = TagGameObjectComponent.AddIfListening(this, tag?.OnGameObjectEnabled);
            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Enabled);
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

        private void OnDisable()
        {
            if (tag != null) tag.OnGameObjectDisabled?.Invoke(gameObject);
            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Disabled);
        }

        private void OnDestroy()
        {
            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Destroyed);
            if (tag != null)
            {
                tag.OnGameObjectDestroyed?.Invoke(gameObject);
                // When this GameObject is destroyed, remove its reference from the lookup list
                TagStatic.Unregister(gameObject, tag);
            }

            TagGameObjectComponent.RemoveIfListening(this);
        }
    }
}