using System;

namespace Pancake
{
    public interface IModel<TData>
    {
        event Action OnDataChanged;
        void Initialize();
        TData GetData();
        void SetData(TData data);
    }

    public abstract class BaseModel<TData> : IModel<TData>
    {
        protected TData data;
        public event Action OnDataChanged;

        public virtual void Initialize() { }

        public virtual TData GetData() => data;

        public virtual void SetData(TData newData)
        {
            data = newData;
            NotifyDataChanged();
        }

        protected void NotifyDataChanged() { OnDataChanged?.Invoke(); }
    }
}