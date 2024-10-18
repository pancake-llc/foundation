using Pancake.Game.Interfaces;
using Sisus.Init;

namespace Pancake.Game
{
    using UnityEngine;

    public class PickupHandler : MonoBehaviour<IPlayerStat>
    {
        private IPlayerStat _playerStat;

        public void OnItemCollected()
        {
            Debug.Log("PickupHandler::OnItemCollected");

            Debug.Log("Now Current Player HP:" + _playerStat.Health);
        }

        protected override void Init(IPlayerStat argument) { _playerStat = argument; }
    }
}