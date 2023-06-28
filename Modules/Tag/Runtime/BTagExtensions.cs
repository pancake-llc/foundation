using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Pancake.BTag
{
    public static class BTag
    {
        [NonSerialized] public static List<Tag> AllTags = new List<Tag>();

        // Fast-ish lookup for tagged GameObjects - populated on Awake per GameObject
        internal static bool AwakeComplete = false;
        public static bool StoreTagNames = false;
        internal static Dictionary<int, GameObject> _AllTaggedGOs = new Dictionary<int, GameObject>();
        internal static Dictionary<int, int> _AllGOsTagCount = new Dictionary<int, int>();

        public static Dictionary<int, GameObject> AllTaggedGOs
        {
            get
            {
#if UNITY_EDITOR
                // If in the Editor and not playing, find all tagged GameObjects in open scenes and use those for search methods
                if (!Application.isPlaying) FindInactiveTags();
#endif
                return _AllTaggedGOs;
            }
        }

        internal static Dictionary<BHash128, HashSet<int>> _AllTaggedGOsByIndex = new Dictionary<BHash128, HashSet<int>>(new BHash128EqualityComparer());

        public static Dictionary<BHash128, HashSet<int>> AllTaggedGOsByIndex
        {
            get
            {
                // If AllTaggedGOs are requested before Awake has completed for all MonoBehaviours, we manually invoke this components Init method
                if (!AwakeComplete)
                {
                    var components = GameObject.FindObjectsOfType<TagGameObject>();
                    for (int c = 0; c < components.Length; ++c) components[c].Init();
                    var multiComponents = GameObject.FindObjectsOfType<MultiTagGameObject>();
                    for (int c = 0; c < multiComponents.Length; ++c) multiComponents[c].Init();
                    AwakeComplete = true;
                }

                return _AllTaggedGOsByIndex;
            }
        }

#if UNITY_EDITOR
        internal static void ResetWarnCount() { WarnCount = 0; }
#endif

        public static void FindInactiveTags()
        {
            var components = Resources.FindObjectsOfTypeAll<TagGameObject>();
            for (int c = 0; c < components.Length; ++c) components[c].Init();
            var multiComponents = Resources.FindObjectsOfTypeAll<MultiTagGameObject>();
            for (int c = 0; c < multiComponents.Length; ++c) multiComponents[c].Init();
        }


        /* Experimental WIP*/
        internal static Dictionary<BTagQueryGameObject, Action<GameObject>> OnGameObjectStartListeners = new Dictionary<BTagQueryGameObject, Action<GameObject>>();
        internal static Dictionary<BTagQueryGameObject, Action<GameObject>> OnGameObjectEnabledListeners = new Dictionary<BTagQueryGameObject, Action<GameObject>>();
        internal static Dictionary<BTagQueryGameObject, Action<GameObject>> OnGameObjectDisabledListeners = new Dictionary<BTagQueryGameObject, Action<GameObject>>();
        internal static Dictionary<BTagQueryGameObject, Action<GameObject>> OnGameObjectDestroyedListeners = new Dictionary<BTagQueryGameObject, Action<GameObject>>();

        internal static bool HasGlobalListeners = false;

        // Events can fire from multiple components per frame
        // We want to ignore those but catch e.g. an OnEnable & OnDisable on the same frame
        internal static List<Transform> StartTransforms = new List<Transform>();
        internal static List<Transform> EnabledTransforms = new List<Transform>();
        internal static HashSet<Transform> DisabledTransforms = new HashSet<Transform>();
        internal static HashSet<Transform> DestroyedTransforms = new HashSet<Transform>();

        internal enum GOEventType
        {
            Start,
            Enabled,
            Disabled,
            Destroyed
        }

        internal static void CheckGlobalQueries(Transform t, GOEventType evt)
        {
            switch (evt)
            {
                case GOEventType.Start:
                    if (StartTransforms.Contains(t)) return;
                    StartTransforms.Add(t);
                    break;
                case GOEventType.Enabled:
                    if (EnabledTransforms.Contains(t)) return;
                    EnabledTransforms.Add(t);
                    break;
                case GOEventType.Disabled:
                    if (DisabledTransforms.Contains(t)) return;
                    CheckGlobalQueriesImmediate(t, evt);
                    break;
                default:
                    if (DestroyedTransforms.Contains(t)) return;
                    CheckGlobalQueriesImmediate(t, evt);
                    break;
            }
        }

        internal static void CheckGlobalQueriesImmediate(Transform t, GOEventType evt)
        {
            if (!HasGlobalListeners) return;
            Dictionary<BTagQueryGameObject, Action<GameObject>> listeners;
            switch (evt)
            {
                case GOEventType.Disabled:
                    DisabledTransforms.Add(t);
                    listeners = OnGameObjectDisabledListeners;
                    break;
                default:
                    DestroyedTransforms.Add(t);
                    listeners = OnGameObjectDestroyedListeners;
                    break;
            }

            foreach (var listener in listeners)
            {
                var query = listener.Key;
                query.InitialTargets = null;
                query.InitialTargets = new HashSet<GameObject> {t.gameObject};
                if (query.GetFirstGameObject() == t.gameObject) listener.Value?.Invoke(t.gameObject);
            }
        }

        internal static void CheckQueuedGlobalQueriesLateUpdate()
        {
            if (!HasGlobalListeners) return;

            foreach (var listener in OnGameObjectStartListeners)
            {
                var query = listener.Key;
                for (int i = 0; i < StartTransforms.Count; ++i)
                {
                    query.InitialTargets = new HashSet<GameObject> {StartTransforms[i].gameObject};
                    if (query.GetFirstGameObject() == StartTransforms[i].gameObject) listener.Value?.Invoke(StartTransforms[i].gameObject);
                }
            }

            foreach (var listener in OnGameObjectEnabledListeners)
            {
                var query = listener.Key;
                for (int i = 0; i < EnabledTransforms.Count; ++i)
                {
                    query.InitialTargets = new HashSet<GameObject> {EnabledTransforms[i].gameObject};
                    if (query.GetFirstGameObject() == EnabledTransforms[i].gameObject) listener.Value?.Invoke(EnabledTransforms[i].gameObject);
                }
            }

            StartTransforms.Clear();
            EnabledTransforms.Clear();
            DisabledTransforms.Clear();
            DestroyedTransforms.Clear();
        }

        public static void AddListenerOnStart(this BTagQueryGameObject query, Action<GameObject> onEnabled)
        {
            HasGlobalListeners = true;
            OnGameObjectStartListeners.Add(query, onEnabled);
        }

        public static void RemoveListenerOnStart(this BTagQueryGameObject query) { OnGameObjectStartListeners.Remove(query); }

        public static void AddListenerOnEnabled(this BTagQueryGameObject query, Action<GameObject> onEnabled)
        {
            HasGlobalListeners = true;
            OnGameObjectEnabledListeners.Add(query, onEnabled);
        }

        public static void RemoveListenerOnEnabled(this BTagQueryGameObject query) { OnGameObjectEnabledListeners.Remove(query); }

        public static void AddListenerOnDisabled(this BTagQueryGameObject query, Action<GameObject> onEnabled)
        {
            HasGlobalListeners = true;
            OnGameObjectDisabledListeners.Add(query, onEnabled);
        }

        public static void RemoveListenerOnDisabled(this BTagQueryGameObject query) { OnGameObjectDisabledListeners.Remove(query); }

        public static void AddListenerOnDestroyed(this BTagQueryGameObject query, Action<GameObject> onEnabled)
        {
            HasGlobalListeners = true;
            OnGameObjectDestroyedListeners.Add(query, onEnabled);
        }

        public static void RemoveListenerOnDestroyed(this BTagQueryGameObject query) { OnGameObjectDestroyedListeners.Remove(query); }
        /*-----------------*/


        internal static void Register(GameObject go, Tag tag)
        {
            // If it's the first time the tag is registered, create a list
            if (!BTag._AllTaggedGOsByIndex.ContainsKey(tag.Hash))
            {
                BTag._AllTaggedGOsByIndex.Add(tag.Hash, new HashSet<int>());
            }

            // Only store full Tag if they are being found by name via the API
            if (BTag.StoreTagNames && !tag.IsDefault && !BTag.AllTags.Contains(tag))
            {
                BTag.AllTags.Add(tag);
            }

            // Add this gameObject to the list associated with the tag
            int goID = go.GetInstanceID();
            if (!BTag._AllTaggedGOs.ContainsKey(goID))
            {
                BTag._AllTaggedGOs.Add(goID, go);
            }

            if (!BTag._AllGOsTagCount.ContainsKey(goID))
            {
                BTag._AllGOsTagCount.Add(goID, 0);
            }

            BTag._AllGOsTagCount[goID]++;

            var idxArray = BTag._AllTaggedGOsByIndex[tag.Hash];
            if (!idxArray.Contains(goID))
            {
                idxArray.Add(goID);
                BTag._AllTaggedGOsByIndex[tag.Hash] = idxArray;
            }
        }

        internal static void Unregister(GameObject go, Tag tag)
        {
            //Debug.Log("Unregistering tag " + tag + " for " + go);
            if (tag.Hash.IsValid)
            {
                int goID = go.GetInstanceID();
                // Remove this GameObject from the list of tracked GameObjects
                if (BTag._AllGOsTagCount.ContainsKey(goID))
                {
                    if (BTag._AllGOsTagCount[goID] > 1)
                    {
                        BTag._AllGOsTagCount[goID]--;
                    }
                    else
                    {
                        BTag._AllGOsTagCount[goID] = 0;
                        if (BTag._AllTaggedGOs.ContainsKey(goID)) BTag._AllTaggedGOs.Remove(goID);
                    }
                }

                if (BTag._AllTaggedGOsByIndex.ContainsKey(tag.Hash))
                {
                    var goIDHashSet = BTag._AllTaggedGOsByIndex[tag.Hash];
                    if (goIDHashSet.Contains(goID))
                    {
                        goIDHashSet.Remove(goID);
                        BTag._AllTaggedGOsByIndex[tag.Hash] = goIDHashSet;
                    }
                }

                // Only tracking full Tags if they are being found by name via the API
                if (BTag.StoreTagNames && BTag.AllTags.Contains(tag))
                {
                    BTag.AllTags.Remove(tag);
                }
            }
        }

        public static bool IgnoreEditorChecks = false;

#if UNITY_EDITOR
        private static int WarnCount = 0;

        private static void CheckForMultipleResults(IEnumerable<UnityEngine.Object> results, IEnumerable<TagHashWithRules> tags)
        {
            if (WarnCount < 20 && results != null && results.Count() > 1)
            {
                string
                    allTags = ""; // string.Join(",", tags.Select( x => { return (x.rule == InclusionRule.MustExclude ? "<Color=red>-" : "<Color=yellow>+") + x.tag.name + "</Color>"; }) );
                allTags += "Have Any: " + string.Join(",", tags.Where(x => x.rule == InclusionRule.Any).Select(y => "<b>" + FindTagNameForHash(y.hash) + "</b>"));
                allTags += "\nMust Include: " + string.Join(",",
                    tags.Where(x => x.rule == InclusionRule.MustInclude).Select(y => "<b>" + FindTagNameForHash(y.hash) + "</b>"));
                allTags += "\nMust Exclude: " + string.Join(",",
                    tags.Where(x => x.rule == InclusionRule.MustExclude).Select(y => "<b>" + FindTagNameForHash(y.hash) + "</b>"));
                string allMatches = string.Join(", ", results);
                Debug.LogWarning("More than one GameObject matches the following query\n" + allTags +
                                 "\nThe first object will be returned but may be non-deterministic. Use .AnyIsFine() with a query when getting the same object back every time is unimportant.\n" +
                                 allMatches);
                WarnCount++;
                if (WarnCount == 20) Debug.LogWarning("There may be more warnings - capping at " + WarnCount + " warnings per frame.");
            }
        }

        internal static string FindTagNameForHash(BHash128 hash)
        {
            var t = FindTagForHash(hash);
            return t == default ? "untagged" : t.name;
        }

        internal static Tag FindTagForHash(BHash128 hash)
        {
            Tag result = default;
            string[] groupSOGuids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TagGroup));
            for (int i = 0; i < groupSOGuids.Length; ++i)
            {
                object[] tagAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(groupSOGuids[i]));
                if (tagAssets == null) continue;
                for (int t = 0; t < tagAssets.Length; ++t)
                {
                    if (tagAssets[t] != null && tagAssets[t] is Tag && (tagAssets[t] as Tag).Hash == hash)
                    {
                        result = tagAssets[t] as Tag;
                        break;
                    }
                }

                if (result != default) break;
            }

            if (result == default)
            {
                string[] individualSOGuids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Tag));
                for (int i = 0; i < individualSOGuids.Length; ++i)
                {
                    Tag individualAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(individualSOGuids[i])) as Tag;
                    if (individualAsset != null && individualAsset.Hash == hash)
                    {
                        result = individualAsset;
                        break;
                    }
                }
            }

            return result;
        }
