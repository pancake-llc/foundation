namespace Pancake
{
    public struct AudioKey
    {
        public static AudioKey invalid = new AudioKey(-1, null);

        internal int value;
        internal Audio audio;

        public AudioKey(int value, Audio audio)
        {
            this.value = value;
            this.audio = audio;
        }

        public override bool Equals(object obj) { return obj is AudioKey a && value == a.value && audio == a.audio; }
        public override int GetHashCode() { return value.GetHashCode() ^ audio.GetHashCode(); }
        public static bool operator ==(AudioKey a, AudioKey b) { return a.value == b.value && a.audio == b.audio; }
        public static bool operator !=(AudioKey a, AudioKey b) { return !(a == b); }
    }
}