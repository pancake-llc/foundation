using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Threading;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Sound.Reflection;
using UnityEngine.Audio;
using Math = System.Math;

#if UNITY_EDITOR
namespace PancakeEditor.Sound
{
    public class EditorPlayAudioClip
    {
        public class Data
        {
            public AudioClip AudioClip;
            public float Volume;
            public float StartPosition;
            public float EndPosition;
            public float FadeIn;
            public float FadeOut;

            public Data(AudioClip audioClip, float volume, Transport transport)
            {
                AudioClip = audioClip;
                Volume = volume;
                StartPosition = transport.StartPosition;
                EndPosition = transport.EndPosition;
                FadeIn = transport.FadeIn;
                FadeOut = transport.FadeOut;
            }

            public Data(SoundClip broAudioClip)
            {
                AudioClip = broAudioClip.AudioClip;
                Volume = broAudioClip.Volume;
                StartPosition = broAudioClip.StartPosition;
                EndPosition = broAudioClip.EndPosition;
                FadeIn = broAudioClip.FadeIn;
                FadeOut = broAudioClip.FadeOut;
            }

            public float Duration => AudioClip.length - EndPosition - StartPosition;
        }

        public enum MuteState
        {
            None,
            On,
            Off
        }

        public delegate void PlayPreviewClip(AudioClip audioClip, int startSample, bool loop);

        public delegate void StopAllPreviewClips();

        public const string AUDIO_UTIL_CLASS_NAME = "AudioUtil";
        public const string IGNORE_SETTING_TOOLTIP = "Right-click to play the audio clip directly";
        public const string PLAY_CLIP_METHOD_NAME = "PlayPreviewClip";

        public const string STOP_CLIP_METHOD_NAME = "StopAllPreviewClips";

        // This is used to avoid the popup sound at the beginning of the fade-in due to the delay of mixer volume change
        public const float SET_VOLUME_OFFSET_TIME = 0.05f;

        private static EditorPlayAudioClip instance;

        public static EditorPlayAudioClip In
        {
            get
            {
                instance ??= new EditorPlayAudioClip();
                return instance;
            }
        }

        public PlaybackIndicatorUpdater PlaybackIndicator { get; private set; }
        public Action OnFinished;

        private StopAllPreviewClips _stopAllPreviewClipsDelegate;
        private PlayPreviewClip _playPreviewClipDelegate;
        private CancellationTokenSource _playClipTaskCanceller;
        private CancellationTokenSource _audioSourceTaskCanceller;
        private AudioSource _currentEditorAudioSource;
        private Data _currentPlayingClip;
        private bool _isRecursionOutside;
        private AudioMixer _mixer;
        private EditorAudioPreviewer _volumeTransporter;
        private MuteState _previousMuteState = MuteState.None;

        public StopAllPreviewClips StopAllPreviewClipsDelegate
        {
            get
            {
                _stopAllPreviewClipsDelegate ??= GetAudioUtilMethodDelegate<StopAllPreviewClips>(STOP_CLIP_METHOD_NAME);
                return _stopAllPreviewClipsDelegate;
            }
        }

        public PlayPreviewClip PlayPreviewClipDelegate
        {
            get
            {
                _playPreviewClipDelegate ??= GetAudioUtilMethodDelegate<PlayPreviewClip>(PLAY_CLIP_METHOD_NAME);
                return _playPreviewClipDelegate;
            }
        }

