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
        LargeBanner = 1,        // 320x100
        MediumRectangle = 2,    // 300x250
        FullBanner = 3,         // 468x60
        Leaderboard = 4,        // 728x90
        SmartBanner = 5         // width x 32|50|90
    }
    
    public enum EAdNetwork
    {
        None = 0,
        Admob = 1,
        Applovin = 2
    }
    
    public enum EAutoLoadingAd
    {
        None = 0,
        All = 1,
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