namespace Pancake.Sensor
{
    /// <summary>
    /// Identify objects that can be detected by the sensor
    /// </summary>
    public interface ILocateable
    {
        bool IsActive { get; }
        float Priority { get; }
    }
}