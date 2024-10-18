using System;
using Pancake.Game.Interfaces;
using Pancake.StatModifier;
using UnityEngine;

namespace Pancake.Game
{
    [CreateAssetMenu(menuName = "Pancake/Game/Player Stat")]
    public class PlayerStat : ScriptableObject, IPlayerStat
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private BaseStat baseHeathStat;

        [NonSerialized] private Stat _maxHeathStat;
        [NonSerialized] private int _currentHeath;
        [NonSerialized] private StatMediator _maxHeathMediator;

        public float MoveSpeed => moveSpeed;
        private Stat MaxHeathStat => _maxHeathStat ??= new Stat(MaxHeathMediator, baseHeathStat);
        private StatMediator MaxHeathMediator => _maxHeathMediator ??= new StatMediator();

        public void IncreaseMaxHeath(int value)
        {
            var statModifier = new StatModifier.StatModifier(baseHeathStat.statType, new AddModifier(value));
            MaxHeathMediator.AddModifier(statModifier);
        }

        public int MaxHealth => (int) MaxHeathStat.Value;

        public int Health { get => _currentHeath; set => _currentHeath = value; }
    }
}