namespace Pancake.UI.Editor
{
    public enum ActiveWindow
    {
        Align,
        Distribute,
    }

    public enum AlignTo
    {
        SelectionBounds,
        Parent,
        FirstInHierarchy,
        LastInHierarchy,
        BiggestObject,
        SmallestObject,
    }

    public enum DistributeTo
    {
        SelectionBounds,
        Parent,
    }

    public enum DistanceOption
    {
        Space,
        Pivot,
        LeftBottom,
        Center,
        RightTop,
    }

    public enum SortOrder
    {
        Positional,
        Hierarchical,
    }

    public enum AlignMode
    {
        Top,
        Horizontal,
        Bottom,

        Left,
        Vertical,
        Right,
    }

    public enum AnchorMode
    {
        FollowObject,
        SnapToBorder,
        StayAtCurrentPosition,
    }

    public enum SelectionStatus
    {
        Valid,
        NothingSelected,
        ParentIsNull,
        ParentIsNoRectTransform,
        ContainsNoRectTransform,
        UnequalParents,
    }
}
