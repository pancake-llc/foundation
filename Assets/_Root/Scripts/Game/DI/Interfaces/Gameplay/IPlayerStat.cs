namespace Pancake.Game.Interfaces
{
    /// <summary>
    /// Represents an object that holds settings data for the player.
    /// </summary>
    public interface IPlayerStat
    {
        float MoveSpeed { get; }
        int MaxHealth { get; }
        int Health { get; set; }
        void IncreaseMaxHeath(int value);
    }
}