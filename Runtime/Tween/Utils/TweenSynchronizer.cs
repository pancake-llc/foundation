using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.Tween
{
    public static class TweenSynchronizer
    {
        private static Dictionary<string, HashSet<Tween>> _tweensDict;

        static TweenSynchronizer() { _tweensDict = new Dictionary<string, HashSet<Tween>>(); }

        /// <summary>
        /// register
        /// If no synchronization key is specified, tweens with the same duration will be synchronized
        /// It is recommended to specify the target with SetTarget() because the active tween target GameObject will be synchronized.
        /// </summary>
        /// <param name="tween">target tween</param>
        /// <param name="key">sync key</param>
        /// <param name="autoUnregister">Automatically unregister from sync when killed</param>
        /// <returns>Tween</returns>
        /// <returns></returns>
        public static Tween Register(Tween tween, string key = null, bool autoUnregister = true)
        {
            if (tween == null || !tween.IsAlive) return tween;

            if (string.IsNullOrEmpty(key))
            {
                key = GetDefaultSyncKey(tween);
            }

            HashSet<Tween> tweens;
            if (!_tweensDict.TryGetValue(key, out tweens))
            {
                tweens = new HashSet<Tween>();
                _tweensDict.Add(key, tweens);
            }

            tweens.Add(tween);
            
            // Release from synchronization target with Kill

            if (autoUnregister)
            {
                tween.OnKill(() => Unregister(tween));
            }

            return tween;
        }

        public static void Unregister(Tween tween, string key = null)
        {
            if (tween == null) return;

            if (tween.IsAlive)
            {
                if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);

                if (_tweensDict.TryGetValue(key, out var tweens))
                {
                    tweens.Remove(tween);

                    if (tweens.Count == 0) _tweensDict.Remove(key);
                }
            }
            else
            {
                // If tween is killed, default key cannot be created, so delete by scanning all records
                foreach (var set in _tweensDict.Values)
                {
                    set.Remove(tween);
                }
                
                var emptyTweens = _tweensDict.Where(kvp => kvp.Value.Count == 0);
                foreach (var kvp in emptyTweens)
                {
                    _tweensDict.Remove(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// Get the default sync key
        /// Synchronize items with the same tween time
        /// </summary>
        /// <param name="tween"></param>
        /// <returns>Sync key</returns>
        private static string GetDefaultSyncKey(Tween tween)
        {
            if (tween == null || !tween.IsAlive) return null;

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
        public static Tween SyncWithPrimary(Tween tween, string key = null)
        {
            if (_tweensDict.Count == 0) return tween;

            if (string.IsNullOrEmpty(key)) key = GetDefaultSyncKey(tween);

            if (!_tweensDict.ContainsKey(key)) return tween;

            var tweens = _tweensDict.GetValueOrDefault(key);

            Tween first = tweens.FirstOrDefault(_ => _ != tween && _.IsAlive && _.IsPlaying);
            if (first == null) return tween;

            return null;
        }
    }
}