using System;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    public class RandomInt : RandomValue<int>
    {
        public override int Value() { return useConstant ? min : Random.Range(min, max); }
    }
}