namespace Pancake.Sound
{
    public class AudioHandle
    {
        public string id;
        public EAudioType type;
        public float volume; // group volume * clip volume
        public AudioPlayer player;

        public AudioHandle(string id, EAudioType type, float volume, AudioPlayer player)
        {
            this.id = id;
            this.type = type;
            this.volume = volume;
            this.player = player;
        }
    }
}