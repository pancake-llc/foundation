using System;
using Alchemy.Inspector;
using Random = UnityEngine.Random;

namespace Pancake
{
    [Serializable]
    [DisableAlchemyEditor]
    public class RandomInt : RandomValue<int>
    {
        public override int Value() { return useConstant ? min : Random.Range(min, max); }
    }
}