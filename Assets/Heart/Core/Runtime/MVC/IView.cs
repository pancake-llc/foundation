using Pancake.Common;

namespace Pancake
{
    public interface IView : IGameObjectSource
    {
        bool Interactable { get; set; }
        bool Visible { get; set; }
        void SetPresenter(IPresenter presenter);
    }
}