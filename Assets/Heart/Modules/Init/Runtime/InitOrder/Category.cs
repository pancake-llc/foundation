namespace Sisus.Init
{
	/// <summary>
	/// Specifies the category options for the <see cref="InitOrderAttribute"/>.
	/// </summary>
	public enum Category
	{
		ServiceInitializer = -30000,
		Initializer = -29000,
		Service = -20000,
		Default = 0,
		Component = 0
	}
}