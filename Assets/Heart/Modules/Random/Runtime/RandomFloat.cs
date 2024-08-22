using System;
using Alchemy.Inspector;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    [DisableAlchemyEditor]
    public class RandomFloat : RandomValue<float>
    {
        public override float Value() { return useConstant ? min : Random.Range(min, max); }
    }
}