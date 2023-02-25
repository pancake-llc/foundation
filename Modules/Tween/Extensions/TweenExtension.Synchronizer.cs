namespace Pancake.Tween
{
    public static partial class TweenExtension
    {
        public static ITween SyncWithPrimary(this ITween tween, string key = null) => TweenSynchronizer.SyncWithPrimary(tween, key);

        public static ITween UnsyncAll(this ITween tween, string key = null) => TweenSynchronizer.UnregisterAll(tween, key);

        public static ITween Unsync(this ITween tween, string key = null) => TweenSynchronizer.Unregister(tween, key);

        public static void SyncAgain(this ITween tween, string key = null) => TweenSynchronizer.SyncAgain(tween, key);

        public static void ClearAllSync() { TweenSynchronizer.Clear(); }
    }
}