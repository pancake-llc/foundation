using Pancake.Game.Interfaces;
using UnityEngine;

namespace Pancake.Game
{
    [CreateAssetMenu(menuName = "Pancake/Game/Player Stat")]
    public class PlayerStat : ScriptableObject, IPlayerStat
    {
        [SerializeField] private float moveSpeed;

        public float MoveSpeed => moveSpeed;
    }
}