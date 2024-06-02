using System;

namespace Pancake.Sound
{
    [Serializable]
    public struct AudioHandle
    {
        internal string id;
        internal Audio audio;

        public AudioHandle(Audio audio)
        {
            id = Guid.NewGuid().ToString();
            this.audio = audio;
        }
    }
}