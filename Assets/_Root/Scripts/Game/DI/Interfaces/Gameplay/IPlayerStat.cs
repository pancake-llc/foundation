using System;

namespace Pancake.Game.Interfaces
{
    /// <summary>
    /// Represents an object that holds settings data for the player.
    /// </summary>
    public interface IPlayerStat
    {
        event Action<int> OnHealthChanged;
        float MoveSpeed { get; }
        int MaxHealth { get; }
        int Health { get; }
        void UpdateHealth(int amount);
        void IncreaseMaxHeath(int value);
    }
}