#endif

        #region BTag Queries

        [Flags]
        public enum Search
        {
            None = 0,
            Target = 1 << 0,
            Children = 1 << 1,
            Parent = 1 << 2,
            Any = Target | Children
        }

        [Serializable]
        public struct TagQueryWithTarget
        {
            public TagQuery query;
            public Search target;
        }

        [Serializable]
        public struct TagWithRule //: UnityEngine.Object
        {
            public Tag tag;
            public InclusionRule rule;
        }

        [Serializable]
        public struct TagHashWithRules
        {
            public BHash128 hash;
            public InclusionRule rule;
            public Search searchOption;
        }

        internal static TagHashWithRules[] GetTagHashWithRules(Tag tag, InclusionRule rule = InclusionRule.Any, Search searchOption = Search.Any)
        {
            var tmp = new TagHashWithRules[1];
            tmp[0] = new TagHashWithRules() {hash = GetHash(tag), rule = rule, searchOption = searchOption};
            return tmp;
        }

        internal static TagHashWithRules[] GetTagHashesWithRules(Tag[] tags, InclusionRule rule = InclusionRule.Any, Search searchOption = Search.Any)
        {
            var tmp = new TagHashWithRules[tags.Count()];
            for (int i = 0; i < tags.Length; ++i)
            {
                tmp[i] = new TagHashWithRules() {hash = GetHash(tags[i]), rule = rule, searchOption = searchOption};
            }

            return tmp;
        }

        public class BTagQueryGameObject
        {
#if UNITY_EDITOR
            internal bool DisableEditorChecks;
#endif
            public TagHashWithRules[] TagWithSettings;
            public int[] GroupCounts;
            public HashSet<GameObject> InitialTargets;

            public BTagQueryGameObject() { Reset(); }

            internal void Reset()
            {
                TagWithSettings = new TagHashWithRules[0];
                GroupCounts = new int[0];
                InitialTargets = null;
            }
        }

        static List<GameObject> SingleResult = new List<GameObject>(1);

        public static GameObject GetFirstGameObject(this BTagQueryGameObject q)
        {
            if (SingleResult.Count < 1) SingleResult.Add(null);
            else SingleResult[0] = null;
            GetGameObjects(q, ref SingleResult, false);
            return SingleResult[0];
        }

        public static List<GameObject> GetGameObjects(this BTagQueryGameObject q, bool findAll = true)
        {
            var results = new List<GameObject>(1);
            GetGameObjects(q, ref results, findAll);
            return results;
        }

        public static C GetFirstComponent<C>(this BTagQueryGameObject q) where C : Component { return GetComponents<C>(q, false).FirstOrDefault(); }

        public static List<C> GetComponents<C>(this BTagQueryGameObject q, bool findAll = true) where C : Component
        {
            var results = new List<C>(1);
            GetComponents(q, ref results, findAll);
            return results;
        }

        /// <summary>
        /// Important: Names of Tags are not tracked by default - accessing by string will have some
        /// additional overhead as each GameObject will also cache its associated Tags.
        /// In addition, only the name of the Tag is checked - the group its in is ignored
        /// </summary>
        public static Tag ByName(string name)
        {
            if (AllTags.Count < 1)
            {
                AllTags.AddRange(GameObject.FindObjectsOfType<TagGameObject>().Select(x => x.tag));
                var multis = GameObject.FindObjectsOfType<MultiTagGameObject>();
                for (int i = 0; i < multis.Length; ++i) AllTags.AddRange(multis[i].tags);
                // As strings are being used, after finding the initial Tags in the scene,
                // from now on also add/remove tags of any newly instantiated/destroyed GameObjects
                StoreTagNames = true;
            }

            return AllTags.FirstOrDefault(x => (x != null && x.name == name));
        }

        static int searchResultSize = 0;
        static GameObject[] searchResults = new GameObject[1024];
        static int mainWorkingResultSize = 0;
        static int[] mainWorkingResults = new int[1024];

        public static bool GetGameObjects(this BTagQueryGameObject q, ref List<GameObject> results, bool findAll = true) =>
            GetGameObjects<GameObject>(q, ref results, findAll);

        public static bool GetGameObjects<T>(this BTagQueryGameObject q, ref List<GameObject> results, bool findAll = true) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (q.DisableEditorChecks) IgnoreEditorChecks = true;
