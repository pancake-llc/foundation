using System;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    public class RandomFloat : RandomValue<float>
    {
        public override float Value() { return useConstant ? min : Random.Range(min, max); }
    }
}