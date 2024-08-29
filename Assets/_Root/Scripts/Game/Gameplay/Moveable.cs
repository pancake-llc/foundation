using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine.Events;

namespace Pancake.Game
{
    using UnityEngine;

    public class Moveable : MonoBehaviour<IPlayerStat, ITimeProvider>, ITrackable
    {
        private IPlayerStat _playerStat;
        private ITimeProvider _timeProvider;
        
        public Vector2 Direction { get; set; }
        
        protected override void Init(IPlayerStat playerStat, ITimeProvider timeProvider)
        {
            _playerStat = playerStat;
            _timeProvider = timeProvider;
        }

        public event UnityAction PositionChanged;
        public Vector2 Position => transform.position;
    }
}