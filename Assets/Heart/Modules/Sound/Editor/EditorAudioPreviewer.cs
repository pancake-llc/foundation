using System.Reflection;
using LitMotion;
using Pancake.Sound;
using PancakeEditor.Sound.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Pancake.Sound.AudioSettings;

namespace PancakeEditor.Sound
{
    public class EditorAudioPreviewer : EditorUpdateHelper
    {
        public const string VOLUME_PARAMETER_NAME = "PreviewVolume";
        public const string DEFAULT_SNAPSHOT_NAME = "Snapshot";

        private AudioMixer _mixer;
        private AudioMixerGroup _mixerGroup;
        private AudioMixerSnapshot _snapshot;
        private MethodInfo _method;
        private object[] _parameters;

        private EditorPlayAudioClip.Data _clipData = null;
        private Ease _fadeInEase;
        private Ease _fadeOutEase;

        private float _elapsedTime;
        private float _dbVolume;

        private bool _isInitSuccessfully;

        public EditorAudioPreviewer(AudioMixer mixer)
        {
            _mixer = mixer;
            _snapshot = mixer.FindSnapshot(DEFAULT_SNAPSHOT_NAME);
            var tracks = mixer.FindMatchingGroups(AudioConstant.MASTER_TRACK_NAME);
            _mixerGroup = tracks != null && tracks.Length > 0 ? tracks[0] : null;
            _fadeInEase = AudioSettings.DefaultFadeInEase;
            _fadeOutEase = AudioSettings.DefaultFadeOutEase;
            _isInitSuccessfully = _mixer && _snapshot && _mixerGroup;

            if (_isInitSuccessfully)
            {
                _parameters = new object[] {_mixer, _snapshot, 0f};
            }
            else
            {
                Debug.LogError("EditorAudioPreviewer initializing fail! make sure you have " +
                               $"{AudioConstant.EDITOR_MIXER_NAME}.mixer in Resources/Editor folder, " +
                               $"an AudioMixerGroup named:{AudioConstant.MASTER_TRACK_NAME}, " + $"and a snapshot named:{DEFAULT_SNAPSHOT_NAME}");
            }
        }

        protected override float UpdateInterval => 1 / 30f;

        public void SetData(EditorPlayAudioClip.Data clipData)
        {
            _clipData = clipData;

            if (_isInitSuccessfully && _method == null)
            {
                var reflection = new AudioClassReflectionHelper();
                string methodName = AudioReflection.MethodName.SetValueForVolume.ToString();
                _method = reflection.MixerGroupClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            }

            float startVol = _clipData.FadeIn > 0f ? 0f : 1f;
            SetVolume(startVol);
        }

        public override void Start()
        {
            _elapsedTime = 0f;
            base.Start();
        }

        public override void End()
        {
            SetVolume(1f);
            base.End();
        }

        protected override void Update()
        {
            if (!_isInitSuccessfully)
            {
                return;
            }

            float fadeOutPos = _clipData.Duration - _clipData.FadeOut;
            bool hasFaddeOut = _clipData.FadeOut > 0f;

            _elapsedTime += DeltaTime;
            if (_elapsedTime < _clipData.FadeIn)
            {
                float t = EaseUtility.Evaluate(_elapsedTime / _clipData.FadeIn, _fadeInEase);
                SetVolume(Mathf.Lerp(0f, _clipData.Volume, t));
            }
            else if (hasFaddeOut && _elapsedTime >= fadeOutPos && _elapsedTime < _clipData.Duration)
            {
                float t = EaseUtility.Evaluate((_elapsedTime - fadeOutPos) / _clipData.FadeOut, _fadeOutEase);
                SetVolume(Mathf.Lerp(_clipData.Volume, 0f, t));
            }
            else
            {
                SetVolume(hasFaddeOut && _elapsedTime >= _clipData.Duration ? 0f : _clipData.Volume);
            }

            base.Update();
        }

        private void SetVolume(float vol)
        {
            if (!_isInitSuccessfully)
            {
                return;
            }

            float db = vol.ToDecibel();
            if (db == _dbVolume)
            {
                return;
            }

            _dbVolume = db;
            _parameters[2] = db;
            _method?.Invoke(_mixerGroup, _parameters);
        }
    }
}