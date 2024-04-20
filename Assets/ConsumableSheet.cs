using Pancake.BakingSheet;

namespace Heart
{
    public class ConsumableSheet : Sheet<ConsumableSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }
            public string Price { get; private set; }
        }
    }
}