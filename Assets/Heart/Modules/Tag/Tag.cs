using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pancake.ExTag
{
    [EditorIcon("icon_enum")]
    public class Tag : GameComponent
    {
        /// <summary>
        /// Get the tags associated with this GameObject as `StringConstants` in a `ReadOnlyList&lt;T&gt;`.
        /// </summary>
        /// <value>The tags associated with this GameObject as `StringConstants` in a `ReadOnlyList&lt;T&gt;`.</value>
        public ReadOnlyList<StringConstant> Tags
        {
            get
            {
                if (_readOnlyTags == null || _readOnlyTags.Count != _sortedTags.Values.Count)
                {
                    _readOnlyTags = new ReadOnlyList<StringConstant>(_sortedTags.Values);
                }

                return _readOnlyTags;
            }
            private set => _readOnlyTags = value;
        }

        private ReadOnlyList<StringConstant> _readOnlyTags;

        [SerializeField] private List<StringConstant> tags = new();

        private SortedList<string, StringConstant> _sortedTags = new();

        private static readonly Dictionary<string, List<GameObject>> TaggedGameObjects = new();

        private static readonly Dictionary<GameObject, Tag> TagInstances = new();
        private static Action onInitialization;

        #region Serialization

        protected override void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !EditorApplication.isUpdating && !EditorApplication.isCompiling) return;
#endif
            tags.Clear();
            foreach (var kvp in _sortedTags)
            {
                tags.Add(kvp.Value);
            }
        }

        protected override void OnAfterDeserialize()
        {
            _sortedTags = new SortedList<string, StringConstant>();

            for (int i = 0; i != tags.Count; i++)
            {
                if (tags[i] == null || tags[i].Value == null) continue;
                if (_sortedTags.ContainsKey(tags[i].Value))
                {
                    Debug.Log($"Same key [\"{tags[i].Value}\"] already exist!");
                    continue;
                }

                _sortedTags.Add(tags[i].Value, tags[i]);
            }
        }

        #endregion

        #region Lifecycles

        protected void OnEnable()
        {
            if (!IsInitialized(gameObject))
            {
                TaggedGameObjects.Clear();
                TagInstances.Clear();
                var tagsInScene = FindObjectsByType<Tag>(FindObjectsSortMode.None);

                for (var i = 0; i < tagsInScene.Length; ++i)
                {
                    var t = tagsInScene[i];
                    var tagCount = t.Tags.Count;
                    var go = tagsInScene[i].gameObject;
                    TagInstances.TryAdd(go, t);
                    for (var y = 0; y < tagCount; ++y)
                    {
                        var stringConstant = t.Tags[y];
                        if (stringConstant == null) continue;
                        var v = stringConstant.Value;
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
            for (var i = 0; i < Tags.Count; i++)
            {
                var stringConstant = Tags[i];
                if (stringConstant == null) continue;
                var v = stringConstant.Value;
                if (TaggedGameObjects.TryGetValue(v, out var o)) o.Remove(gameObject);
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
        public bool HasTag(string tag)
        {
            if (tag == null) return false;
            return _sortedTags.ContainsKey(tag);
        }

        /// <summary>
        /// Add a tag to this `GameObject`.
        /// </summary>
        /// <param name="tag">The tag to add as a `StringContant`.</param>
        public void AddTag(StringConstant tag)
        {
            if (tag == null || tag.Value == null) return;
            if (_sortedTags.ContainsKey(tag.Value)) return;
            _sortedTags.Add(tag.Value, tag);

            Tags = new ReadOnlyList<StringConstant>(_sortedTags.Values);

            // Update static accessors:
            if (!TaggedGameObjects.ContainsKey(tag.Value)) TaggedGameObjects.Add(tag.Value, new List<GameObject>());
            TaggedGameObjects[tag.Value].Add(this.gameObject);
        }

        /// <summary>
        /// Remove a tag from this `GameObject`.
        /// </summary>
        /// <param name="tag">The tag to remove as a `string`</param>
        public void RemoveTag(string tag)
        {
            if (tag == null) return;
            if (!_sortedTags.ContainsKey(tag)) return;
            _sortedTags.Remove(tag);

            Tags = new ReadOnlyList<StringConstant>(_sortedTags.Values);

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
            if (!TaggedGameObjects.TryGetValue(tag, out var list)) return null;
            return list.Count < 1 ? null : TaggedGameObjects[tag][0];
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
        public static Tag GetTagsForGameObject(GameObject go) { return TagInstances.GetValueOrDefault(go); }

        /// <summary>
        /// Retrieves all tags for a given `GameObject`. A faster alternative to `gameObject.GetComponen&lt;UATags&gt;().Tags`.
        /// </summary>
        /// <param name="go">The `GameObject` to check for tags.</param>
        /// <returns>
        /// A `ReadOnlyList&lt;T&gt;` of tags stored as `StringContant`s. Returns `null` if the `GameObject` does not have any tags or if the `GameObject` is disabled.
        /// </returns>
        public static ReadOnlyList<StringConstant> GetTags(GameObject go) { return !TagInstances.TryGetValue(go, out var t) ? null : t.Tags; }

        private static bool IsInitialized(GameObject go) => TagInstances.ContainsKey(go);
    }
}