#endif
            int groupStartIdx = 0;

            int intialSize = searchResultSize = 0;
            if (q.InitialTargets != null)
            {
                searchResultSize = intialSize = q.InitialTargets.Count;
                q.InitialTargets.CopyTo(searchResults);
            }

            int groupCount = q.GroupCounts.Length;
            for (int g = 0; g < groupCount; ++g)
            {
                if (searchResultSize > 0)
                {
                    mainWorkingResultSize = 0;
                    for (int ws = 0; ws < searchResultSize; ++ws)
                    {
                        var wResults = GetTaggedGameObjectsWithTags<T>(q.TagWithSettings,
                            groupStartIdx,
                            q.GroupCounts[g],
                            searchResults[ws],
                            findAll);
                        var subResultCount = wResults.Count;
                        if (subResultCount == 0) continue;
                        while ((mainWorkingResultSize + subResultCount) > mainWorkingResults.Length)
                        {
                            Array.Resize(ref mainWorkingResults, (mainWorkingResultSize + subResultCount) * 2);
                        }

                        wResults.CopyTo(mainWorkingResults, mainWorkingResultSize);
                        mainWorkingResultSize += subResultCount;
                    }

                    // Overwrite results with last culmination of working set
                    while (mainWorkingResultSize > searchResults.Length)
                    {
                        Array.Resize(ref searchResults, mainWorkingResultSize * 2);
                    }

                    int sIdx = 0;
                    for (int i = 0; i < mainWorkingResultSize; ++i)
                    {
                        searchResults[sIdx] = AllTaggedGOs[mainWorkingResults[i]];
                        sIdx++;
                    }

                    searchResultSize = mainWorkingResultSize;
                }
                else if (g == 0)
                {
                    var wResults = GetTaggedGameObjectsWithTags<T>(q.TagWithSettings,
                        groupStartIdx,
                        q.GroupCounts[g],
                        null,
                        findAll);
                    var subResultCount = wResults.Count;
                    if (subResultCount == 0) return false;
                    while (subResultCount >= searchResults.Length)
                    {
                        Array.Resize(ref searchResults, subResultCount * 2);
                    }

                    searchResultSize = subResultCount;
                    int sIdx = 0;
                    foreach (int goIdx in wResults)
                    {
#if UNITY_EDITOR
                        if (!AllTaggedGOs.ContainsKey(goIdx)) Debug.LogAssertion("Unable to find key for GameObject with instanceID:" + goIdx);
#endif
                        searchResults[sIdx] = AllTaggedGOs[goIdx];
                        sIdx++;
                    }
                }
                else
                {
                    return false;
                }

                //Debug.Log("Checked group " + g + ": " + q.GroupCounts[g] + " with searchResultSize: " + searchResultSize);
                groupStartIdx += q.GroupCounts[g];
            }

#if UNITY_EDITOR
            IgnoreEditorChecks = false;
