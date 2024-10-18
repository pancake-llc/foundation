using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    public sealed class HpPotionItem : MonoBehaviour<IPlayerStat, IEventTrigger>, ICollectable
    {
        [SerializeField] private int valueRecovery;

        private IPlayerStat _playerStat;
        private IEventTrigger _onCollected;

        protected override void Init(IPlayerStat firstArgument, IEventTrigger secondArgument)
        {
            _playerStat = firstArgument;
            _onCollected = secondArgument;
        }

        public void Collect()
        {
            _playerStat.Health += valueRecovery;
            _onCollected.Trigger();
        }
    }
}