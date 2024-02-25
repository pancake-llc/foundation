namespace Pancake.DebugView
{
    public sealed class SeparatorCellModel : CellModel
    {
    }

    public sealed class SeparatorCell : Cell<SeparatorCellModel>
    {
        protected override void SetModel(SeparatorCellModel model) { }
    }
}