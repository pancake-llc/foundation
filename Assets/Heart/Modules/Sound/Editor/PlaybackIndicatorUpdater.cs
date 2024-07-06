using System;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public class PlaybackIndicatorUpdater : EditorUpdateHelper
    {
        public const float AUDIO_CLIP_INDICATOR_WIDTH = 2f;

        public event Action OnEnd;
        private Rect _waveformRect;
        private PreviewClip _clip;
        private double _playingStartTime;
        private float _speed = 1f;

        public bool IsPlaying { get; private set; }
        public bool IsLoop { get; private set; }

        public Color Color => new(1f, 1f, 1f, 0.8f);

        protected override float UpdateInterval => 0.02f; // 50 FPS

        public void SetClipInfo(Rect waveformRect, PreviewClip clip, float pitch = 1f)
        {
            _waveformRect = waveformRect;
            _clip = clip;
            _speed = pitch;
        }

        public Rect GetIndicatorPosition()
        {
            if (_clip.FullLength != 0f && _waveformRect != default)
            {
                double currentPlayedLength = (EditorApplication.timeSinceStartup - _playingStartTime) * _speed;
                double currentPos;
                if (IsLoop)
                {
                    float targetPlayLength = _clip.FullLength - _clip.StartPosition - _clip.EndPosition;
                    currentPos = _clip.StartPosition + currentPlayedLength % targetPlayLength;
                }
                else
                {
                    float endTime = _clip.FullLength - _clip.EndPosition;
                    currentPos = _clip.StartPosition + currentPlayedLength;
                    currentPos = Math.Min(currentPos, endTime);
                }

                float x = (float) (_waveformRect.x + (currentPos / _clip.FullLength * _waveformRect.width));
                return new Rect(x, _waveformRect.y, AUDIO_CLIP_INDICATOR_WIDTH, _waveformRect.height);
            }

            return default;
        }

        public Rect GetEndPos()
        {
            float endTime = _clip.FullLength - _clip.EndPosition;
            return new Rect(_waveformRect.x + (endTime / _clip.FullLength * _waveformRect.width), _waveformRect.y, AUDIO_CLIP_INDICATOR_WIDTH, _waveformRect.height);
        }

        public override void Start()
        {
            _playingStartTime = EditorApplication.timeSinceStartup;
            IsPlaying = true;
            IsLoop = false;
            base.Start();
        }

        public void Start(bool loop)
        {
            Start();
            IsLoop = loop;
        }

        public override void End()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                OnEnd?.Invoke();
            }

            _playingStartTime = default;
            base.End();
        }
        
        public override void Dispose()
        {
            OnEnd = null;
            base.Dispose();
        }
    }
}