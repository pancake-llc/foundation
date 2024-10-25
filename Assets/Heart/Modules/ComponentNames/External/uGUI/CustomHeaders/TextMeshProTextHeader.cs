using TMPro;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class TextMeshProTextHeader : CustomHeader<TMP_Text>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;

		public override Suffix GetSuffix(TMP_Text target)
		{
			if(target.text is not { Length: > 0} text)
			{
				return Suffix.Default;
			}

			if(text.Length < 13)
			{
				return "\"" + text + "\"";
			}

			return "\"" + text.Substring(0, 10) + "...\"";
		}
	}
}