using Pancake.Elm;

namespace Pancake.Game
{
    public class CounterUpdater : IUpdater<CounterModel, ECounterType>
    {
        public (CounterModel, Cmd<ECounterType>) Update(IMessenger<ECounterType> msg, CounterModel model)
        {
            switch (msg)
            {
                case CounterIncreaseMsg: return (new CounterModel {number = model.number + 1}, Cmd<ECounterType>.none);
                default: return (new CounterModel(), Cmd<ECounterType>.none);
            }
        }
    }
}