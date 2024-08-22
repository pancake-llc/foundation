namespace Sisus.Init.Internal
{
	/// <summary>
	/// Interface implemented by Any{T} and AnyGeneric{T}.
	/// <para>
	/// Because Odin inspector does not support custom property drawers
	/// targeting open generic types, this non-generic interface
	/// was needed to get Any{T} fields to be drawn using the right
	/// drawer inside OdinEditor.
	/// </para>
	/// <para>
	/// It's also useful for more easily checking if the type of a field
	/// if Any{T} or AnyGeneric{T} in Inspector code without having to
	/// extract the generic type definition from the type.
	/// </para>
	/// </summary>
	public interface IAny { }
}