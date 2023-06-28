using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pancake.BTag
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

        bool initialized = false;

        void Awake()
        {
            if (!initialized)
            {
                BTag.AwakeComplete = false;
                Init();
            }
        }

        public void Init()
        {
            if (!initialized) RegisterTags();
        }

        void RegisterTags()
        {
            if (tags == null) return;
            initialized = Application.isPlaying;
            for (int t = 0; t < tags.Length; ++t)
            {
                var tag = tags[t];
                // If the tag is set to default / empty then don't tag this GameObject
                if (tag == null || tag.IsDefault) continue;
                BTag.Register(gameObject, tag);
            }
        }

        private void UnRegisterTags()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                if (tags[t] == null || tags[t].IsDefault) continue;
                BTag.Unregister(gameObject, tags[t]);
            }
        }

        void Start()
        {
            BTag.AwakeComplete = true;
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                triggerOnStart = TagGameObjectMasterUpdate.AddIfListening(this, tags[t]?.OnGameObjectStart);
                if (triggerOnStart) break;
            }

            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Start);
        }

        // Invoke in LateUpdate, giving user scripts an opportunity to Awake/Enable
        bool triggerOnStart = false;

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
        bool triggerOnEnable = false;

        void OnEnable()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                triggerOnEnable = TagGameObjectMasterUpdate.AddIfListening(this, tags[t]?.OnGameObjectEnabled);
                if (triggerOnEnable) break;
            }

            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Enabled);
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

        void OnDisable()
        {
            if (tags == null) return;
            for (int t = 0; t < tags.Length; ++t)
            {
                tags[t].OnGameObjectDisabled?.Invoke(gameObject);
            }

            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Disabled);
        }

        void OnDestroy()
        {
            if (BTag.HasGlobalListeners) BTag.CheckGlobalQueries(transform, BTag.GOEventType.Destroyed);
            if (tags != null)
            {
                for (int t = 0; t < tags.Length; ++t)
                {
                    if (tags[t] == null || tags[t].IsDefault) continue;
                    tags[t].OnGameObjectDestroyed?.Invoke(gameObject);
                    BTag.Unregister(gameObject, tags[t]);
                }
            }

            TagGameObjectMasterUpdate.RemoveIfListening(this);
        }
    }
}