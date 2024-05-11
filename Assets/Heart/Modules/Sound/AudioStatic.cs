namespace Pancake.Sound
{
    public static class AudioStatic
    {
        public static AudioHandle PlaySfx(this Audio audio) { return AudioManager.PlaySfx(audio); }
        public static void StopSfx(this AudioHandle handle) { AudioManager.StopSfx(handle); }
        public static void PauseSfx(this AudioHandle handle) { AudioManager.PauseSfx(handle); }
        public static void ResumeSfx(this AudioHandle handle) { AudioManager.ResumeSfx(handle); }
        public static void FinishSfx(this AudioHandle handle) { AudioManager.FinishSfx(handle); }
        public static AudioHandle PlayMusic(this Audio audio) { return AudioManager.PlayMusic(audio); }
        public static void StopMusic(this AudioHandle handle) { AudioManager.StopMusic(handle); }
        public static void PauseMusic(this AudioHandle handle) { AudioManager.PauseMusic(handle); }
        public static void ResumeMusic(this AudioHandle handle) { AudioManager.ResumeMusic(handle); }
    }
}