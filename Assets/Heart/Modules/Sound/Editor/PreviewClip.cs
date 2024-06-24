namespace PancakeEditor.Sound
{
	public struct PreviewClip : IClip
	{
		public float StartPosition { get; set; }
		public float EndPosition { get; set; }
		public float Length { get; set; }

		public PreviewClip(IClip clip)
		{
			StartPosition = clip.StartPosition;
			EndPosition = clip.EndPosition;
			Length = clip.Length;
		}

		public PreviewClip(float length) : this()
		{
			Length = length;
		}
	} 
}