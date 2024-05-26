using UnityEngine;

namespace Pancake.UI
{
    public abstract class Modal : UIContext
    {
        public CanvasGroup BackDrop { get; internal set; }
    }
}