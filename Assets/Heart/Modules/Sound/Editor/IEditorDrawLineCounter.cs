namespace PancakeEditor.Sound
{
	public interface IEditorDrawLineCounter
	{
		float SingleLineSpace { get; }
		int DrawLineCount { get; set; }
		float Offset { get; set; }
	}
}