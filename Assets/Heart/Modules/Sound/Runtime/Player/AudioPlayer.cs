using System;
using System.Collections;
using System.Collections.Generic;
using LitMotion;
using Pancake.Common;
using UnityEngine;
using UnityEngine.Audio;


namespace Pancake.Sound
{
    [RequireComponent(typeof(AudioSource)), AddComponentMenu("")]
    public class AudioPlayer : MonoBehaviour, IAudioPlayer, IRecyclable<AudioPlayer>
    {
        public delegate void SeamlessLoopReplay(int id, PlaybackPreference pref, EEffectType effectType, float trackVolume, float pitch);

        private enum VolumeControl
        {
            Clip,
            Track,
            MixerDecibel,
        }

        public const float USE_ENTITY_SETTING = -1f;
        public const float IMMEDIATE = 0f;
        private const int DECORATORS_ARRAY_SIZE = 2;
        public const float DEFAULT_CLIP_VOLUME = 0f;
        public const float DEFAULT_TRACK_VOLUME = AudioConstant.FULL_VOLUME;
        public const float DEFAULT_MIXER_DECIBEL_VOLUME = int.MinValue;

        public event Action<AudioPlayer> OnRecycle;
        public static Dictionary<int, AudioPlayer> resumablePlayers;
        public event SeamlessLoopReplay OnSeamlessLoopReplay;
        public event Action<SoundId> OnEndPlaying;

        [SerializeField] private AudioSource audioSource;

        private AudioMixer _audioMixer;
        private Func<EAudioTrackType, AudioMixerGroup> _getAudioTrack;
        private ISoundClip _currentClip;
        private AudioPlayerDecorator[] _decorators;
        private string _sendParaName = string.Empty;
        private string _currentTrackName = string.Empty;
        private EAudioStopMode _stopMode;
        private AsyncProcessHandle _playbackControlHandle;
        private AsyncProcessHandle _trackVolumeControlHandle;
        private float _clipVolume;
        private float _trackVolume = DEFAULT_TRACK_VOLUME;
        private float _mixerDecibelVolume = DEFAULT_MIXER_DECIBEL_VOLUME;
        private AsyncProcessHandle _pitchCoroutine;

        public SoundId Id { get; private set; } = -1;
        public bool IsPlaying => audioSource.isPlaying;
        public bool IsActive => Id > 0;
        public bool IsStopping { get; private set; }
        public bool IsFadingOut { get; private set; }
        public bool IsFadingIn { get; private set; }
        public EEffectType CurrentActiveEffects { get; private set; } = EEffectType.None;
        public bool IsUsingEffect => CurrentActiveEffects != EEffectType.None;
        public bool IsDominator => TryGetDecorator<DominatorPlayer>(out _);
        public bool IsBGM => TryGetDecorator<MusicPlayer>(out _);
        public int PlaybackStartingTime { get; private set; }

        /// <summary>
        /// Pitch value without any fading process
        /// </summary>
        public float StaticPitch { get; private set; } = AudioConstant.DEFAULT_PITCH;

        public string VolumeParaName
        {
            get
            {
                if (IsUsingEffect) return _sendParaName;
                if (AudioTrack) return _currentTrackName;
                return string.Empty;
            }
        }

        public EAudioTrackType TrackType { get; private set; } = EAudioTrackType.Generic;

        public AudioMixerGroup AudioTrack
        {
            get => audioSource.outputAudioMixerGroup;
            private set
            {
                audioSource.outputAudioMixerGroup = value;
                _currentTrackName = value == null ? string.Empty : value.name;
                _sendParaName = value == null ? string.Empty : _currentTrackName + AudioConstant.EFFECT_PARA_NAME_SUFFIX;
            }
        }

        protected virtual void Awake() { audioSource ??= GetComponent<AudioSource>(); }

        public void SetData(AudioMixer mixer, Func<EAudioTrackType, AudioMixerGroup> getAudioTrack)
        {
            _audioMixer = mixer;
            _getAudioTrack = getAudioTrack;
        }

