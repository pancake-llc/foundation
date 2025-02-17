namespace Pancake.Elm
{
    public interface IRenderer<TModel, TMessage> where TModel : struct where TMessage : struct
    {
        void Init(Dispatcher<TMessage> dispatcher);
        void Render(TModel model);
    }
}