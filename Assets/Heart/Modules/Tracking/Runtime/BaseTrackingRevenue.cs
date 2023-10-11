namespace Pancake.Tracking
{
    public abstract class BaseTrackingRevenue
    {
        public abstract void TrackRevenue(double value, string network, string unitId, string format, string placement, string clientType);
    }
}