        private void SetSpatial(PlaybackPreference pref)
        {
            SpatialSetting setting = pref.entity.SpatialSetting;
            SetSpatialBlend();

            if (setting == null)
            {
                ResetAudioSourceSpatial();
                return;
            }

            audioSource.panStereo = setting.stereoPan;
            audioSource.dopplerLevel = setting.dopplerLevel;
            audioSource.minDistance = setting.minDistance;
            audioSource.maxDistance = setting.maxDistance;

            audioSource.SetCustomCurveOrResetDefault(setting.reverbZoneMix, AudioSourceCurveType.ReverbZoneMix);
            audioSource.SetCustomCurveOrResetDefault(setting.spread, AudioSourceCurveType.Spread);

            audioSource.rolloffMode = setting.rolloffMode;
            if (setting.rolloffMode == AudioRolloffMode.Custom)
            {
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, setting.customRolloff);
            }

            return;

            void SetSpatialBlend()
            {
                if (pref.followTarget != null && transform.parent != pref.followTarget)
                {
                    transform.SetParent(pref.followTarget, false);
                    SetTo3D();
                }
                else if (pref.HasPosition(out var position))
                {
                    transform.position = position;
                    SetTo3D();
                }
                else if (setting != null && !setting.spatialBlend.IsDefaultCurve(AudioConstant.SPATIAL_BLEND_2D) && pref.entity is IAudioIdentity entity)
                {
                    Debug.LogWarning(AudioConstant.LOG_HEADER +
                                     $"You've set a non-2D SpatialBlend for :{entity.Name}, but didn't specify a position or a follow target when playing it");
                }
            }

