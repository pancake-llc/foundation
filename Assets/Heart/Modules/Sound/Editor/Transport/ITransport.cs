using Pancake.Sound;

namespace PancakeEditor.Sound
{
	public interface ITransport : IClip
	{
		float Delay { get; }
		float FadeIn { get; }
		float FadeOut { get; }
		float[] PlaybackValues { get; }
		float[] FadingValues { get; }
		void SetValue(float value, ETransportType transportType);
		void Update();
	}

	public interface IClip
	{
		float StartPosition { get; }
		float EndPosition { get; }
		float FullLength { get; }
	}
}
