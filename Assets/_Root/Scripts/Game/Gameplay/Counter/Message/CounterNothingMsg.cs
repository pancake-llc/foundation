using Pancake.Elm;

namespace Pancake.Game
{
    public class CounterNothingMsg : IMessenger<ECounterType>
    {
        public ECounterType GetMessage() { return ECounterType.Nothing; }
    }
}