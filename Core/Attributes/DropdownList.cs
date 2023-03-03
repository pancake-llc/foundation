using System.Collections.Generic;

namespace Pancake.Attribute
{
    public class DropdownList<T> : List<DropdownItem<T>>
    {
        public void Add(string text, T value) { Add(new DropdownItem<T> {Text = text, Value = value,}); }
    }

    public interface IDropdownItem
    {
        string Text { get; }
        object Value { get; }
    }

    public struct DropdownItem : IDropdownItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
    }

    public struct DropdownItem<T> : IDropdownItem
    {
        public string Text;
        public T Value;

        string IDropdownItem.Text => Text;
        object IDropdownItem.Value => Value;
    }
}