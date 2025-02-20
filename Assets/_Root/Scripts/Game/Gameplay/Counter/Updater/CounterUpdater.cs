using Pancake.Elm;

namespace Pancake.Game
{
    public class CounterUpdater : IUpdater<CounterModel, ECounterType>
    {
        public (CounterModel, Cmd<ECounterType>) Update(IMessenger<ECounterType> msg, CounterModel model)
        {
            return msg switch
            {
                CounterIncreaseMsg => (new CounterModel {number = model.number + 1}, Cmd<ECounterType>.None),
                CounterDecreaseMsg => (new CounterModel {number = model.number - 1}, Cmd<ECounterType>.None),
                _ => (new CounterModel {number = model.number}, Cmd<ECounterType>.None)
            };
        }
    }
}