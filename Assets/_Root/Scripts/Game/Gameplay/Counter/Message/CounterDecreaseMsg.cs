using Pancake.Elm;

namespace Pancake.Game
{
    public class CounterDecreaseMsg : IMessenger<ECounterType>
    {
        public ECounterType GetMessage()
        {
            return ECounterType.Decrease;
        }
    }
}