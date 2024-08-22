namespace Pancake.Monetization
{
    public enum EBannerPosition
    {
        Top = 1,
        Bottom = 0,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
    }

    public enum EBannerCollapsiblePosition
    {
        None = 0,
        Top = 2,
        Bottom = 1
    }

    public enum EBannerSize
    {
        Banner = 0, // 320x50
        Adaptive = 5 // full width
    }

    public enum EAdNetwork
    {
        Applovin = 0,
        Admob = 1,
    }
}