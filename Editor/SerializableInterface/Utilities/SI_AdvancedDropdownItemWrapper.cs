using UnityEditor.IMGUI.Controls;

namespace Pancake.Editor
{
    internal sealed class SI_AdvancedDropdownItemWrapper : AdvancedDropdownItem
    {
        /// <inheritdoc />
        public SI_AdvancedDropdownItemWrapper(string name)
            : base(name)
        {
        }

        public new SI_AdvancedDropdownItemWrapper AddChild(AdvancedDropdownItem child)
        {
            base.AddChild(child);
            return this;
        }

        public new SI_AdvancedDropdownItemWrapper AddSeparator()
        {
            base.AddSeparator();
            return this;
        }
    }
}