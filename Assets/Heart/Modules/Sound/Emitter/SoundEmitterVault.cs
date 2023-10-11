using System.Collections.Generic;

namespace Pancake.Sound
{
    public class SoundEmitterVault
    {
        private int _nextUniqueKey;
        private List<AudioHandle> _keys;
        private List<SoundEmitter[]> _emitters;

        public SoundEmitterVault()
        {
            _keys = new List<AudioHandle>();
            _emitters = new List<SoundEmitter[]>();
        }

        public AudioHandle Get(Audio audio) { return new AudioHandle(_nextUniqueKey++, audio); }

        public bool Get(AudioHandle key, out SoundEmitter[] emitter)
        {
            int index = _keys.FindIndex(x => x == key);

            if (index < 0)
            {
                emitter = null;
                return false;
            }

            emitter = _emitters[index];
            return true;
        }

        internal List<SoundEmitter[]> GetAll() => _emitters;

        public void Add(AudioHandle key, SoundEmitter[] emitter)
        {
            _keys.Add(key);
            _emitters.Add(emitter);
        }

        public AudioHandle Add(Audio audio, SoundEmitter[] emitter)
        {
            var key = Get(audio);
            _keys.Add(key);
            _emitters.Add(emitter);
            return key;
        }

        public bool Remove(AudioHandle key)
        {
            int index = _keys.FindIndex(x => x == key);
            return RemoveAt(index);
        }

        private bool RemoveAt(int index)
        {
            if (index < 0) return false;

            _keys.RemoveAt(index);
            _emitters.RemoveAt(index);

            return true;
        }
    }
}