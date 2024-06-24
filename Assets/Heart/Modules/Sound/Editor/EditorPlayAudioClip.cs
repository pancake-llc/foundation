using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Threading;
using Pancake.Common;
using Pancake.Sound;
using Math = System.Math;

#if UNITY_EDITOR
namespace PancakeEditor.Sound
{
    public static class EditorPlayAudioClip
    {
        public const string PLAY_WITH_VOLUME_SETTING = "[Experimental]\nRight-click to play at the current volume (maximum at 0dB)";
        public const string PLAY_CLIP_METHOD_NAME = "PlayPreviewClip";
        public const string STOP_CLIP_METHOD_NAME = "StopAllPreviewClips";
        public static readonly PlaybackIndicatorUpdater PlaybackIndicator = new();
        public static AudioClip CurrentPlayingClip { get; private set; }
        public static bool IsPlaying => CurrentPlayingClip != null;

        private static CancellationTokenSource currentPlayingTaskCanceller;
        private static CancellationTokenSource currentAudioSourceTaskCanceller;
        private static AudioSource currentEditorAudioSource;

        public static void PlayClipByAudioSource(AudioClip audioClip, float volume, float startTime, float endTime, bool loop = false)
        {
            StopAllPreviewClipsAndCancelTask();
            if (currentEditorAudioSource)
            {
                CancelTask(ref currentAudioSourceTaskCanceller);
                currentEditorAudioSource.Stop();
            }
            else
            {
                var gameObj = new GameObject("PreviewAudioClip") {tag = "EditorOnly", hideFlags = HideFlags.HideAndDontSave};
                currentEditorAudioSource = gameObj.AddComponent<AudioSource>();
            }

            var audioSource = currentEditorAudioSource;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.time = startTime;

            audioSource.Play();
            PlaybackIndicator.Start(loop);
            CurrentPlayingClip = audioClip;

            float duration = audioClip.length - endTime - startTime;
            currentAudioSourceTaskCanceller ??= new CancellationTokenSource();
            if (loop)
            {
                App.Task.DelayInvoke(duration, Replay, currentAudioSourceTaskCanceller.Token);
            }
            else
            {
                App.Task.DelayInvoke(duration, DestroyPreviewAudioSourceAndCancelTask, currentAudioSourceTaskCanceller.Token);
            }

            void Replay()
            {
                if (audioSource != null && currentAudioSourceTaskCanceller != null)
                {
                    currentEditorAudioSource.Stop();
                    audioSource.time = startTime;
                    audioSource.Play();
                    App.Task.DelayInvoke(duration, Replay, currentAudioSourceTaskCanceller.Token);
                }
            }
        }

        public static void PlayClip(AudioClip audioClip, float startTime, float endTime, bool loop = false)
        {
            int startSample = AudioExtension.GetTimeSample(audioClip, startTime);
            int endSample = AudioExtension.GetTimeSample(audioClip, endTime);
            PlayClip(audioClip, startSample, endSample, loop);
        }

        public static void PlayClip(AudioClip audioClip, int startSample, int endSample, bool loop = false)
        {
            StopAllClips();

            var unityEditorAssembly = typeof(AudioImporter).Assembly;

            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtilClass.GetMethod(PLAY_CLIP_METHOD_NAME,
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] {typeof(AudioClip), typeof(int), typeof(bool)},
                null);

            var parameters = new object[] {audioClip, startSample, false};
            if (method != null)
            {
                method.Invoke(null, parameters);
                PlaybackIndicator.Start(loop);
                CurrentPlayingClip = audioClip;
            }

            int sampleLength = audioClip.samples - startSample - endSample;
            var lengthInMs = (int) Math.Round(sampleLength / (double) audioClip.frequency * AudioConstant.SECOND_IN_MILLISECONDS, MidpointRounding.AwayFromZero);
            currentPlayingTaskCanceller ??= new CancellationTokenSource();
            if (loop)
            {
                App.Task.DelayInvoke(lengthInMs, Replay, currentPlayingTaskCanceller.Token);
            }
            else
            {
                App.Task.DelayInvoke(lengthInMs, StopAllPreviewClipsAndCancelTask, currentPlayingTaskCanceller.Token);
            }

            void Replay()
            {
                if (method != null && currentPlayingTaskCanceller != null)
                {
                    StopAllPreviewClips();
                    method.Invoke(null, parameters);
                    App.Task.DelayInvoke(lengthInMs, Replay, currentPlayingTaskCanceller.Token);
                }
            }
        }

        public static void StopAllClips()
        {
            StopAllPreviewClipsAndCancelTask();
            DestroyPreviewAudioSourceAndCancelTask();
        }

        private static void DestroyPreviewAudioSourceAndCancelTask()
        {
            if (currentEditorAudioSource)
            {
                CancelTask(ref currentAudioSourceTaskCanceller);

                currentEditorAudioSource.Stop();
                UnityEngine.Object.DestroyImmediate(currentEditorAudioSource.gameObject);
                PlaybackIndicator.End();
                currentEditorAudioSource = null;
                CurrentPlayingClip = null;
            }
        }

        private static void StopAllPreviewClipsAndCancelTask()
        {
            CancelTask(ref currentPlayingTaskCanceller);
            StopAllPreviewClips();
            PlaybackIndicator.End();
            CurrentPlayingClip = null;
        }

        private static void StopAllPreviewClips()
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;

            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtilClass.GetMethod(STOP_CLIP_METHOD_NAME,
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null);

            method?.Invoke(null, new object[] { });
        }

        public static void AddPlaybackIndicatorListener(Action action)
        {
            RemovePlaybackIndicatorListener(action);
            PlaybackIndicator.OnUpdate += action;
            PlaybackIndicator.OnEnd += action;
        }

        public static void RemovePlaybackIndicatorListener(Action action)
        {
            PlaybackIndicator.OnUpdate -= action;
            PlaybackIndicator.OnEnd -= action;
        }

        private static void CancelTask(ref CancellationTokenSource cancellation)
        {
            if (cancellation != null && cancellation.Token.CanBeCanceled)
            {
                cancellation.Cancel();
                cancellation?.Dispose();
                cancellation = null;
            }
        }
    }
}
#endif