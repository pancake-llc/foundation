namespace Pancake.Sound
{
    public static class AudioStatic
    {
        public static AudioId Play(this AudioId id)
        {
            if (string.IsNullOrEmpty(id.id)) return id;
            AudioManager.playEvent?.Invoke(id);
            return id;
        }

        public static void Stop(this AudioId id) { AudioManager.stopEvent?.Invoke(id); }
        public static void Pause(this AudioId id) { AudioManager.pauseEvent?.Invoke(id); }
        public static void Resume(this AudioId id) { AudioManager.resumeEvent?.Invoke(id); }

        public static void QuietOthers(this AudioId id, float volume, float fadeDuration, float effectiveTime)
        {
            AudioManager.quietOthersEvent?.Invoke(id, volume, fadeDuration, effectiveTime);
        }

        public static void StopAll() { AudioManager.stopAllEvent?.Invoke(); }
        public static void StopAllByType(this EAudioType audioType) { AudioManager.stopAllByTypeEvent?.Invoke(audioType); }
        public static void StopAllAndClean() { AudioManager.stopAllAndCleanEvent?.Invoke(); }
    }
}