#endif
            // No need to resolve all GameObjects from found indicies if only require first one
            if (searchResultSize >= 1 && !findAll)
            {
                if (results.Count > 0) results[0] = searchResults[0];
                else results.Add(searchResults[0]);
                return true;
            }

            for (int i = 0; i < searchResultSize; ++i)
            {
                results.Add(searchResults[i]);
            }

            return results.Count > 0;
        }

        public static bool GetComponents<C>(this BTagQueryGameObject q, ref List<C> results, bool findAll = true) where C : Component
        {
#if UNITY_EDITOR
            if (q.DisableEditorChecks) IgnoreEditorChecks = true;
#endif
            // We only want to search for components after finding the GameObjects
            List<GameObject> matchingGOs = new List<GameObject>();
            bool found = GetGameObjects<C>(q, ref matchingGOs, findAll);
            if (found && matchingGOs.Count > 0)
            {
                for (int r = 0; r < matchingGOs.Count; ++r)
                {
                    results.AddRange(matchingGOs[r].GetComponents<C>());
                }
            }

#if UNITY_EDITOR
            if (!BTagSetting.Instance.disableEditorChecks && !IgnoreEditorChecks && !findAll) CheckForMultipleResults(results, q.TagWithSettings);
            IgnoreEditorChecks = false;
#endif
            return (results.Count() > 0);
        }

        public static BTagQueryGameObject For(GameObject gameObject)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            q.InitialTargets = new HashSet<GameObject> {gameObject};
            return q;
        }

        public static BTagQueryGameObject For(IEnumerable<GameObject> gameObjects)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            q.InitialTargets = new HashSet<GameObject>();
            var enumerator = gameObjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                q.InitialTargets.Add(enumerator.Current);
            }

            return q;
        }

        static BTagQueryGameObject AddEntryToQuery(ref BTagQueryGameObject q, TagHashWithRules hashWithRule, bool combine = false)
        {
            var settings = q.TagWithSettings;
            Array.Resize(ref settings, settings.Length + 1);
            settings[settings.Length - 1] = hashWithRule;
            q.TagWithSettings = settings;

            var counts = q.GroupCounts;
            if (combine && counts.Length > 0)
            {
                counts[counts.Length - 1]++;
            }
            else
            {
                Array.Resize(ref counts, counts.Length + 1);
                counts[counts.Length - 1] = 1;
            }

            q.GroupCounts = counts;
            return q;
        }

        static BTagQueryGameObject AddEntriesToQuery(ref BTagQueryGameObject q, Tag[] tags, InclusionRule rule, Search search, bool combine = false)
        {
            var settings = q.TagWithSettings;
            int startIdx = settings.Length;
            int badCount = 0;
            Array.Resize(ref settings, startIdx + tags.Length);
            for (int i = 0; i < tags.Length; ++i)
            {
                if (tags[i] != null && !tags[i].IsDefault)
                {
                    settings[startIdx + i - badCount] = new TagHashWithRules() {hash = tags[i], rule = rule, searchOption = search};
                }
                else
                {
                    badCount++;
                }
            }

            if (badCount == tags.Length)
            {
                // If all passed tags are invalid, store one bad entry
                // so that the group doesn't go unfiltered.
                // E.g. FindAll(someInvalidTags).With(tag) should return 0 results
                badCount--;
                settings[startIdx] = new TagHashWithRules() {hash = default, rule = InclusionRule.MustInclude, searchOption = search};
            }

            if (badCount > 0) Array.Resize(ref settings, settings.Length - badCount);
            q.TagWithSettings = settings;

            var counts = q.GroupCounts;
            if (combine && counts.Length > 0)
            {
                counts[counts.Length - 1]++;
            }
            else
            {
                Array.Resize(ref counts, counts.Length + 1);
                counts[counts.Length - 1] = tags.Length - badCount;
            }

            q.GroupCounts = counts;
            return q;
        }

        public static BTagQueryGameObject Find(TagQuery query)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            return q.Find(query);
        }

        public static BTagQueryGameObject Find(this BTagQueryGameObject q, TagQuery query, Search search = Search.Target)
        {
            if (query == null || query.IsDefault)
            {
                q.InitialTargets = null;
                return q;
            }

            for (int i = 0; i < query.matchingTags?.Length; ++i)
            {
                AddEntryToQuery(ref q,
                    new TagHashWithRules() {hash = query.matchingTags[i].tag, rule = query.matchingTags[i].rule, searchOption = search},
                    search == Search.Target);
            }

            if (query.subQueries != null)
            {
                for (int i = 0; i < query.subQueries.Length; ++i)
                {
                    q.Find(query.subQueries[i].query, query.subQueries[i].target);
                }
            }

            return q;
        }

        public static BTagQueryGameObject Find(string tag) => Find(ByName(tag));

        public static BTagQueryGameObject Find(Tag tag)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            return AddEntryToQuery(ref q,
                new TagHashWithRules() {hash = tag, rule = !GetHash(tag).IsValid ? InclusionRule.MustInclude : InclusionRule.Any, searchOption = Search.Target});
        }

        public static BTagQueryGameObject FindAny(IEnumerable<Tag> tags)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            return AddEntriesToQuery(ref q, tags.ToArray(), InclusionRule.Any, Search.Target);
        }

        public static BTagQueryGameObject FindAll(IEnumerable<Tag> tags)
        {
            BTagQueryGameObject q = new BTagQueryGameObject();
            return AddEntriesToQuery(ref q, tags.ToArray(), InclusionRule.MustInclude, Search.Target);
        }

        public static BTagQueryGameObject AnyIsFine(this BTagQueryGameObject q)
        {
#if UNITY_EDITOR
            q.DisableEditorChecks = true;
#endif
            return q;
        }

        public static bool HasTag(this GameObject gameObj, string tagName) => HasTag(gameObj, ByName(tagName));

        public static bool HasTag(this GameObject gameObj, Tag tag)
        {
            if (tag == null || tag.IsDefault || gameObj == null) return false;
            var goIndex = gameObj.GetInstanceID();
            if (AllTaggedGOsByIndex.TryGetValue(tag.Hash, out var firstTags))
            {
                return firstTags.Contains(goIndex);
            }

            return false;
        }

        public static BTagQueryGameObject ParentWith(this BTagQueryGameObject q, Tag tag, Search search = Search.Parent) => With(q, tag, search);
        public static BTagQueryGameObject ChildrenWith(this BTagQueryGameObject q, Tag tag, Search search = Search.Children) => With(q, tag, search);

        public static BTagQueryGameObject With(this BTagQueryGameObject q, Tag tag, Search search = Search.Target)
        {
            if (tag == null || tag.IsDefault)
            {
                // Clear initial targets as null was passed so nothing should match
                q.InitialTargets = null;
                return q;
            }

            return AddEntryToQuery(ref q, new TagHashWithRules() {hash = tag, rule = InclusionRule.MustInclude, searchOption = search}, search == Search.Target);
        }

        public static BTagQueryGameObject ParentWithAny(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Parent) => WithAny(q, tags, search);
        public static BTagQueryGameObject ChildrenWithAny(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Children) => WithAny(q, tags, search);

        public static BTagQueryGameObject WithAny(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Target)
        {
            return AddEntriesToQuery(ref q,
                tags.ToArray(),
                InclusionRule.Any,
                search,
                search == Search.Target);
        }

        public static BTagQueryGameObject ParentWithAll(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Parent) => WithAll(q, tags, search);
        public static BTagQueryGameObject ChildrenWithAll(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Children) => WithAll(q, tags, search);

        public static BTagQueryGameObject WithAll(this BTagQueryGameObject q, IEnumerable<Tag> tags, Search search = Search.Target)
        {
            // Can't merge with group that has Any (as wouldn't enforce All restriction)
            // Can't merge with a search for children as it would depend on initial results
            // Also can't merge if provided multiple gameobjects to search through
            bool safeToCombineSearch = search == Search.Target && !q.TagWithSettings.Any(x => x.rule == InclusionRule.Any) &&
                                       (q.GroupCounts.Length != 1 || q.InitialTargets == null || q.InitialTargets.Count <= 1);
            return AddEntriesToQuery(ref q,
                tags.ToArray(),
                InclusionRule.MustInclude,
                search,
                safeToCombineSearch);
        }

        public static BTagQueryGameObject Without(this BTagQueryGameObject q, Tag tag)
        {
            if (tag == null || tag.IsDefault)
            {
                // Clear initial targets as null was passed so nothing should match
                q.InitialTargets = null;
                return q;
            }

            AddEntryToQuery(ref q, new TagHashWithRules() {hash = tag, rule = InclusionRule.MustExclude, searchOption = Search.Target}, true);
            //if(q.GroupCounts.Length != 1)
            //{
            //    // Unless this is the only query, this should be part of a previous group
            //    // so remove the additional group we added and increase number in last group
            //    var counts = q.GroupCounts;
            //    Array.Resize(ref counts, counts.Length - 1);
            //    counts[counts.Length - 1] += 1;
            //    q.GroupCounts = counts;
            //}
            return q;
        }

        public static BTagQueryGameObject Without(this BTagQueryGameObject q, IEnumerable<Tag> tags)
        {
            AddEntriesToQuery(ref q,
                tags.ToArray(),
                InclusionRule.MustExclude,
                Search.Target,
                true);
            //if (q.GroupCounts.Length != 1)
            //{
            //    // Unless this is the only query, this should be part of a previous group
            //    // so remove the additional group we added and increase number in last group
            //    var counts = q.GroupCounts;
            //    counts[counts.Length - 2] += counts[counts.Length - 1];
            //    Array.Resize(ref counts, counts.Length - 1);
            //    q.GroupCounts = counts;
            //}

            return q;
        }

        #endregion

        #region BTag First GameObject Queries

        /// <summary>
        /// Returns the <b>first</b> GameObject in the scene matching the passed Tag.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectForTag(this Tag tag) => GetTaggedGameObjectsWithTagsResult(GetTagHashWithRules(tag), null, false);

        /// <summary>
        /// Returns the <b>first</b> GameObject with a matching tag that is a child of the passed gameObject or the gameObject itself.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectForTag(this GameObject gameObject, Tag tag) =>
            GetTaggedGameObjectsWithTagsResult(GetTagHashWithRules(tag), gameObject, false);

        /// <summary>
        /// Returns the <b>first</b> GameObject in the scene matching <b>all</b> Tags passed in.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectWithAllTags(this IEnumerable<Tag> tags) => GetGameObjectWithAllTags(tags.ToArray());

        public static GameObject GetGameObjectWithAllTags(this Tag[] tags) => GetTaggedGameObjectsWithTagsResult(GetTagHashesWithRules(tags), null, false);

        /// <summary>
        /// Returns the <b>first</b> GameObject in the scene matching <b>all</b> Tags passed in
        /// that is also a child of the passed gameObject or the gameObject itself.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectWithAllTags(this GameObject gameObject, IEnumerable<Tag> tags) => GetGameObjectWithAllTags(gameObject, tags.ToArray());

        public static GameObject GetGameObjectWithAllTags(this GameObject gameObject, Tag[] tags) =>
            GetTaggedGameObjectsWithTagsResult(GetTagHashesWithRules(tags), gameObject, false);

        /// <summary>
        /// Returns the <b>first</b> GameObject in the scene matching <b>any</b> Tag in the passed tags.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectWithAnyTags(this IEnumerable<Tag> tags) => GetGameObjectWithAnyTags(tags.ToArray());

        public static GameObject GetGameObjectWithAnyTags(this Tag[] tags) => GetTaggedGameObjectsWithTagsResult(GetTagHashesWithRules(tags), null, false);

        /// <summary>
        /// Returns the <b>first</b> GameObject in the scene matching <b>any</b> Tag in the passed tags.
        /// Do note that if multiple GameObjects in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching GameObject found</returns>
        public static GameObject GetGameObjectWithAnyTags(this GameObject gameObject, IEnumerable<Tag> tags) => GetGameObjectWithAnyTags(gameObject, tags.ToArray());

        public static GameObject GetGameObjectWithAnyTags(this GameObject gameObject, Tag[] tags) =>
            GetTaggedGameObjectsWithTagsResult(GetTagHashesWithRules(tags), gameObject, false);

        #endregion

        #region BTag First Component Queries

        /// <summary>
        /// Returns the <b>first</b> Component in the scene matching the passed Tag.
        /// Do note that if multiple Component in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentForTag<C>(this Tag tag) where C : Component => GetTaggedComponentsWith<C>(GetTagHashWithRules(tag), null, false).FirstOrDefault();

        /// <summary>
        /// Returns the <b>first</b> Component with a matching tag that is a child of the passed gameObject or the gameObject itself.
        /// Do note that if multiple Components in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentForTag<C>(this GameObject gameObject, Tag tag) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashWithRules(tag), gameObject, false).FirstOrDefault();

        /// <summary>
        /// Returns the <b>first</b> Component in the scene matching <b>all</b> Tags passed in.
        /// Do note that if multiple Components in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentWithAllTags<C>(this IEnumerable<Tag> tags) where C : Component => GetComponentWithAllTags<C>(tags.ToArray());

        public static C GetComponentWithAllTags<C>(this Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags, InclusionRule.MustInclude), null, false).FirstOrDefault();

        /// <summary>
        /// Returns the <b>first</b> Component in the scene matching <b>all</b> Tags passed in
        /// that is also a child of the passed gameObject or the gameObject itself.
        /// Do note that if multiple Components in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentWithAllTags<C>(this GameObject gameObject, IEnumerable<Tag> tags) where C : Component =>
            GetComponentWithAllTags<C>(gameObject, tags.ToArray());

        public static C GetComponentWithAllTags<C>(this GameObject gameObject, Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags, InclusionRule.MustInclude), gameObject, false).FirstOrDefault();

        /// <summary>
        /// Returns the <b>first</b> Component in the scene matching <b>any</b> Tag in the passed tags.
        /// Do note that if multiple Components in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentWithAnyTags<C>(this IEnumerable<Tag> tags) where C : Component => GetComponentWithAnyTags<C>(tags.ToArray());

        public static C GetComponentWithAnyTags<C>(this Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags), null, false).FirstOrDefault();

        /// <summary>
        /// Returns the <b>first</b> Component in the scene matching <b>any</b> Tag in the passed tags.
        /// Do note that if multiple Components in the scene match this Tag, you should not rely on the result being deterministic.
        /// </summary>
        /// <returns>Null if no matching Component found</returns>
        public static C GetComponentWithAnyTags<C>(this GameObject gameObject, IEnumerable<Tag> tags) where C : Component =>
            GetComponentWithAnyTags<C>(gameObject, tags.ToArray());

        public static C GetComponentWithAnyTags<C>(this GameObject gameObject, Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags), gameObject, false).FirstOrDefault();

        #endregion

        #region BTag List<GameObject> Queries

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching the provided Tag.
        /// </summary>
        public static List<GameObject> GetGameObjectsForTag(this Tag tag) => GetTaggedGameObjectsWithTagsResults(GetTagHashWithRules(tag), null, true);

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching the provided Tag
        /// that are also a child of the passed gameObject or the gameObject itself.
        /// </summary>
        public static List<GameObject> GetGameObjectsForTag(this GameObject gameObject, Tag tag) =>
            GetTaggedGameObjectsWithTagsResults(GetTagHashWithRules(tag), gameObject, true);

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching <b>all</b> Tags passed in.
        /// </summary>
        public static List<GameObject> GetGameObjectsWithAllTags(this IEnumerable<Tag> tags) => GetGameObjectsWithAllTags(tags.ToArray());

        public static List<GameObject> GetGameObjectsWithAllTags(this Tag[] tags) =>
            GetTaggedGameObjectsWithTagsResults(GetTagHashesWithRules(tags, InclusionRule.MustInclude), null, true);

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching <b>all</b> Tags passed in
        /// that are also children of the passed gameObject or the gameObject itself.
        /// </summary>
        public static List<GameObject> GetGameObjectsWithAllTags(this GameObject gameObject, IEnumerable<Tag> tags) =>
            GetGameObjectsWithAllTags(gameObject, tags.ToArray());

        public static List<GameObject> GetGameObjectsWithAllTags(this GameObject gameObject, Tag[] tags) =>
            GetTaggedGameObjectsWithTagsResults(GetTagHashesWithRules(tags, InclusionRule.MustInclude), gameObject, true);

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching <b>any</b> Tag in the passed tags.
        /// </summary>
        public static List<GameObject> GetGameObjectsWithAnyTags(this IEnumerable<Tag> tags) => GetGameObjectsWithAnyTags(tags.ToArray());

        public static List<GameObject> GetGameObjectsWithAnyTags(this Tag[] tags) => GetTaggedGameObjectsWithTagsResults(GetTagHashesWithRules(tags), null, true);

        /// <summary>
        /// Returns <b>all</b> GameObjects in the scene matching <b>any</b> Tag in the passed tags
        /// that are also children of the passed gameObject or the gameObject itself.
        /// </summary>
        public static List<GameObject> GetGameObjectsWithAnyTags(this GameObject gameObject, IEnumerable<Tag> tags) =>
            GetGameObjectsWithAnyTags(gameObject, tags.ToArray());

        public static List<GameObject> GetGameObjectsWithAnyTags(this GameObject gameObject, Tag[] tags) =>
            GetTaggedGameObjectsWithTagsResults(GetTagHashesWithRules(tags), gameObject, true);

        #endregion

        #region BTag List<Component> Queries

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching the provided Tag.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsForTag<C>(this Tag tag) where C : Component => GetTaggedComponentsWith<C>(GetTagHashWithRules(tag), null, false);

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching the provided Tag
        /// that are also a child of the passed gameObject or the gameObject itself.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsForTag<C>(this GameObject gameObject, Tag tag) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashWithRules(tag), gameObject, false);

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching <b>all</b> Tags passed in.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsWithAllTags<C>(this IEnumerable<Tag> tags) where C : Component => GetComponentsWithAllTags<C>(tags.ToArray());

        public static List<C> GetComponentsWithAllTags<C>(this Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags, InclusionRule.MustInclude), null, true);

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching <b>all</b> Tags passed in
        /// that are also children of the passed gameObject or the gameObject itself.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsWithAllTags<C>(this GameObject gameObject, IEnumerable<Tag> tags) where C : Component =>
            GetComponentsWithAllTags<C>(gameObject, tags.ToArray());

        public static List<C> GetComponentsWithAllTags<C>(this GameObject gameObject, Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags, InclusionRule.MustInclude), gameObject, true);

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching <b>any</b> Tag in the passed tags.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsWithAnyTags<C>(this IEnumerable<Tag> tags) where C : Component => GetComponentsWithAnyTags<C>(tags.ToArray());

        public static List<C> GetComponentsWithAnyTags<C>(this Tag[] tags) where C : Component => GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags), null, true);

        /// <summary>
        /// Returns <b>all</b> Components in the scene matching <b>any</b> Tag in the passed tags
        /// that are also children of the passed gameObject or the gameObject itself.
        /// </summary>
        /// <returns>Note: Every matching Component for a given GameObject, of which there can be multiple per GameObject, will be returned.</returns>
        public static List<C> GetComponentsWithAnyTags<C>(this GameObject gameObject, IEnumerable<Tag> tags) where C : Component =>
            GetComponentsWithAnyTags<C>(gameObject, tags.ToArray());

        public static List<C> GetComponentsWithAnyTags<C>(this GameObject gameObject, Tag[] tags) where C : Component =>
            GetTaggedComponentsWith<C>(GetTagHashesWithRules(tags), gameObject, true);

        #endregion


        internal static bool TransformMatchesTags(Transform transform, ref TagHashWithRules[] tags, int idx, int count)
        {
            bool found = false;
            bool onlyDidExclusion = true;

            List<BHash128> tagsInComponents = transform.gameObject.GetComponents<MultiTagGameObject>().SelectMany(x => x.tags).Select(y => y.Hash).ToList();
            tagsInComponents.AddRange(transform.gameObject.GetComponents<TagGameObject>().Where(x => x.tag != null).Select(x => x.tag.Hash));

            for (int i = idx; i < (idx + count); ++i)
            {
                var tagEntry = tags[i];
                switch (tagEntry.rule)
                {
                    case InclusionRule.MustExclude:
                        if (tagsInComponents.Contains(tagEntry.hash)) return false;
                        break;
                    case InclusionRule.MustInclude:
                        if (!tagsInComponents.Contains(tagEntry.hash)) return false;
                        found = true;
                        onlyDidExclusion = false;
                        break;
                    case InclusionRule.Any:
                        if (!found && tagsInComponents.Contains(tagEntry.hash)) found = true;
                        onlyDidExclusion = false;
                        break;
                }
            }

            return (onlyDidExclusion || found);
        }

        static HashSet<int> SubSearchResults = new HashSet<int>();

        internal static GameObject GetTaggedGameObjectsWithTagsResult(TagHashWithRules[] tags, GameObject gameObject = null, bool findAll = true)
        {
            var result = GetTaggedGameObjectsWithTags(tags,
                0,
                tags.Length,
                gameObject,
                findAll);
            return result.Count > 0 ? AllTaggedGOs[result.FirstOrDefault()] : null;
        }

        internal static List<GameObject> GetTaggedGameObjectsWithTagsResults(TagHashWithRules[] tags, GameObject gameObject = null, bool findAll = true)
        {
            var allGOs = AllTaggedGOs;
            var result = GetTaggedGameObjectsWithTags(tags,
                0,
                tags.Length,
                gameObject,
                findAll);
            int resultCount = result.Count;
            var results = new List<GameObject>(resultCount);
            foreach (var goIdx in result) results.Add(allGOs[goIdx]);
            return results;
        }


        /// <summary>
        /// Returns all GameObjects matching the provided Tags.
        /// If a GameObject is provided, results will only be within and including that GameObject's search.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="idx"></param>
        /// <param name="count"></param>
        /// <param name="gameObject"></param>
        /// <param name="findAll"></param>
        /// <returns></returns>
        internal static HashSet<int> GetTaggedGameObjectsWithTags(TagHashWithRules[] tags, int idx, int count, GameObject gameObject = null, bool findAll = true) =>
            GetTaggedGameObjectsWithTags<GameObject>(tags,
                idx,
                count,
                gameObject,
                findAll);

        internal static HashSet<int> GetTaggedGameObjectsWithTags<T>(TagHashWithRules[] tags, int idx, int count, GameObject gameObject = null, bool findAll = true)
            where T : UnityEngine.Object
        {
            bool checkForComponent = (typeof(T) != typeof(GameObject));
            SubSearchResults.Clear();
            //If no tags, early out
            if ((idx + count) > tags.Length || idx < 0)
            {
                Debug.LogWarning(idx + " + " + count + " out of range of " + tags.Length);
                return SubSearchResults;
            }

            var firstTag = tags[idx];

            if (gameObject != null)
            {
                var allGOs = AllTaggedGOs;
                var allTransforms = firstTag.searchOption == Search.Target
                    ? gameObject.GetComponents<Transform>()
                    : (firstTag.searchOption == Search.Parent ? gameObject.GetComponentsInParent<Transform>() : gameObject.GetComponentsInChildren<Transform>(true));
                if (checkForComponent) allTransforms = allTransforms.Where(x => x.GetComponent<T>() != null).ToArray();

                for (int t = 0; t < allTransforms.Length; ++t)
                {
                    // If only children, skip past GameObject's own transform
                    if ((firstTag.searchOption == Search.Children || firstTag.searchOption == Search.Parent) && allTransforms[t] == gameObject.transform) continue;

                    bool found = TransformMatchesTags(allTransforms[t], ref tags, idx, count);
                    if (found)
                    {
                        foreach (var goIdx in SubSearchResults)
                        {
                            if (allGOs[goIdx] == allTransforms[t].gameObject) found = false;
                        }
                    }

                    if (found)
                    {
                        SubSearchResults.Add(allTransforms[t].gameObject.GetInstanceID());

                        if (!findAll)
#if UNITY_EDITOR
                            if (IgnoreEditorChecks || BTagSetting.Instance.disableEditorChecks || SubSearchResults.Count > 2)
#endif
                                break;
                    }
                }
            }
            else
            {
                GetTaggedWith<T>(tags, idx, count, findAll);

#if UNITY_EDITOR
                if (!BTagSetting.Instance.disableEditorChecks && !IgnoreEditorChecks && !findAll && (count + idx == tags.Length))
                {
                    var allGOs = AllTaggedGOs;
                    var results = new List<GameObject>(WorkingHashSet.Count);
                    foreach (var goIdx in WorkingHashSet) results.Add(allGOs[goIdx]);
                    CheckForMultipleResults(results, tags);
                }
#endif
                return WorkingHashSet;
            }

#if UNITY_EDITOR
            if (!BTagSetting.Instance.disableEditorChecks && !IgnoreEditorChecks && !findAll && (count + idx == tags.Length))
            {
                var allGOs = AllTaggedGOs;
                var results = new List<GameObject>(SubSearchResults.Count);
                foreach (var goIdx in SubSearchResults) results.Add(allGOs[goIdx]);
                CheckForMultipleResults(results, tags);
            }
#endif
            return SubSearchResults;
        }

        internal static List<C> GetTaggedComponentsWith<C>(TagHashWithRules[] tags, GameObject gameObject = null, bool findAll = true) where C : Component
        {
            var results = new List<C>();
            if (tags == null) return results;

            //If no tags, early out
            if (tags.Length < 1) return results;
            var firstTag = tags[0];

            var allGOs = AllTaggedGOs;
            if (gameObject != null)
            {
                var allTransforms = firstTag.searchOption == Search.Target ? gameObject.GetComponents<Transform>() : gameObject.GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < allTransforms.Length; ++t)
                {
                    // If only children, skip passed GameObject's own transform
                    if (firstTag.searchOption == Search.Children && allTransforms[t] == gameObject.transform) continue;

                    bool found = TransformMatchesTags(allTransforms[t], ref tags, 0, tags.Length);
                    if (found)
                    {
                        results.AddRange(allTransforms[t].GetComponents<C>());
                        if (!findAll)
#if UNITY_EDITOR
                            if (IgnoreEditorChecks || BTagSetting.Instance.disableEditorChecks || results.Count > 2)
#endif
                                break;
                    }
                }
            }
            else
            {
                var cResults = GetTaggedWith<C>(tags, 0, tags.Length, findAll);
                if (cResults > 0)
                {
                    foreach (var goIdx in WorkingHashSet) results.AddRange(allGOs[goIdx].GetComponents<C>());
                }
            }
