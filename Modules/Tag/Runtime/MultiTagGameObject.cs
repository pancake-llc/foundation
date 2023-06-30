using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pancake.Tag
{
    [EditorIcon("tag_icon")]
    public class MultiTagGameObject : MonoBehaviour, ITaggedGameObject
    {
        // The Tag - replaces Component.tag mostly to avoid confusion and
        // as I believe it is rarely used
        public Tag[] tags = new Tag[0];

        // If changing the tags at runtime please use 'SetTag()' to ensure the new tag is registered
        public void SetTags(Tag[] newTags)
        {
            UnRegisterTags();
            tags = newTags;
            RegisterTags();
        }

        private bool initialized = false;

        private void Awake()
        {
            if (!initialized)
            {
                TagStatic.AwakeComplete = false;
                Init();
            }
        }

        public void Init()
        {
            if (!initialized) RegisterTags();
        }

        private void RegisterTags()
        {
            if (tags == null) return;
            initialized = Application.isPlaying;
            for (int t = 0; t < tags.Length; ++t)
            {
                var tag = tags[t];
                // If the tag is set to default / empty then don't tag this GameObject
                if (tag == null || tag.IsDefault) continue;
                TagStatic.Register(gameObject, tag);
            }
        }

        private void UnRegisterTags()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                if (tags[t] == null || tags[t].IsDefault) continue;
                TagStatic.Unregister(gameObject, tags[t]);
            }
        }

        private void Start()
        {
            TagStatic.AwakeComplete = true;
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                triggerOnStart = TagGameObjectComponent.AddIfListening(this, tags[t]?.OnGameObjectStart);
                if (triggerOnStart) break;
            }

            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Start);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        private bool triggerOnStart = false;

        internal void TriggerOnStart()
        {
            triggerOnStart = false;
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                if (tags[t] != null) tags[t].OnGameObjectStart?.Invoke(gameObject);
            }
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        private bool triggerOnEnable = false;

        private void OnEnable()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                triggerOnEnable = TagGameObjectComponent.AddIfListening(this, tags[t]?.OnGameObjectEnabled);
                if (triggerOnEnable) break;
            }

            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Enabled);
        }

        internal void TriggerOnEnable()
        {
            triggerOnEnable = false;
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                tags[t]?.OnGameObjectEnabled?.Invoke(gameObject);
            }
        }

        void ITaggedGameObject.InvokeIfRequired()
        {
            if (triggerOnEnable) TriggerOnEnable();
            if (triggerOnStart) TriggerOnStart();
        }

        private void OnDisable()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                tags[t].OnGameObjectDisabled?.Invoke(gameObject);
            }

            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Disabled);
        }

        private void OnDestroy()
        {
            if (TagStatic.HasGlobalListeners) TagStatic.CheckGlobalQueries(transform, TagStatic.GOEventType.Destroyed);
            if (tags != null)
            {
                for (int t = 0; t < tags.Length; ++t)
                {
                    if (tags[t] == null || tags[t].IsDefault) continue;
                    tags[t].OnGameObjectDestroyed?.Invoke(gameObject);
                    TagStatic.Unregister(gameObject, tags[t]);
                }
            }

            TagGameObjectComponent.RemoveIfListening(this);
        }
    }
}