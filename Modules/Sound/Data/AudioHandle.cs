namespace Pancake.Sound
{
    [System.Serializable]
    public struct AudioHandle
    {
        public static AudioHandle invalid = new AudioHandle(-1, null);

        internal int value;
        internal Audio audio;

        public AudioHandle(int value, Audio audio)
        {
            this.value = value;
            this.audio = audio;
        }

        public override bool Equals(object obj) { return obj is AudioHandle a && value == a.value && audio == a.audio; }
        public override int GetHashCode() { return value.GetHashCode() ^ audio.GetHashCode(); }
        public static bool operator ==(AudioHandle a, AudioHandle b) { return a.value == b.value && a.audio == b.audio; }
        public static bool operator !=(AudioHandle a, AudioHandle b) { return !(a == b); }


        public void Pause() { }

        public void Resume() { }

        public void Stop() { }

        public void Finish() { }
    }
}