using System;
using Pancake.Apex;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    [UseDefaultEditor]
    public class RandomInt : RandomValue<int>
    {
        public override int Value() { return useConstant ? min : Random.Range(min, max); }
    }
}