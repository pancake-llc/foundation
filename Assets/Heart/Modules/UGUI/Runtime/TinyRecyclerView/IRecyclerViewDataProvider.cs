using UnityEngine;

namespace Pancake.UI
{
    public interface IRecyclerViewDataProvider
    {
        void SetupCell(int dataIndex, GameObject cell);
    }
}