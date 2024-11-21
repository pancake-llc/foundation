using Pancake.Common;

namespace Pancake
{
    public interface IModel<T>
    {
        ObservableList<T> Datas { get; }
        void Add(T data);
    }

    public abstract class BaseModel<T> : IModel<T>
    {
        public ObservableList<T> Datas { get; } = new();

        public void Add(T data) { Datas.Add(data); }
    }
}