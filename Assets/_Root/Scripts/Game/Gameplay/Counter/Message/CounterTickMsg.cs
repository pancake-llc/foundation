using Pancake.Elm;

namespace Pancake.Game
{
    public class CounterTickMsg : IMessenger<ECounterType>
    {
        public ECounterType GetMessage()
        {
            return ECounterType.Tick;
        }
    }
}