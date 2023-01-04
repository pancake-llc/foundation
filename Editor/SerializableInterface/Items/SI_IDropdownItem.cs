namespace Pancake.Editor
{
    internal interface SI_IDropdownItem
    {
        internal InterfaceRefMode Mode { get; }
        object GetValue();
    }
}