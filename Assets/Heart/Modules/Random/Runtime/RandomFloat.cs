using System;
using Pancake.Apex;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    [UseDefaultEditor]
    public class RandomFloat : RandomValue<float>
    {
        public override float Value() { return useConstant ? min : Random.Range(min, max); }
    }
}