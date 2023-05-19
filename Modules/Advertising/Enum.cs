namespace Pancake.Monetization
{
    public enum EBannerPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
    }
    
    public enum EBannerSize
    {
        Banner = 0,             // 320x50
        Adaptive = 5         // full width
    }
    
    public enum EAdNetwork
    {
        Applovin = 0,
        Admob = 1,
    }
    
    public enum EAdapterStatus
    {
        NotInstall = 0,
        Installed = 1,
        Upgrade = 2
    }
    
    public enum EVersionComparisonResult
    {
        Lesser = -1,
        Equal = 0,
        Greater = 1
    }
}