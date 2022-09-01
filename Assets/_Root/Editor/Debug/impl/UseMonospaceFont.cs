namespace Pancake.Debugging.Console
{
    public enum UseMonospaceFont
	{
		Never = 0,

		#if UNITY_2021_2_OR_NEWER
		DetailsViewOnly = 1,
		Always = 2
		#endif
	}
}