using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Pancake.Common;


namespace Pancake.Sound
{
    [DisallowMultipleComponent, AddComponentMenu("")]
    public class SoundManager : MonoBehaviour, IAudioMixer
    {
        private static SoundManager instance;

        public static SoundManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return null;
#endif
                return instance;
            }
        }

        [SerializeField] private AudioPlayer audioPlayerPrefab;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private List<ScriptableObject> soundAssets = new();
        private readonly Dictionary<int, IAudioEntity> _audioBank = new();
        private readonly Dictionary<EAudioType, AudioTypePlaybackPreference> _audioTypePreference = new();
        private EffectAutomationHelper _automationHelper;
        private readonly Dictionary<int, bool> _combFilteringPreventer = new();
        private AsyncProcessHandle _masterVolumeHandle;
        private AudioPlayerObjectPool _audioPlayerPool;
        private ObjectPool<AudioMixerGroup> _audioTrackPool;
        private ObjectPool<AudioMixerGroup> _dominatorTrackPool;

        public IReadOnlyDictionary<EAudioType, AudioTypePlaybackPreference> AudioTypePreference => _audioTypePreference;
        public AudioMixer Mixer => audioMixer;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Debug.LogError("SoundManager already exist at runtime");

            string noticeTemplate = AudioConstant.LOG_HEADER + $"Please assign {{0}} in {nameof(SoundManager)}.prefab";
            if (!audioMixer)
            {
                Debug.LogError(string.Format(noticeTemplate, AudioConstant.MIXER_NAME));
                return;
            }

            if (!audioPlayerPrefab)
            {
                Debug.LogError(string.Format(noticeTemplate, AudioConstant.AUDIO_PLAYER_PREFAB_NAME));
                return;
            }

            _audioPlayerPool = new AudioPlayerObjectPool(audioPlayerPrefab, transform, AudioSettings.DefaultAudioPlayerPoolSize, this);
            var mixerGroups = audioMixer.FindMatchingGroups(AudioConstant.GENERIC_TRACK_NAME);
            var dominatorGroups = audioMixer.FindMatchingGroups(AudioConstant.DOMINATOR_TRACK_NAME);

            _audioTrackPool = new AudioTrackObjectPool(mixerGroups);
            _dominatorTrackPool = new AudioTrackObjectPool(dominatorGroups, true);

            InitBank();
            _automationHelper = new EffectAutomationHelper(audioMixer);
        }

        #region InitBank

        private void InitBank()
        {
            foreach (var scriptableObj in soundAssets)
            {
                IAudioAsset asset = scriptableObj as IAudioAsset;
                if (asset == null)
                    continue;

                foreach (var entity in asset.GetAllAudioEntities())
                {
                    if (!entity.Validate())
                        continue;

                    if (!_audioBank.ContainsKey(entity.Id))
                    {
                        _audioBank.Add(entity.Id, entity as IAudioEntity);
                    }
                }
            }

            AudioExtension.ForeachConcreteAudioType(audioType => _audioTypePreference.Add(audioType, new AudioTypePlaybackPreference()));
        }

        #endregion

        #region Volume

        public void SetVolume(float vol, EAudioType targetType, float fadeTime)
        {
#if !UNITY_WEBGL
            if (targetType == EAudioType.All)
            {
                SetMasterVolume(vol, fadeTime);
                return;
            }
#endif
            if (targetType == EAudioType.None)
            {
                Debug.LogWarning(AudioConstant.LOG_HEADER + $"SetVolume with {targetType} is meaningless");
                return;
            }

            GetPlaybackPrefByType(targetType, pref => pref.Volume = vol);
            GetCurrentActivePlayers(player =>
            {
                if (targetType.HasFlagUnsafe(AudioExtension.GetAudioType(player.Id))) player.SetVolume(vol, fadeTime);
            });
        }

        private void SetMasterVolume(float targetVolume, float fadeTime)
        {
            targetVolume = targetVolume.ToDecibel();
            if (audioMixer.SafeGetFloat(AudioConstant.MASTER_TRACK_NAME, out float currentVolume))
            {
                if (Mathf.Approximately(currentVolume, targetVolume)) return;

                if (fadeTime != 0f)
                {
                    var ease = currentVolume < targetVolume ? AudioSettings.DefaultFadeInEase : AudioSettings.DefaultFadeOutEase;
                    var volumes = AudioExtension.GetLerpValuesPerFrame(currentVolume, targetVolume, fadeTime, ease);
                    App.StopAndReassign(ref _masterVolumeHandle, IeSetMasterVolume(volumes));
                }
                else
                {
                    audioMixer.SafeSetFloat(AudioConstant.MASTER_TRACK_NAME, targetVolume);
                }
            }

            return;

            IEnumerator IeSetMasterVolume(IEnumerable<float> volumes)
            {
                foreach (float vol in volumes)
                {
                    audioMixer.SafeSetFloat(AudioConstant.MASTER_TRACK_NAME, vol);
                    yield return null;
                }
            }
        }

        public void SetVolume(int id, float vol, float fadeTime)
        {
            GetCurrentActivePlayers(player =>
            {
                if (player && player.Id == id) player.SetVolume(vol, fadeTime);
            });
        }

        #endregion

        #region Effect

        public IAutoResetWaitable SetEffect(Effect effect) { return SetEffect(EAudioType.All, effect); }

        public IAutoResetWaitable SetEffect(EAudioType targetType, Effect effect)
        {
            ESetEffectMode mode = ESetEffectMode.Add;
            if (effect.type == EEffectType.None) mode = ESetEffectMode.Override;
            else if (effect.IsDefault()) mode = ESetEffectMode.Remove;

            Action<EEffectType> onResetEffect = null;
            if (!effect.isDominator)
            {
                if (mode == ESetEffectMode.Remove)
                {
                    // wait for reset tweaking of the previous effect
                    onResetEffect = resetType => SetPlayerEffect(targetType, resetType, ESetEffectMode.Remove);
                }
                else
                {
                    SetPlayerEffect(targetType, effect.type, mode);
                }
            }

            _automationHelper.SetEffectTrackParameter(effect, onResetEffect);
            return _automationHelper;
        }

        private void SetPlayerEffect(EAudioType targetType, EEffectType effectType, ESetEffectMode mode)
        {
            GetPlaybackPrefByType(targetType,
                pref =>
                {
                    switch (mode)
                    {
                        case ESetEffectMode.Add:
                            pref.EffectType |= effectType;
                            break;
                        case ESetEffectMode.Remove:
                            pref.EffectType &= ~effectType;
                            break;
                        case ESetEffectMode.Override:
                            pref.EffectType = effectType;
                            break;
                    }
                });

            GetCurrentActivePlayers(player =>
            {
                if (targetType.HasFlagUnsafe(AudioExtension.GetAudioType(player.Id)) && !player.IsDominator) player.SetEffect(effectType, mode);
            });
        }

        #endregion

        AudioMixerGroup IAudioMixer.GetTrack(EAudioTrackType trackType)
        {
            switch (trackType)
            {
                case EAudioTrackType.Generic: return _audioTrackPool.Extract();
                case EAudioTrackType.Dominator: return _dominatorTrackPool.Extract();
            }

            return null;
        }

        void IAudioMixer.ReturnTrack(EAudioTrackType trackType, AudioMixerGroup track)
        {
            switch (trackType)
            {
                case EAudioTrackType.Generic:
                    _audioTrackPool.Recycle(track);
                    break;
                case EAudioTrackType.Dominator:
                    _dominatorTrackPool.Recycle(track);
                    break;
            }
        }

        private void GetPlaybackPrefByType(EAudioType targetType, Action<AudioTypePlaybackPreference> onGetPref)
        {
            // For those which may be played in the future.
            AudioExtension.ForeachConcreteAudioType(audioType =>
            {
                if (targetType.HasFlagUnsafe(audioType) && _audioTypePreference.TryGetValue(audioType, out var preference)) onGetPref.Invoke(preference);
            });
        }

        private void GetCurrentActivePlayers(Action<AudioPlayer> onGetPlayer)
        {
            // For those which are currently playing.
            var players = _audioPlayerPool.GetCurrentAudioPlayers();
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player.IsActive) onGetPlayer?.Invoke(player);
                }
            }
        }

        private bool TryGetAvailablePlayer(int id, out AudioPlayer audioPlayer)
        {
            audioPlayer = null;
            if (AudioPlayer.resumablePlayers == null || !AudioPlayer.resumablePlayers.TryGetValue(id, out audioPlayer))
            {
                if (TryGetNewAudioPlayer(out var newPlayer)) audioPlayer = newPlayer;
            }

            return audioPlayer != null;
        }

        private bool TryGetNewAudioPlayer(out AudioPlayer player)
        {
            player = _audioPlayerPool.Extract();
            return player != null;
        }

        private AudioPlayer GetNewAudioPlayer() { return _audioPlayerPool.Extract(); }

        private IEnumerator PreventCombFiltering(int id, float preventTime)
        {
            _combFilteringPreventer[id] = true;
            var waitInstruction = preventTime > Time.deltaTime ? new WaitForSeconds(preventTime) : null;
            yield return waitInstruction;
            _combFilteringPreventer[id] = false;
        }

        #region NullChecker

        private bool IsPlayable(int id, out IAudioEntity entity)
        {
            entity = null;
            if (id <= 0 || !_audioBank.TryGetValue(id, out entity))
            {
                Debug.LogError(AudioConstant.LOG_HEADER + $"The sound is missing or it has never been assigned. No sound will be played. SoundID:{id}");
                return false;
            }

            if (_combFilteringPreventer.TryGetValue(id, out bool isPreventing) && isPreventing)
            {
#if UNITY_EDITOR
                if (AudioSettings.LogCombFilteringWarning)
                {
                    Debug.LogWarning(AudioConstant.LOG_HEADER +
                                     $"One of the plays of Audio:{id.ToName().ToWhiteBold()} has been rejected due to the concern about sound quality. " +
                                     "For more information, please go to the [Comb Filtering] section Audio tab in Wizard");
                }
#endif
                return false;
            }

            return true;
        }

        #endregion

        public string GetNameByID(int id)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError(AudioConstant.LOG_HEADER + $"The method {"GetNameByID".ToWhiteBold()} is {"Runtime Only".ToBold().SetColor(Color.green)}");
                return null;
            }

            var result = string.Empty;
            if (_audioBank.TryGetValue(id, out var entity))
            {
                var entityIdentity = entity as IAudioIdentity;
                result = entityIdentity?.Name;
            }

            return result;
        }

        #region Editor

