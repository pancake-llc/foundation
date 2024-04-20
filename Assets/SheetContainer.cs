using Pancake.BakingSheet;

namespace Heart
{
    public class SheetContainer : SheetContainerBase
    {
        // property name matches with corresponding sheet name
        // for .xlsx or google sheet, **property name matches with the name of sheet tab in workbook**
        // for .csv or .json, **property name matches with the name of file**
        public ConsumableSheet foundation { get; private set; }
    }
}