        public EditorPlayAudioClip()
        {
            _mixer = Resources.Load<AudioMixer>($"EditorAudioMixer");
            PlaybackIndicator = new PlaybackIndicatorUpdater();
            _volumeTransporter = new EditorAudioPreviewer(_mixer);

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public async void PlayClipByAudioSource(Data clip, bool selfLoop = false, Action onReplay = null, float pitch = 1f)
        {
            StopStaticPreviewClipsAndCancelTask();
            ResetAndGetAudioSource(out var audioSource);

            SetAudioSource(ref audioSource, clip, pitch);
            _currentPlayingClip = clip;
            _previousMuteState = EditorUtility.audioMasterMute ? MuteState.On : MuteState.Off;

            _volumeTransporter.SetData(clip);

            audioSource.Play();
            PlaybackIndicator.Start(selfLoop);
            _volumeTransporter.Start();
            EditorUtility.audioMasterMute = false;

            _audioSourceTaskCanceller ??= new CancellationTokenSource();

            float waitTime = clip.Duration / pitch;
            if (clip.FadeIn >= SET_VOLUME_OFFSET_TIME)
            {
                audioSource.volume = 0f;
                await C.TaskDelaySeconds(SET_VOLUME_OFFSET_TIME, _audioSourceTaskCanceller.Token);
                audioSource.volume = 1f;
                waitTime -= SET_VOLUME_OFFSET_TIME;

                if (_audioSourceTaskCanceller.IsCanceled())
                {
                    return;
                }
            }

            await C.TaskDelaySeconds(waitTime, _audioSourceTaskCanceller.Token);
            if (_audioSourceTaskCanceller.IsCanceled()) return;

            _isRecursionOutside = onReplay != null;
            if (_isRecursionOutside) onReplay.Invoke();
            else
            {
                if (selfLoop) AudioSourceReplay();
                else DestroyPreviewAudioSourceAndCancelTask();
            }
        }

        private void ResetAndGetAudioSource(out AudioSource result)
        {
            if (_currentEditorAudioSource)
            {
                CancelTask(ref _audioSourceTaskCanceller);
                _currentEditorAudioSource.Stop();
            }
            else
            {
                var gameObj = new GameObject("PreviewAudioClip");
                gameObj.tag = "EditorOnly";
                gameObj.hideFlags = HideFlags.HideAndDontSave;
                _currentEditorAudioSource = gameObj.AddComponent<AudioSource>();
            }

            result = _currentEditorAudioSource;
        }

        private void SetAudioSource(ref AudioSource audioSource, Data clip, float pitch)
        {
            audioSource.clip = clip.AudioClip;
            audioSource.playOnAwake = false;
            audioSource.time = clip.StartPosition;
            audioSource.pitch = pitch;
            audioSource.outputAudioMixerGroup = GetEditorMasterTrack();
            audioSource.reverbZoneMix = 0f;
        }

        private async void AudioSourceReplay()
        {
            if (_currentEditorAudioSource != null && _audioSourceTaskCanceller != null)
            {
                _volumeTransporter.End();
                PlaybackIndicator.End();

                _currentEditorAudioSource.Stop();
                _currentEditorAudioSource.time = _currentPlayingClip.StartPosition;
                _currentEditorAudioSource.Play();

                _volumeTransporter.Start();
                PlaybackIndicator.Start();
                await C.TaskDelaySeconds(_currentPlayingClip.Duration, _audioSourceTaskCanceller.Token);
                if (!_audioSourceTaskCanceller.IsCanceled())
                {
                    // Recursive
                    AudioSourceReplay();
                }
            }
        }

        public void PlayClip(AudioClip audioClip, float startTime, float endTime, bool loop = false)
        {
            int startSample = AudioExtension.GetTimeSample(audioClip, startTime);
            int endSample = AudioExtension.GetTimeSample(audioClip, endTime);
            PlayClip(audioClip, startSample, endSample, loop);
        }

        public async void PlayClip(AudioClip audioClip, int startSample, int endSample, bool loop = false)
        {
            StopAllClips();

            PlayPreviewClipDelegate.Invoke(audioClip, startSample, loop);
            PlaybackIndicator.Start(loop);

            int sampleLength = audioClip.samples - startSample - endSample;
            int lengthInMs = (int) Math.Round(sampleLength / (double) audioClip.frequency * AudioConstant.SECOND_IN_MILLISECONDS, MidpointRounding.AwayFromZero);

            _playClipTaskCanceller ??= new CancellationTokenSource();
            await C.TaskDelay(lengthInMs, _playClipTaskCanceller.Token);
            if (_playClipTaskCanceller.IsCanceled()) return;

            if (loop) AudioClipReplay(audioClip, startSample, loop, lengthInMs);
            else StopStaticPreviewClipsAndCancelTask();
        }

        private async void AudioClipReplay(AudioClip audioClip, int startSample, bool loop, int lengthInMs)
        {
            if (_playClipTaskCanceller != null)
            {
                StopAllPreviewClipsDelegate.Invoke();
                PlayPreviewClipDelegate.Invoke(audioClip, startSample, loop);
                PlaybackIndicator.End();
                PlaybackIndicator.Start();

                await C.TaskDelay(lengthInMs, _playClipTaskCanceller.Token);
                if (!_playClipTaskCanceller.IsCanceled())
                {
                    // Recursive
                    AudioClipReplay(audioClip, startSample, loop, lengthInMs);
                }
            }
        }

        public void StopAllClips()
        {
            _isRecursionOutside = false;
            StopStaticPreviewClipsAndCancelTask();
            DestroyPreviewAudioSourceAndCancelTask();

            if (_previousMuteState != MuteState.None)
            {
                EditorUtility.audioMasterMute = _previousMuteState == MuteState.On;
                _previousMuteState = MuteState.None;
            }
        }

        private void DestroyPreviewAudioSourceAndCancelTask()
        {
            if (_currentEditorAudioSource)
            {
                CancelTask(ref _audioSourceTaskCanceller);

                _currentEditorAudioSource.Stop();
                UnityEngine.Object.DestroyImmediate(_currentEditorAudioSource.gameObject);
                PlaybackIndicator.End();
                _volumeTransporter.End();
                _currentEditorAudioSource = null;
                TriggerOnFinished();
            }
        }

        private void StopStaticPreviewClipsAndCancelTask()
        {
            CancelTask(ref _playClipTaskCanceller);
            StopAllPreviewClipsDelegate.Invoke();
            PlaybackIndicator.End();
            TriggerOnFinished();
        }

        private void TriggerOnFinished()
        {
            if (!_isRecursionOutside)
            {
                OnFinished?.Invoke();
                OnFinished = null;
            }
        }

        public void AddPlaybackIndicatorListener(Action action)
        {
            RemovePlaybackIndicatorListener(action);
            PlaybackIndicator.OnUpdate += action;
            PlaybackIndicator.OnEnd += action;
        }

        public void RemovePlaybackIndicatorListener(Action action)
        {
            PlaybackIndicator.OnUpdate -= action;
            PlaybackIndicator.OnEnd -= action;
        }

        private void CancelTask(ref CancellationTokenSource cancellation)
        {
            if (cancellation != null && cancellation.Token.CanBeCanceled)
            {
                cancellation.Cancel();
                cancellation?.Dispose();
                cancellation = null;
            }
        }

        private void Dispose()
        {
            OnFinished = null;
            _currentPlayingClip = null;
            _mixer = null;
            _volumeTransporter.Dispose();
            _volumeTransporter = null;
            StopStaticPreviewClipsAndCancelTask();
            DestroyPreviewAudioSourceAndCancelTask();
            PlaybackIndicator.Dispose();
            PlaybackIndicator = null;
            instance = null;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (mode == PlayModeStateChange.ExitingEditMode || mode == PlayModeStateChange.ExitingPlayMode) Dispose();
        }

        private T GetAudioUtilMethodDelegate<T>(string methodName) where T : Delegate
        {
            Type audioUtilClass = AudioClassReflectionHelper.GetUnityEditorClass(AUDIO_UTIL_CLASS_NAME);
            MethodInfo method = audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        private AudioMixerGroup GetEditorMasterTrack()
        {
            var tracks = _mixer != null ? _mixer.FindMatchingGroups("Master") : null;
            if (tracks != null && tracks.Length > 0) return tracks[0];

            Debug.LogError("Can't find EditorAudioMixer's Master audioMixerGroup, the fading and extra volume is not applied to the preview");
            return null;
        }
    }
}
#endif