#if UNITY_EDITOR
        public void AddAsset(ScriptableObject asset)
        {
            if (!soundAssets.Contains(asset)) soundAssets.Add(asset);
        }

        public void RemoveDeletedAsset(ScriptableObject asset)
        {
            for (int i = soundAssets.Count - 1; i >= 0; i--)
            {
                if (soundAssets[i] == asset) soundAssets.RemoveAt(i);
            }
        }
#endif

        #endregion

        #region Playback

        #region Play

        public IAudioPlayer Play(int id)
        {
            if (IsPlayable(id, out var entity) && TryGetAvailablePlayer(id, out var player))
            {
                var pref = new PlaybackPreference(entity);
                return PlayerToPlay(id, player, pref);
            }

            return null;
        }

        public IAudioPlayer Play(int id, Vector3 position)
        {
            if (IsPlayable(id, out var entity) && TryGetAvailablePlayer(id, out var player))
            {
                var pref = new PlaybackPreference(entity, position);
                return PlayerToPlay(id, player, pref);
            }

            return null;
        }

        public IAudioPlayer Play(int id, Transform followTarget)
        {
            if (IsPlayable(id, out var entity) && TryGetAvailablePlayer(id, out var player))
            {
                var pref = new PlaybackPreference(entity, followTarget);
                return PlayerToPlay(id, player, pref);
            }

            return null;
        }

        private IAudioPlayer PlayerToPlay(int id, AudioPlayer player, PlaybackPreference pref)
        {
            var audioType = AudioExtension.GetAudioType(id);
            if (_audioTypePreference.TryGetValue(audioType, out var audioTypePref)) pref.AudioTypePlaybackPreference = audioTypePref;

            player.Play(id, pref);
            var wrapper = new AudioPlayerInstanceWrapper(player);

            if (AudioSettings.AlwaysPlayMusicAsBGM && audioType == EAudioType.Music)
            {
                player.AsBGM().SetTransition(AudioSettings.DefaultBGMTransition, AudioSettings.DefaultBGMTransitionTime);
            }

            if (AudioSettings.CombFilteringPreventionInSeconds > 0f)
            {
                StartCoroutine(PreventCombFiltering(id, AudioSettings.CombFilteringPreventionInSeconds));
            }

            if (pref.entity.SeamlessLoop)
            {
                var seamlessLoopHelper = new SeamlessLoopHelper(wrapper, GetNewAudioPlayer);
                seamlessLoopHelper.SetPlayer(player);
            }

            return wrapper;
        }

        #endregion

        #region Stop

        public void Stop(EAudioType targetType) { Stop(targetType, AudioPlayer.USE_ENTITY_SETTING); }

        public void Stop(int id) { Stop(id, AudioPlayer.USE_ENTITY_SETTING); }

        public void Stop(int id, float fadeTime) { StopPlayer(fadeTime, x => x.Id == id); }

        public void Stop(EAudioType targetType, float fadeTime) { StopPlayer(fadeTime, x => targetType.HasFlagUnsafe(AudioExtension.GetAudioType(x.Id))); }

        private void StopPlayer(float fadeTime, Predicate<AudioPlayer> predicate)
        {
            var players = _audioPlayerPool.GetCurrentAudioPlayers();
            if (players != null)
            {
                for (int i = players.Count - 1; i >= 0; i--)
                {
                    var player = players[i];
                    if (player.IsActive && predicate.Invoke(player)) player.Stop(fadeTime);
                }
            }
        }

        #endregion

        public void Pause(int id, float fadeTime = AudioPlayer.USE_ENTITY_SETTING)
        {
            GetCurrentActivePlayers(player =>
            {
                if (player.Id == id) player.Stop(fadeTime, EAudioStopMode.Pause, null);
            });
        }

        #endregion
    }
}