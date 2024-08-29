namespace Pancake.Game.Interfaces
{
    /// <summary>
    /// Represents an object that holds settings data for the player.
    /// </summary>
    public interface IPlayerStat
    {
        float Damage { get; set; }
        float MoveSpeed { get; }
    }
}