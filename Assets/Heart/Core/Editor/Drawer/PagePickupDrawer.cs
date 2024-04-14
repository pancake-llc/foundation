using Alchemy.Editor;
using Pancake;
using Pancake.UI;

namespace PancakeEditor
{
    [CustomAttributeDrawer(typeof(PagePickupAttribute))]
    public class PagePickupDrawer : NamePickupDrawer<Page>
    {
        protected override string NameClass => "Pancake.UI.Page";
        protected override string NameOfT => "Page`1";
    }
}