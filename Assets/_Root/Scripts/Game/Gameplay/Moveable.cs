using Pancake.Game.Interfaces;
using Pancake.PlayerLoop;
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

        private void OnEnable() { GameLoop.Register(this, OnUpdate, PlayerLoopTiming.PreUpdate); }

        private void OnDisable() { GameLoop.Unregister(this, PlayerLoopTiming.PreUpdate); }

        private void OnUpdate()
        {
            if (Direction == Vector2.zero) return;

            var result = _timeProvider.DeltaTime * _playerStat.MoveSpeed * Direction;
            var pos = transform.position + new Vector3(result.x, 0, result.y);
            if (transform.position == pos) return;
            transform.position = pos;
            PositionChanged?.Invoke();
        }

        protected override void Init(IPlayerStat playerStat, ITimeProvider timeProvider)
        {
            _playerStat = playerStat;
            _timeProvider = timeProvider;
        }

        public event UnityAction PositionChanged;
        public Vector2 Position => new(transform.position.x, transform.position.z);
    }
}