using Pancake.Game.Interfaces;
using UnityEngine;

namespace Pancake.Game
{
    public class Player : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ICollectable collectable)) collectable.Collect();
        }
    }
}