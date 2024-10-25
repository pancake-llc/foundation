using TMPro;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class TextMeshProDropdownHeader : CustomHeader<TMP_Dropdown>
	{
		private const string TextMeshProSuffix = " - TextMeshPro";

		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;

		public override Name GetName(TMP_Dropdown target)
		{
			if(Name.EndsWith(TextMeshProSuffix))
			{
				return Name.Substring(0, Name.Length - TextMeshProSuffix.Length);
			}

			return Name.Default;
		}

		public override Suffix GetSuffix(TMP_Dropdown target)
		{
			if(target.value >= 0
				&& target.value < target.options.Count
				&& target.options[target.value].text is { Length: > 0 } text)
			{
				return text;
			}

			return target.value.ToString();
		}

		public override Tooltip GetTooltip(TMP_Dropdown target) => "TextMesh Pro dropdown.\n\nPresents a list of options when clicked, of which one can be chosen.";
	}
}