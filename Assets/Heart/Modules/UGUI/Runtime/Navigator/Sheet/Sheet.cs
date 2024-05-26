using UnityEngine;

namespace Pancake.UI
{
    public abstract class Sheet : UIContext
    {
        [field: SerializeField] public bool IsRecycle { get; private set; } = true;
    }
}