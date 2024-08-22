using System.Runtime.CompilerServices;

namespace Pancake.StatModifier
{
    public class AddModifier : IModifier
    {
        private readonly float _baseValue;

        public AddModifier(float value) { _baseValue = value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calculate(float value) => _baseValue + value;
    }

    public class MultiplyModifier : IModifier
    {
        private readonly float _baseValue;

        public MultiplyModifier(float value) { _baseValue = value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Calculate(float value) => _baseValue * value;
    }
}