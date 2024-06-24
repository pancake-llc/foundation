using UnityEngine;
using System.Collections.Generic;

namespace Pancake.Sound
{
    internal class AudioPlayerObjectPool : ObjectPool<AudioPlayer>
    {
        private readonly Transform _parent;
        private List<AudioPlayer> _currentPlayers;
        private readonly IAudioMixer _mixer;

        public AudioPlayerObjectPool(AudioPlayer baseObject, Transform parent, int maxInternalPoolSize, IAudioMixer mixer)
            : base(baseObject, maxInternalPoolSize)
        {
            _parent = parent;
            _mixer = mixer;
        }

        public override AudioPlayer Extract()
        {
            var player = base.Extract();
#if !UNITY_WEBGL
            player.SetData(_mixer.Mixer, _mixer.GetTrack);
#endif

            _currentPlayers ??= new List<AudioPlayer>();
            _currentPlayers.Add(player);
            return player;
        }

        public override void Recycle(AudioPlayer player)
        {
            _mixer.ReturnTrack(player.TrackType, player.AudioTrack);
            RemoveFromCurrent(player);
            base.Recycle(player);
        }

        protected override AudioPlayer CreateObject()
        {
            var newPlayer = Object.Instantiate(baseObject, _parent);
            newPlayer.OnRecycle += Recycle;
            return newPlayer;
        }

        protected override void DestroyObject(AudioPlayer instance) { Object.Destroy(instance.gameObject); }

        private void RemoveFromCurrent(AudioPlayer player)
        {
            for (int i = _currentPlayers.Count - 1; i >= 0; i--)
            {
                if (_currentPlayers[i] == player) _currentPlayers.RemoveAt(i);
            }
        }

        public IReadOnlyList<AudioPlayer> GetCurrentAudioPlayers() { return _currentPlayers; }
    }
}