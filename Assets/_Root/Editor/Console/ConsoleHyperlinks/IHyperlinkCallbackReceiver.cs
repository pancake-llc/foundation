using System;

namespace Needle
{
	public interface IHyperlinkCallbackReceiver
	{
		bool OnHyperlinkClicked(string path, string line);
	}
	
	public abstract class HyperlinkCallbackReceiver : IHyperlinkCallbackReceiver
	{
		public abstract bool OnHyperlinkClicked(string path, string line);
		
		// ReSharper disable once EmptyConstructor
		// ReSharper disable once PublicConstructorInAbstractClass
		public HyperlinkCallbackReceiver(){}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class HyperlinkCallback : Attribute
	{
		public int Priority;
		public string Href = null;
	}
}