using System;

namespace Pancake.Sound
{
    public class SeamlessLoopHelper
    {
        private readonly Func<AudioPlayer> _getPlayerFunc;
        private readonly AudioPlayerInstanceWrapper _playerWrapper;

        public SeamlessLoopHelper(AudioPlayerInstanceWrapper playerWrapper, Func<AudioPlayer> getPlayerFunc)
        {
            _getPlayerFunc = getPlayerFunc;
            _playerWrapper = playerWrapper;
            _playerWrapper.OnRecycle += Recycle;
        }

        private void Recycle(AudioPlayer player)
        {
            _playerWrapper.OnRecycle -= Recycle;
            player.OnSeamlessLoopReplay -= Replay;
        }

        public void AddReplayListener(AudioPlayer player) { player.OnSeamlessLoopReplay += Replay; }

        private void Replay(int id, PlaybackPreference pref, EEffectType previousEffect, float trackVolume, float pitch)
        {
            var newPlayer = _getPlayerFunc?.Invoke();
            if (newPlayer == null) return;

            _playerWrapper.UpdateInstance(newPlayer);
            newPlayer.SetEffect(previousEffect, ESetEffectMode.Override);
            newPlayer.SetVolume(trackVolume);
            newPlayer.SetPitch(pitch);
            newPlayer.Play(id, pref);
            newPlayer.OnSeamlessLoopReplay += Replay;
        }
    }
}