using Alchemy.Editor;
using Pancake;
using Pancake.UI;

namespace PancakeEditor
{
    [CustomAttributeDrawer(typeof(SheetPickupAttribute))]
    public class SheetPickupDrawer : NamePickupDrawer<Sheet>
    {
        protected override string NameClass => "Pancake.UI.Sheet";
        protected override string NameOfT => "Sheet`1";
    }
}