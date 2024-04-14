using Alchemy.Editor;
using Pancake;
using Pancake.UI;

namespace PancakeEditor
{
    [CustomAttributeDrawer(typeof(PopupPickupAttribute))]
    public sealed class PopupPickupDrawer : NamePickupDrawer<Popup>
    {
        protected override string NameClass => "Pancake.UI.Popup";
        protected override string NameOfT => "Popup`1";
    }
}