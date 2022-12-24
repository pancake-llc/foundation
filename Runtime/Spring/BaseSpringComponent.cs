using UnityEngine;

namespace Pancake
{
    [AddComponentMenu("")]
    public class BaseSpringComponent : BaseMono
    {
        [SerializeField, Range(0f, 100f)] protected float damping = 26f;
        [SerializeField, Range(0f, 500f)] protected float stiffness = 169f;
    }
}