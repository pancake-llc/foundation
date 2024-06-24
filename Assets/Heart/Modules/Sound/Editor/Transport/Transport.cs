using System;
using Pancake.Sound;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public class Transport : ITransport
    {
        public const int FLOAT_FIELD_DIGITS = 3;

        public virtual float StartPosition => PlaybackValues[0];
        public virtual float EndPosition => PlaybackValues[1];
        public virtual float Delay => PlaybackValues[2];
        public virtual float FadeIn => FadingValues[0];
        public virtual float FadeOut => FadingValues[1];
        public float Length { get; private set; }
        public float[] PlaybackValues { get; private set; }
        public float[] FadingValues { get; private set; }

        public Transport(float length)
        {
            Length = length;
            PlaybackValues = new float[3]; // StartPosition, EndPosition, Delay
            FadingValues = new float[2]; // FadeIn, FadeOut
        }

        public bool HasDifferentPosition => StartPosition != 0f || EndPosition != 0f || (Delay > StartPosition);
        public bool HasFading => FadeIn != 0f || FadeOut != 0f;

        public virtual void SetValue(float newValue, ETransportType transportType)
        {
            switch (transportType)
            {
                case ETransportType.Start:
                    PlaybackValues[0] = ClampAndRound(newValue, transportType);
                    break;
                case ETransportType.End:
                    PlaybackValues[1] = ClampAndRound(newValue, transportType);
                    break;
                case ETransportType.Delay:
                    PlaybackValues[2] = Mathf.Max(newValue, 0f);
                    break;
                case ETransportType.FadeIn:
                    FadingValues[0] = ClampAndRound(newValue, transportType);
                    break;
                case ETransportType.FadeOut:
                    FadingValues[1] = ClampAndRound(newValue, transportType);
                    break;
            }
        }

        public void Update()
        {
            PlaybackValues[0] = StartPosition;
            PlaybackValues[1] = EndPosition;
            PlaybackValues[2] = Delay;
            FadingValues[0] = FadeIn;
            FadingValues[1] = FadeOut;
        }

        private float ClampAndRound(float value, ETransportType transportType)
        {
            float clamped = Mathf.Clamp(value, 0f, GetLengthLimit(transportType));
            return (float) Math.Round(clamped, FLOAT_FIELD_DIGITS, MidpointRounding.AwayFromZero);
        }

        private float GetLengthLimit(ETransportType modifiedType)
        {
            return Length - GetLength(ETransportType.Start, StartPosition) - GetLength(ETransportType.End, EndPosition) - GetLength(ETransportType.FadeIn, FadeIn) -
                   GetLength(ETransportType.FadeOut, FadeOut);

            float GetLength(ETransportType transportType, float value) { return modifiedType != transportType ? value : 0f; }
        }
    }
}