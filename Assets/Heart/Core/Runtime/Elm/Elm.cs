using System;

namespace Pancake.Elm
{
    public class Elm<TModel, TMessage> where TModel : struct where TMessage : struct
    {
        private readonly IUpdater<TModel, TMessage> _updater;
        private readonly IRenderer<TModel, TMessage> _renderer;
        private readonly Func<TModel, Sub<IMessenger<TMessage>>> _subscription;
        private readonly Dispatcher<TMessage> _dispatcher;
        private TModel _model;
        private readonly Sub<IMessenger<TMessage>> _currentSubscription;

        public Elm(Func<(TModel, Cmd<TMessage>)> init, IUpdater<TModel, TMessage> updater, IRenderer<TModel, TMessage> renderer)
            : this(init, updater, renderer, _ => Sub<IMessenger<TMessage>>.None)
        {
        }

        public Elm(
            Func<(TModel, Cmd<TMessage>)> init,
            IUpdater<TModel, TMessage> updater,
            IRenderer<TModel, TMessage> renderer,
            Func<TModel, Sub<IMessenger<TMessage>>> subscription)
        {
            _dispatcher = Dispath;
            _updater = updater;
            _renderer = renderer;
            _subscription = subscription;
            var (model, cmd) = init.Invoke();
            _model = model;
            cmd.Execute(_dispatcher);
            _renderer.Init(_dispatcher);
            _renderer.Render(_model);
            _currentSubscription = _subscription(_model);
            _currentSubscription.OnWatch += Dispath;
        }

        private void Dispath(IMessenger<TMessage> msg)
        {
            var (model, cmd) = _updater.Update(msg, _model);
            if (!Equals(_model, model))
            {
                _renderer.Render(model);
                _model = model;
            }

            cmd.Execute(_dispatcher);
            UpdateSubscription();
        }

        private void UpdateSubscription() { _currentSubscription.UpdateEffect(_subscription(_model).Effect); }
    }
}