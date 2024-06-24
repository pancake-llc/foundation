using Pancake.Sound;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public class SerializedTransport : Transport
    {
        private readonly SerializedProperty _delayProp;
        public readonly SerializedProperty startPosProp;
        private readonly SerializedProperty _endPosProp;
        private readonly SerializedProperty _fadeInProp;
        private readonly SerializedProperty _fadeOutProp;

        public SerializedTransport(
            SerializedProperty startPosProp,
            SerializedProperty endPosProp,
            SerializedProperty fadeInProp,
            SerializedProperty fadeOutProp,
            SerializedProperty delayProp,
            float fullLength)
            : base(fullLength)
        {
            _delayProp = delayProp;
            this.startPosProp = startPosProp;
            _endPosProp = endPosProp;
            _fadeInProp = fadeInProp;
            _fadeOutProp = fadeOutProp;
        }

        public override float Delay => _delayProp.floatValue;
        public override float StartPosition => startPosProp.floatValue;
        public override float EndPosition => _endPosProp.floatValue;
        public override float FadeIn => _fadeInProp.floatValue;
        public override float FadeOut => _fadeOutProp.floatValue;

        public override void SetValue(float newValue, ETransportType transportType)
        {
            base.SetValue(newValue, transportType);

            switch (transportType)
            {
                case ETransportType.Start:
                    startPosProp.floatValue = PlaybackValues[0];
                    break;
                case ETransportType.End:
                    _endPosProp.floatValue = PlaybackValues[1];
                    break;
                case ETransportType.Delay:
                    _delayProp.floatValue = PlaybackValues[2];
                    break;
                case ETransportType.FadeIn:
                    _fadeInProp.floatValue = FadingValues[0];
                    break;
                case ETransportType.FadeOut:
                    _fadeOutProp.floatValue = FadingValues[1];
                    break;
            }

            startPosProp.serializedObject.ApplyModifiedProperties();
        }
    }
}