#if UNITY_EDITOR
            if (!BTagSetting.Instance.disableEditorChecks && !IgnoreEditorChecks && !findAll) CheckForMultipleResults(results, tags);
#endif
            return results;
        }


        static HashSet<int> WorkingHashSet = new HashSet<int>();
        internal static int GetTaggedWith(TagHashWithRules[] tags, int idx, int tagCount, bool findAll = true) => GetTaggedWith<GameObject>(tags, idx, tagCount, findAll);

        internal static int GetTaggedWith<T>(TagHashWithRules[] tags, int idx, int tagCount, bool findAll = true) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            // If in the Editor and not playing, find all tagged GameObjects in open scenes and use those for search methods
            if (!Application.isPlaying) FindInactiveTags();
#endif
            WorkingHashSet.Clear();

            if (tags == null || tagCount < 1 || (idx + tagCount) > tags.Length)
            {
                Debug.LogWarning("List of provided tags is empty");
                return 0;
            }

            bool checkForComponent = (typeof(T) != typeof(GameObject));
            TagHashWithRules firstInclusiveTag = tags[idx];
            if (tagCount > 1)
            {
                for (int i = idx; i < (idx + tagCount); ++i)
                {
                    if (tags[i].hash.IsValid)
                    {
                        if (tags[i].rule != InclusionRule.MustExclude)
                        {
                            firstInclusiveTag = tags[i];
                            break;
                        }
                    }
                    else if (tags[i].rule == InclusionRule.MustInclude)
                    {
                        return 0;
                    }
                }
            }
            else
            {
                if (!firstInclusiveTag.hash.IsValid) return 0;
                if (firstInclusiveTag.rule == InclusionRule.MustExclude || !firstInclusiveTag.hash.IsValid) firstInclusiveTag = default;
            }

            // If there are only tags for exclusion, add everything that doesn't include the passed tags
            if (!firstInclusiveTag.hash.IsValid)
            {
                var enumerator = AllTaggedGOsByIndex.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var kvp = enumerator.Current;
                    bool isExcluded = false;
                    for (int i = idx; i < (idx + tagCount); ++i)
                    {
                        isExcluded |= !tags[i].hash.IsValid || tags[i].hash == kvp.Key;
                        if (!isExcluded) break;
                    }

                    if (!isExcluded)
                    {
                        WorkingHashSet.UnionWith(kvp.Value);
                    }
                }
            }
            else
            {
                BHash128 firstTagHash = firstInclusiveTag.hash;
                if (AllTaggedGOsByIndex.TryGetValue(firstTagHash, out var firstTags))
                {
                    if (firstTags.Count < 1)
                    {
                        WorkingHashSet.Clear();
                        return 0;
                    }

                    bool isLastSearch = tagCount + idx == tags.Length;
                    bool canUseFirstResult = isLastSearch && !findAll && tagCount == 1;

                    //Debug.Log("Found first tag, checking " + TempWorkingSetSize + " of " + firstTags.Length);
                    if (canUseFirstResult)
                    {
                        if (checkForComponent)
                        {
                            // When we're finding a component, the first gameobject might not necessarily
                            // be the one we're looking for. Unless we're checking all the gameobjects,
                            // find the first that has the component
                            bool found = false;
                            foreach (var goIdx in firstTags)
                            {
                                //Debug.Log("Checking if " + allGOs[goIdx] + " (" + goIdx + ") has component " + typeof(T) + ": " + allGOs[goIdx].GetComponent<T>());
                                if (AllTaggedGOs[goIdx].GetComponent<T>() != null)
                                {
                                    found = true;
                                    WorkingHashSet.Add(goIdx);
                                    break;
                                }
                            }

                            // If there were no gameobjects with the first tag that have the component
                            // we're looking for then early out
                            if (!found)
                            {
                                WorkingHashSet.Clear();
                                return 0;
                            }
                        }
                        else
                        {
                            var e = firstTags.GetEnumerator();
                            if (e.MoveNext())
                            {
                                WorkingHashSet.Add(e.Current);
                                return 1;
                            }

                            WorkingHashSet.Clear();
                            return 0;
                        }
                    }
                    else
                    {
                        WorkingHashSet.UnionWith(firstTags);
                    }

                    // There was one tag and it was either include Any or MustInclude
                    // Therefore we can directly return any entries from our lookup
                    if (tagCount == 1) return WorkingHashSet.Count;

                    for (int t = idx + 1; t < (idx + tagCount); ++t)
                    {
                        var tagEntry = tags[t];
                        if (tagEntry.rule == InclusionRule.Any && tagEntry.hash != firstTagHash)
                        {
                            if (AllTaggedGOsByIndex.TryGetValue(tagEntry.hash, out var entries))
                            {
                                WorkingHashSet.UnionWith(entries);
                            }
                        }
                    }


                    // For every Tag that's not the first Tag, check to see
                    // the list of GameObjects for the tag contains the GameObject
                    if (tagCount > 1)
                    {
                        for (int j = idx; j < (idx + tagCount); ++j)
                        {
                            var tagEntry = tags[j];
                            if (tagEntry.hash != firstTagHash)
                            {
                                switch (tagEntry.rule)
                                {
                                    case InclusionRule.Any: continue;
                                    case InclusionRule.MustInclude:
                                        if (AllTaggedGOsByIndex.TryGetValue(tagEntry.hash, out var entries))
                                        {
                                            WorkingHashSet.IntersectWith(entries);
                                        }
                                        else if (tagEntry.hash.IsValid)
                                        {
                                            // The required tag wasn't found
                                            //Debug.LogWarning("Unable to find required tag: " + tagEntry.hash);
                                            WorkingHashSet.Clear();
                                            return 0;
                                        }

                                        break;
                                    case InclusionRule.MustExclude:
                                        bool exists = AllTaggedGOsByIndex.TryGetValue(tagEntry.hash, out var exclusionEntries);
                                        if (!exists)
                                        {
                                            WorkingHashSet.Clear();
                                        }
                                        else
                                        {
                                            WorkingHashSet.ExceptWith(exclusionEntries);
                                        }

                                        break;
                                }
                            }
                        }
                    }

                    if (checkForComponent)
                    {
                        foreach (var goIdx in WorkingHashSet)
                        {
                            if (AllTaggedGOs[goIdx].GetComponent<T>() == null) WorkingHashSet.Remove(goIdx);
                        }
                    }
                }
            }

            return WorkingHashSet.Count;
        }

        // Checks the Tag is not null and returns the default Hash if it is
        public static BHash128 GetHash(Tag tag)
        {
            bool valid = tag != null;
#if UNITY_EDITOR
            if (!valid) Debug.LogWarning("-Untagged- was passed into a query");
#endif
            return valid ? tag.Hash : default;
        }
    }
}