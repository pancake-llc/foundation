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

        public SerializedTransport(SerializedProperty clipProp, float fullLength)
            : base(fullLength)
        {
            _delayProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.Delay);
            startPosProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.StartPosition);
            _endPosProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.EndPosition);
            _fadeInProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.FadeIn);
            _fadeOutProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.FadeOut);
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