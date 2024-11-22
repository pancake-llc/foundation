namespace Pancake
{
    public interface IView<TData>
    {
        void Initialize();
        void Cleanup();
        void UpdateView(TData data);
    }
}