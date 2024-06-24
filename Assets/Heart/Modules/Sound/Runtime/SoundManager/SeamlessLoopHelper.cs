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
            player.OnFinishingOneRound -= OnFinishingOneRound;
        }

        public void SetPlayer(AudioPlayer player) { player.OnFinishingOneRound += OnFinishingOneRound; }

        private void OnFinishingOneRound(int id, PlaybackPreference pref, EEffectType previousEffect)
        {
            var newPlayer = _getPlayerFunc?.Invoke();
            if (newPlayer == null) return;

            _playerWrapper.UpdateInstance(newPlayer);

            var audioType = AudioExtension.GetAudioType(id);
            if (SoundManager.Instance.AudioTypePreference.TryGetValue(audioType, out var audioTypePreference)) pref.AudioTypePlaybackPreference = audioTypePreference;

            newPlayer.SetEffect(previousEffect, ESetEffectMode.Override);
            newPlayer.Play(id, pref, false);
            newPlayer.OnFinishingOneRound += OnFinishingOneRound;
        }
    }
}