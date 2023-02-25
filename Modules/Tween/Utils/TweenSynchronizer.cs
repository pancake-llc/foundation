using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.Tween
{
    internal static class TweenSynchronizer
    {
        private static Dictionary<string, HashSet<ITween>> tweensDict;

        static TweenSynchronizer() { tweensDict = new Dictionary<string, HashSet<ITween>>(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tween"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ITween Unregister(ITween tween, string key = null)
        {
            if (tween == null) return null;

            if (tween.IsPlaying)
            {
                if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);

                if (tweensDict.TryGetValue(key, out var tweens))
                {
                    tweens.Remove(tween);

                    if (tweens.Count == 0) tweensDict.Remove(key);
                }
            }
            else
            {
                // If tween is killed, default key cannot be created, so delete by scanning all records
                foreach (var set in tweensDict.Values)
                {
                    set.Remove(tween);
                }

                var emptyTweens = tweensDict.Where(kvp => kvp.Value.Count == 0);
                foreach (var kvp in emptyTweens.ToArray())
                {
                    tweensDict.Remove(kvp.Key);
                }
            }

            return tween;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tween"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ITween UnregisterAll(ITween tween, string key = null)
        {
            if (tween == null && string.IsNullOrEmpty(key)) return null;

            if (tween != null)
            {
                if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);

                if (tweensDict.TryGetValue(key, out var tweens))
                {
                    tweens.Clear();
                }

                tweensDict.Remove(key);
            }

            return tween;
        }

        /// <summary>
        /// Get the default sync key
        /// Synchronize items with the same tween time
        /// </summary>
        /// <param name="tween"></param>
        /// <returns>Sync key</returns>
        private static string GetDefaultSyncKey(ITween tween)
        {
            if (tween == null) return null;

            float duration = tween.GetDuration();
            if (Mathf.Approximately(duration, 0f))
            {
                Debug.LogWarning("A Tween with a Duration of 0 was specified");
            }

            return $"duration_{duration}";
        }

        /// <summary>
        /// Sync with first tween
        /// </summary>
        /// <param name="tween">target tween</param>
        /// <param name="key">sync key</param>
        /// <returns></returns>
        public static ITween SyncWithPrimary(ITween tween, string key = null)
        {
            if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);
            if (tweensDict.Count == 0)
            {
                var newTweens = new HashSet<ITween> {tween};
                tweensDict.Add(key, newTweens);
                return tween;
            }

            if (!tweensDict.TryGetValue(key, out var tweens))
            {
                tweens = new HashSet<ITween> {tween};
                tweensDict.Add(key, tweens);
                return tween;
            }

            if (!tweens.Contains(tween)) tweens.Add(tween);

            var first = tweens.First(_ => _ != tween && _.IsPlaying);

            tween.OnStartLate(OnStartLate);

            void OnStartLate()
            {
                tween.OnStartLate(null);
                tween.Goto(first.GetElapsed());
            }

            return tween;
        }

        public static void SyncAgain(ITween tween, string key = null)
        {
            if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);
            if (tweensDict.Count == 0)
            {
                var newTweens = new HashSet<ITween> {tween};
                tweensDict.Add(key, newTweens);
                return;
            }

            if (!tweensDict.TryGetValue(key, out var tweens))
            {
                tweens = new HashSet<ITween> {tween};
                tweensDict.Add(key, tweens);
                return;
            }

            if (!tweens.Contains(tween)) tweens.Add(tween);

            var first = tweens.First(_ => _ != tween && _.IsPlaying);

            tween.Goto(first.GetElapsed());
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Clear()
        {
            if (tweensDict.Count == 0) return;

            foreach (var value in tweensDict.Values)
            {
                value.Clear();
            }

            tweensDict.Clear();
        }
    }
}