using System;
using System.Collections.Generic;
using Pancake.Sound;
using UnityEngine;
using static Pancake.Sound.AudioExtension;

namespace PancakeEditor.Sound
{
    public class AudioClipEditingHelper : IDisposable
    {
        private float[] _sampleDatas;
        private AudioClip _originalClip;
        private bool _isMono;
        public bool HasEdited { get; private set; }

        public AudioClipEditingHelper(AudioClip originalClip) { _originalClip = originalClip; }

        public bool HasClip => _originalClip != null;
        public bool CanEdit => HasClip && Samples != null;

        private float[] Samples
        {
            get
            {
                if (_sampleDatas == null && HasClip) _sampleDatas = _originalClip.GetSampleData();
                return _sampleDatas;
            }
            set => _sampleDatas = value;
        }

        public AudioClip GetResultClip()
        {
            if (!HasClip) return null;
            if (!HasEdited) return _originalClip;

            return CreateAudioClip(_originalClip.name, Samples, _originalClip.GetAudioClipSetting(_isMono));
        }

        private int GetChannelCount() { return _isMono ? 1 : _originalClip.channels; }

        public void Trim(float startPos, float endPos)
        {
            if (!HasClip) return;

            HasEdited = _originalClip.TryGetSampleData(out _sampleDatas, startPos, endPos);
        }

        public void AddSlient(float time)
        {
            if (!CanEdit) return;

            var slientSampleLength = (int) (time * _originalClip.frequency * GetChannelCount());
            var newSampleDatas = new float[Samples.Length + slientSampleLength];
            Array.Copy(Samples,
                0,
                newSampleDatas,
                slientSampleLength,
                Samples.Length);
            Samples = newSampleDatas;
            HasEdited = true;
        }

        public void AdjustVolume(float volume)
        {
            if (!CanEdit) return;

            for (var i = 0; i < Samples.Length; i++)
            {
                Samples[i] *= volume;
            }

            HasEdited = true;
        }

        public void ConvertToMono(EMonoConversionMode monoMode)
        {
            if (!CanEdit) return;

            var resultSamples = new List<float>();
            if (monoMode == EMonoConversionMode.Downmixing) Downmix();
            else SelectOneChannel();

            Samples = resultSamples.ToArray();
            _isMono = true;
            HasEdited = true;

            void Downmix()
            {
                // Multi-Channel would require addtional weight calculation, we only 
                var sum = 0f;
                for (var i = 0; i < Samples.Length; i++)
                {
                    if (i != 0 && i % _originalClip.channels == 0)
                    {
                        resultSamples.Add(sum / _originalClip.channels);
                        sum = 0f;
                    }

                    sum += Samples[i];
                }
            }

            void SelectOneChannel()
            {
                for (var i = 0; i < Samples.Length; i++)
                {
                    if (i % _originalClip.channels == (int) monoMode - 1) resultSamples.Add(Samples[i]);
                }
            }
        }

        public void Reverse()
        {
            if (!CanEdit) return;

            Array.Reverse(Samples);
            HasEdited = true;
        }

        public void FadeIn(float fadeTime)
        {
            if (!CanEdit) return;

            var fadeSample = (int) Math.Round(fadeTime * _originalClip.frequency * GetChannelCount(), MidpointRounding.AwayFromZero);

            // TODO: Accept more ease type
            var volFactor = 0f;
            float volIncrement = 1f / fadeSample;

            for (var i = 0; i < fadeSample; i++)
            {
                Samples[i] *= volFactor;
                volFactor += volIncrement;
            }

            HasEdited = true;
        }

        public void FadeOut(float fadeTime)
        {
            if (!CanEdit) return;

            var fadeSample = (int) Math.Round(fadeTime * _originalClip.frequency * GetChannelCount(), MidpointRounding.AwayFromZero);
            int startSampleIndex = Samples.Length - fadeSample;

            // TODO: Accept more ease type
            var volFactor = 1f;
            float volIncrement = 1f / fadeSample * -1f;


            for (int i = startSampleIndex; i < Samples.Length; i++)
            {
                Samples[i] *= volFactor;
                volFactor += volIncrement;
            }

            HasEdited = true;
        }

        public void Dispose()
        {
            _sampleDatas = null;
            _originalClip = null;
        }
    }
}