using UnityEngine;

namespace Pancake
{
    public abstract class Entity : MonoBehaviour
    {
        protected virtual void Awake() { Cabin.Register(this); }
    }
}