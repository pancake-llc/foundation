using System;

namespace  Pancake.SelectiveProfiling
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class AlwaysProfile : Attribute
	{
	}
}