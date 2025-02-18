using Pancake.Elm;

namespace Pancake.Game
{
    public struct CounterIncreaseMsg : IMessenger<ECounterType>
    {
        public ECounterType GetMessage() { return ECounterType.Increase; }
    }
}