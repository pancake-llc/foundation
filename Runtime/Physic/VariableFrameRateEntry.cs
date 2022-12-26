using UnityEngine;

namespace Pancake
{
    public class VariableFrameRateEntry : BaseMono
    {
        [SerializeField] private VariableFrameRatePhysicsSystem.FixedDeltaTimeType type = VariableFrameRatePhysicsSystem.FixedDeltaTimeType.Fixed;
        private void Start() { VariableFrameRatePhysicsSystem.Type = type; }
    }
}