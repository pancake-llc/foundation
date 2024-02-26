namespace Pancake.UI
{
    public interface ICell
    {
        void Setup(CellModel model);
    }

    public abstract class Cell<T> : UnityEngine.MonoBehaviour, ICell where T : CellModel
    {
        public void Setup(CellModel model)
        {
            var d = (T) model;
            SetModel(d);
        }

        protected abstract void SetModel(T model);
    }

    public abstract class CellModel
    {
    }
}