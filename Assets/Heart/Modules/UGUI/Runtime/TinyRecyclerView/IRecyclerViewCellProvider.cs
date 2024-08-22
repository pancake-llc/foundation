using UnityEngine;

namespace Pancake.UI
{
    public interface IRecyclerViewCellProvider
    {
        GameObject GetCell(int dataIndex);

        void ReleaseCell(GameObject obj);
    }
}