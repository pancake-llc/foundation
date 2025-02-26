using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using UnityEngine;

namespace Pancake.ExTag
{
    [EditorIcon("icon_enum")]
    public class Tag : GameComponent
    {
        [SerializeField] private List<StringKey> tags = new();
        private static readonly Dictionary<string, List<GameObject>> TaggedGameObjects = new();
        private static readonly Dictionary<GameObject, Tag> TagInstances = new();
        private static Action onInitialization;

        protected override void OnAfterDeserialize()
        {
            tags.RemoveAll(t => t == null);
        }

        #region Lifecycles

        protected void OnEnable()
        {
            if (!IsInitialized(gameObject))
            {
                TaggedGameObjects.Clear();
                TagInstances.Clear();
                var tagsInScene = FindObjectsByType<Tag>(FindObjectsSortMode.None);
                if (tagsInScene.IsNullOrEmpty()) return;
                
                foreach (var t in tagsInScene)
                {
                    var go = t.gameObject;
                    TagInstances.TryAdd(go, t);
                    foreach (var key in t.tags)
                    {
                        if (key == null) continue;
                        var v = key.Name;
                        if (!TaggedGameObjects.ContainsKey(v)) TaggedGameObjects.Add(v, new List<GameObject>());
                        TaggedGameObjects[v].Add(go);
                    }
                }

                onInitialization?.Invoke();
                onInitialization = null;
            }
        }

        protected void OnDisable()
        {
            if (TagInstances.ContainsKey(gameObject)) TagInstances.Remove(gameObject);
            foreach (var key in tags.ToList())
            {
                if (key == null) continue;
                if (TaggedGameObjects.TryGetValue(key.Name, out var o)) o.Remove(gameObject);
            }
        }

        #endregion

        public static void OnInitialization(Action handler)
        {
            var t = FindFirstObjectByType<Tag>();
            if (t != null && !IsInitialized(t.gameObject))
            {
                onInitialization += handler;
            }
            else
            {
                handler();
            }
        }

        /// <summary>
        /// Check if the tag provided is associated with this `GameObject`.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>`true` if the tag exists, otherwise `false`.</returns>
        public bool HasTag(string tag) => !string.IsNullOrEmpty(tag) && tags.Exists(k => k.Name == tag);

        /// <summary>
        /// Add a tag to this `GameObject`.
        /// </summary>
        /// <param name="tag">The tag to add as a `StringContant`.</param>
        public void AddTag(StringKey tag)
        {
            if (tag == null || tag.Name == null) return;
            if (HasTag(tag.Name)) return;
            tags.Add(tag);

            // Update static accessors:
            if (!TaggedGameObjects.ContainsKey(tag.Name)) TaggedGameObjects.Add(tag.Name, new List<GameObject>());
            TaggedGameObjects[tag.Name].Add(this.gameObject);
        }

        /// <summary>
        /// Remove a tag from this `GameObject`.
        /// </summary>
        /// <param name="tag">The tag to remove as a `string`</param>
        public void RemoveTag(string tag)
        {
            if (tag == null) return;
            if (!HasTag(tag)) return;
            tags.RemoveAll(k => k.Name == tag);

            // Update static accessors:
            if (!TaggedGameObjects.TryGetValue(tag, out var list)) return; // this should never happen
            list.Remove(gameObject);
        }

        /// <summary>
        /// Find first `GameObject` that has the tag provided.
        /// </summary>
        /// <param name="tag">The tag that the `GameObject` that you search for will have.</param>
        /// <returns>The first `GameObject` with the provided tag found. If no `GameObject`is found, it returns `null`.</returns>
        public static GameObject FindByTag(string tag)
        {
            return TaggedGameObjects.TryGetValue(tag, out var list) && list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Find all `GameObject`s that have the tag provided.
        /// </summary>
        /// <param name="tag">The tag that the `GameObject`s that you search for will have.</param>
        /// <returns>An array of `GameObject`s with the provided tag. If not found it returns `null`.</returns>
        public static GameObject[] FindAllByTag(string tag) { return !TaggedGameObjects.TryGetValue(tag, out var list) ? null : list.ToArray(); }

        /// <summary>
        /// Find all `GameObject`s that have the tag provided. Mutates the output `List&lt;GameObject&gt;` and adds the `GameObject`s found to it.
        /// </summary>
        /// <param name="tag">The tag that the `GameObject`s that you search for will have.</param>
        /// <param name="output">A `List&lt;GameObject&gt;` that this method will clear and add the `GameObject`s found to.</param>
        public static void FindAllByTagNoAlloc(string tag, List<GameObject> output)
        {
            output.Clear();
            if (!TaggedGameObjects.ContainsKey(tag)) return;
            for (var i = 0; i < TaggedGameObjects[tag].Count; ++i)
            {
                output.Add(TaggedGameObjects[tag][i]);
            }
        }

        /// <summary>
        /// A faster alternative to `gameObject.GetComponen&lt;UATags&gt;()`.
        /// </summary>
        /// <returns>
        /// Returns the `UATags` component. Returns `null` if the `GameObject` does not have a `UATags` component or if the `GameObject` is disabled.
        /// </returns>
        public static Tag GetTagsForGameObject(GameObject go) => TagInstances.GetValueOrDefault(go);

        /// <summary>
        /// Retrieves all tags for a given `GameObject`. A faster alternative to `gameObject.GetComponen&lt;UATags&gt;().Tags`.
        /// </summary>
        /// <param name="go">The `GameObject` to check for tags.</param>
        /// <returns>
        /// A `ReadOnlyList&lt;T&gt;` of tags stored as `StringContant`s. Returns `null` if the `GameObject` does not have any tags or if the `GameObject` is disabled.
        /// </returns>
        public static IReadOnlyList<StringKey> GetTags(GameObject go) { return !TagInstances.TryGetValue(go, out var t) ? null : t.tags.AsReadOnly(); }

        private static bool IsInitialized(GameObject go) => TagInstances.ContainsKey(go);
    }
}