            void SetTo3D()
            {
                if (setting != null && !setting.spatialBlend.IsDefaultCurve(AudioConstant.SPATIAL_BLEND_2D))
                {
                    // Don't use SetCustomCurveOrResetDefault, it will set to 2D if isDefaultCurve.
                    audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, setting.spatialBlend);
                }
                else
                {
                    // force to 3D if it's played with a position or a follow target, even if it has no custom curve. 
                    audioSource.spatialBlend = AudioConstant.SPATIAL_BLEND_3D;
                }
            }
        }

        private void ResetAudioSourceSpatial()
        {
            audioSource.panStereo = AudioConstant.DEFAULT_PAN_STEREO;
            audioSource.dopplerLevel = AudioConstant.DEFAULT_DOPPLER;
            audioSource.minDistance = AudioConstant.ATTENUATION_MIN_DISTANCE;
            audioSource.maxDistance = AudioConstant.ATTENUATION_MAX_DISTANCE;
            audioSource.reverbZoneMix = AudioConstant.DEFAULT_REVER_ZONE_MIX;
            audioSource.spread = AudioConstant.DEFAULT_SPREAD;
            audioSource.rolloffMode = AudioConstant.DEFAULT_ROLLOFF_MODE;
        }

        private void ResetSpatial()
        {
            audioSource.spatialBlend = AudioConstant.SPATIAL_BLEND_2D;
            if (transform.parent != SoundManager.Instance.transform) transform.SetParent(SoundManager.Instance.transform);
            transform.position = Vector3.zero;
        }

        public void SetEffect(EEffectType effect, ESetEffectMode mode)
        {
            if ((effect == EEffectType.None && mode != ESetEffectMode.Override) || Id <= 0)
            {
                return;
            }

            bool oldUsingEffectState = IsUsingEffect;
            switch (mode)
            {
                case ESetEffectMode.Add:
                    CurrentActiveEffects |= effect;
                    break;
                case ESetEffectMode.Remove:
                    CurrentActiveEffects &= ~effect;
                    break;
                case ESetEffectMode.Override:
                    CurrentActiveEffects = effect;
                    break;
            }

            bool newUsingEffectState = IsUsingEffect;
            if (oldUsingEffectState != newUsingEffectState)
            {
                string from = IsUsingEffect ? _currentTrackName : _sendParaName;
                string to = IsUsingEffect ? _sendParaName : _currentTrackName;
                _audioMixer.ChangeChannel(from, to, MixerDecibelVolume);
            }
        }

        private void ResetEffect()
        {
            if (IsUsingEffect) _audioMixer.SafeSetFloat(_sendParaName, AudioConstant.MIN_DECIBEL_VOLUME);

            CurrentActiveEffects = EEffectType.None;
        }

        IMusicPlayer IMusicDecoratable.AsBGM() { return GetOrCreateDecorator(() => new MusicPlayer(this)); }

        IPlayerEffect IEffectDecoratable.AsDominator() { return GetOrCreateDecorator(() => new DominatorPlayer(this)); }

        private T GetOrCreateDecorator<T>(Func<T> onCreateDecorator) where T : AudioPlayerDecorator
        {
            if (_decorators != null && TryGetDecorator(out T decoratePlayer)) return decoratePlayer;

            decoratePlayer = null;
            _decorators ??= new AudioPlayerDecorator[DECORATORS_ARRAY_SIZE];
            for (int i = 0; i < _decorators.Length; i++)
            {
                if (_decorators[i] == null)
                {
                    decoratePlayer = onCreateDecorator.Invoke();
                    _decorators[i] = decoratePlayer;
                    break;
                }
            }

            if (decoratePlayer == null) Debug.LogError(AudioConstant.LOG_HEADER + "Audio Player decorators array size is too small");

            return decoratePlayer;
        }

        private bool TryGetDecorator<T>(out T result) where T : AudioPlayerDecorator
        {
            result = null;
            if (_decorators != null)
            {
                foreach (var deco in _decorators)
                {
                    if (deco is T target)
                    {
                        result = target;
                        return true;
                    }
                }
            }

            return false;
        }

        private void Recycle()
        {
            MixerDecibelVolume = AudioConstant.MIN_DECIBEL_VOLUME;
            OnRecycle?.Invoke(this);

            TrackType = EAudioTrackType.Generic;
            AudioTrack = null;
            _decorators = null;
        }

        #region Playback

        public void Play(int id, PlaybackPreference pref)
        {
            if (IsStopping) return;

            Id = id;
            PlaybackStartingTime = C.UnscaledCurrentFrameBeganTime;
            if (_stopMode == default) _currentClip = pref.entity.PickNewClip();

            App.StopAndReassign(ref _playbackControlHandle, PlayControl(pref));
        }

        private IEnumerator PlayControl(PlaybackPreference pref)
        {
            IAudioPlaybackPreference audioTypePlaybackPref = null;
            if (SoundManager.Instance.AudioTypePreferences.TryGetValue(Id.ToAudioType(), out var typePref))
            {
                audioTypePlaybackPref = typePref;
            }

            if (!RemoveFromResumablePlayer()) // if is not resumable (not paused)
            {
                if (pref.PlayerWaiter != null)
                {
                    var cache = pref;
                    yield return new WaitUntil(() => cache.PlayerWaiter.IsFinished);
                    pref.DisposeWaiter();
                }

                if (_currentClip.Delay > 0) yield return new WaitForSeconds(_currentClip.Delay);

                audioSource.clip = _currentClip.AudioClip;
                audioSource.priority = pref.entity.Priority;
                SetInitialPitch(pref.entity, audioTypePlaybackPref);
                SetSpatial(pref);

                if (TryGetDecorator<MusicPlayer>(out var musicPlayer))
                {
                    audioSource.reverbZoneMix = 0f;
                    audioSource.priority = AudioConstant.HIGHEST_PRIORITY;
                    pref = musicPlayer.Transition(pref);
                }

                if (IsDominator) TrackType = EAudioTrackType.Dominator;
                else SetEffect(audioTypePlaybackPref.EffectType, ESetEffectMode.Add);
            }

            App.StopAndClean(ref _trackVolumeControlHandle);
            TrackVolume = StaticTrackVolume * audioTypePlaybackPref.Volume;
            ClipVolume = 0f;
            float targetClipVolume = _currentClip.Volume * pref.entity.GetMasterVolume();
            AudioTrack = _getAudioTrack?.Invoke(TrackType);

            int sampleRate = _currentClip.AudioClip.frequency;
            const VolumeControl fader = VolumeControl.Clip;
            do
            {
                switch (_stopMode)
                {
                    case EAudioStopMode.Stop:
                        PlayFromPos(_currentClip.StartPosition);
                        break;
                    case EAudioStopMode.Pause:
                        audioSource.UnPause();
                        break;
                    case EAudioStopMode.Mute:
                        this.SetVolume(StaticTrackVolume);
                        if (!audioSource.isPlaying) PlayFromPos(_currentClip.StartPosition);
                        break;
                }

                _stopMode = default;

                #region FadeIn

                if (HasFading(_currentClip.FadeIn, pref.FadeIn, out float fadeIn))
                {
                    IsFadingIn = true;
                    //FadeIn start from here
                    yield return Fade(targetClipVolume, fadeIn, fader, pref.FadeInEase);
                    IsFadingIn = false;
                }
                else
                {
                    ClipVolume = targetClipVolume;
                }

                #endregion

                if (pref.entity.SeamlessLoop) pref.ApplySeamlessFade();

                #region FadeOut

                int endSample = (int) (audioSource.clip.samples - (_currentClip.EndPosition * sampleRate));
                if (HasFading(_currentClip.FadeOut, pref.FadeOut, out float fadeOut))
                {
                    yield return new WaitUntil(() => endSample - audioSource.timeSamples <= fadeOut * sampleRate);

                    IsFadingOut = true;
                    TriggerSeamlessLoopReplay(pref);
                    //FadeOut start from here
                    yield return Fade(0f, fadeOut, fader, pref.FadeOutEase);
                    IsFadingOut = false;
                }
                else
                {
                    bool hasPlayed = false;
                    yield return new WaitUntil(() => HasEndPlaying(ref hasPlayed) || endSample - audioSource.timeSamples <= 0);
                    TriggerSeamlessLoopReplay(pref);
                }

                #endregion
            } while (pref.entity.Loop);

            EndPlaying();

            void PlayFromPos(float pos)
            {
                audioSource.Stop();
                audioSource.timeSamples = (int) (pos * sampleRate);
                audioSource.Play();
            }

            // more accurate than AudioSource.isPlaying
            bool HasEndPlaying(ref bool hasPlayed)
            {
                int timeSample = audioSource.timeSamples;
                if (!hasPlayed) hasPlayed = timeSample > 0;

                return hasPlayed && timeSample == 0;
            }
        }

        private void TriggerSeamlessLoopReplay(PlaybackPreference pref)
        {
            OnSeamlessLoopReplay?.Invoke(Id,
                pref,
                CurrentActiveEffects,
                StaticTrackVolume,
                StaticPitch);
            OnSeamlessLoopReplay = null;
        }

        #region Stop Overloads

        void IAudioStoppable.Pause() => this.Pause(USE_ENTITY_SETTING);
        void IAudioStoppable.Pause(float fadeOut) => Stop(fadeOut, EAudioStopMode.Pause, null);
        void IAudioStoppable.Stop() => this.Stop(USE_ENTITY_SETTING);
        void IAudioStoppable.Stop(float fadeOut) => this.Stop(fadeOut, null);
        void IAudioStoppable.Stop(Action onFinished) => this.Stop(USE_ENTITY_SETTING, onFinished);
        void IAudioStoppable.Stop(float fadeOut, Action onFinished) => Stop(fadeOut, EAudioStopMode.Stop, onFinished);

        #endregion

        public void Stop(float overrideFade, EAudioStopMode stopMode, Action onFinished)
        {
            if (IsStopping && Mathf.Approximately(overrideFade, IMMEDIATE)) return;

            if (Id <= 0 || !audioSource.isPlaying)
            {
                onFinished?.Invoke();
                return;
            }

            App.StopAndReassign(ref _playbackControlHandle, StopControl(overrideFade, stopMode, onFinished));
        }

        private IEnumerator StopControl(float overrideFade, EAudioStopMode stopMode, Action onFinished)
        {
            _stopMode = stopMode;
            IsStopping = true;

            #region FadeOut

            if (HasFading(_currentClip.FadeOut, overrideFade, out float fadeTime))
            {
                if (IsFadingOut)
                {
                    // if it is fading out. then don't stop. just wait for it
                    var clip = audioSource.clip;
                    float endSample = clip.samples - (_currentClip.EndPosition * clip.frequency);
                    yield return new WaitUntil(() => audioSource.timeSamples >= endSample);
                }
                else
                {
                    yield return Fade(0f, fadeTime, VolumeControl.Clip, AudioSettings.DefaultFadeOutEase);
                }
            }

            #endregion

            switch (stopMode)
            {
                case EAudioStopMode.Stop:
                    EndPlaying();
                    break;
                case EAudioStopMode.Pause:
                    audioSource.Pause();
                    AddResumablePlayer();
                    break;
                case EAudioStopMode.Mute:
                    StaticTrackVolume = TrackVolume;
                    this.SetVolume(0f);
                    AddResumablePlayer();
                    break;
            }

            IsStopping = false;
            onFinished?.Invoke();
        }

        private bool HasFading(float clipFade, float overrideFade, out float fadeTime)
        {
            fadeTime = clipFade;
            if (Mathf.Approximately(overrideFade, USE_ENTITY_SETTING)) fadeTime = overrideFade;

            return fadeTime > IMMEDIATE;
        }

        private void AddResumablePlayer()
        {
            resumablePlayers ??= new Dictionary<int, AudioPlayer>();
            resumablePlayers[Id] = this;
        }

        private bool RemoveFromResumablePlayer() { return resumablePlayers?.Remove(Id) ?? false; }

        private void EndPlaying()
        {
            PlaybackStartingTime = 0;
            _stopMode = default;
            ResetVolume();
            ResetPitch();

            audioSource.Stop();
            audioSource.clip = null;
            _currentClip = null;
            ResetSpatial();
            ResetEffect();

            App.StopAndClean(ref _trackVolumeControlHandle);
            RemoveFromResumablePlayer();
            OnEndPlaying?.Invoke(Id);
            OnEndPlaying = null;
            Id = -1;
            Recycle();
        }

        IAudioPlayer IPitchSettable.SetPitch(float pitch, float fadeTime)
        {
            StaticPitch = pitch;
            switch (AudioSettings.PitchSetting)
            {
                case EPitchShiftingSetting.AudioMixer:
                    //_audioMixer.SafeSetFloat(_pitchParaName, pitch); // Don't * 100f, the value in percentage is displayed in Editor only.  
                    break;
                case EPitchShiftingSetting.AudioSource:
                    pitch = Mathf.Clamp(pitch, AudioConstant.MIN_AUDIO_SOURCE_PITCH, AudioConstant.MAX_AUDIO_SOURCE_PITCH);
                    if (fadeTime > 0f) App.StopAndReassign(ref _pitchCoroutine, PitchControl(pitch, fadeTime));
                    else audioSource.pitch = pitch;
                    break;
            }

            return this;
        }

        private void SetInitialPitch(IAudioEntity entity, IAudioPlaybackPreference audioTypePlaybackPref)
        {
            float pitch;
            if (!Mathf.Approximately(StaticPitch, AudioConstant.DEFAULT_PITCH))
            {
                pitch = entity.GetRandomValue(StaticPitch, ERandomFlag.Pitch);
            }
            else if (!Mathf.Approximately(audioTypePlaybackPref.Pitch, AudioConstant.DEFAULT_PITCH))
            {
                pitch = entity.GetRandomValue(audioTypePlaybackPref.Pitch, ERandomFlag.Pitch);
            }
            else
            {
                pitch = entity.GetPitch();
            }

            audioSource.pitch = pitch;
        }

        private IEnumerator PitchControl(float targetPitch, float fadeTime)
        {
            var pitchs = AudioExtension.GetLerpValuesPerFrame(audioSource.pitch, targetPitch, fadeTime, Ease.Linear);

            foreach (var pitch in pitchs)
            {
                audioSource.pitch = pitch;
                yield return null;
            }
        }

        private void ResetPitch()
        {
            StaticPitch = AudioConstant.DEFAULT_PITCH;
            audioSource.pitch = AudioConstant.DEFAULT_PITCH;
        }

        #endregion

        #region Volume

        /// <summary>
        /// The playback volume of an audio clip, it's determined by the clip's property settings, such as Fade In/Out.   
        /// </summary>
        public float ClipVolume
        {
            get => _clipVolume;
            private set
            {
                _clipVolume = value;
#if UNITY_WEBGL
                WebGLSetVolume();
#else
                MixerDecibelVolume = (_clipVolume * _trackVolume).ToDecibel();
#endif
            }
        }

        /// <summary>
        /// The playback volume of an audio track, it will be set by SetVolume() function.
        /// </summary>
        public float TrackVolume
        {
            get => _trackVolume;
            private set
            {
                _trackVolume = value;
#if UNITY_WEBGL
                WebGLSetVolume();
#else
                MixerDecibelVolume = (_clipVolume * _trackVolume).ToDecibel();
#endif
            }
        }

        /// <summary>
        /// Track volume without any fading process
        /// </summary>
        public float StaticTrackVolume { get; private set; } = DEFAULT_TRACK_VOLUME;

        /// <summary>
        /// The final decibel volume that is set in the mixer.
        /// </summary>
        public float MixerDecibelVolume
        {
            get
            {
                if (Mathf.Approximately(_mixerDecibelVolume, DEFAULT_MIXER_DECIBEL_VOLUME))
                {
                    if (_audioMixer.SafeGetFloat(VolumeParaName, out float currentVol)) _mixerDecibelVolume = currentVol;
                }

                return _mixerDecibelVolume;
            }
            private set
            {
                _mixerDecibelVolume = value.ClampDecibel(true);
                _audioMixer.SafeSetFloat(VolumeParaName, _mixerDecibelVolume);
            }
        }

        IAudioPlayer IVolumeSettable.SetVolume(float volume, float fadeTime)
        {
            StaticTrackVolume = volume; // in case the fading process is interrupted
            App.StopAndClean(ref _trackVolumeControlHandle);
            if (fadeTime > 0)
            {
                var ease = TrackVolume < volume ? AudioSettings.DefaultFadeInEase : AudioSettings.DefaultFadeOutEase;
                _trackVolumeControlHandle = App.StartCoroutine(Fade(volume, fadeTime, VolumeControl.Track, ease));
            }
            else
            {
                TrackVolume = volume;
            }

            return this;
        }

        private IEnumerator Fade(float targetVol, float duration, VolumeControl fader, Ease ease)
        {
            Func<float> getVol = null;
            Action<float> setVol = null;

            switch (fader)
            {
                case VolumeControl.Clip:
                    getVol = () => ClipVolume;
                    setVol = vol => ClipVolume = vol;
                    break;
                case VolumeControl.Track:
                    getVol = () => TrackVolume;
                    setVol = vol => TrackVolume = vol;
                    break;
                case VolumeControl.MixerDecibel:
                    break;
            }

            if (duration <= 0)
            {
                setVol?.Invoke(targetVol);
                yield break;
            }

            float startVol = getVol?.Invoke() ?? 0;

            var volumes = AudioExtension.GetLerpValuesPerFrame(startVol, targetVol, duration, ease);
            if (volumes != null)
            {
                foreach (float vol in volumes)
                {
                    setVol?.Invoke(vol);
                    yield return null;
                }
            }
        }

        private void ResetVolume()
        {
            _clipVolume = DEFAULT_CLIP_VOLUME;
            _trackVolume = DEFAULT_TRACK_VOLUME;
            _mixerDecibelVolume = DEFAULT_MIXER_DECIBEL_VOLUME;
            StaticTrackVolume = DEFAULT_TRACK_VOLUME;
        }

        #endregion
    }
}