namespace Sisus.Init
{
	/// <summary>
	/// Specifies the order options for the <see cref="InitOrderAttribute"/>.
	/// </summary>
	public enum Order
	{
		VeryEarly = -500,
		Early = -100,
		Default = 0,
		Late = 100,
		VeryLate = 500
	}
}