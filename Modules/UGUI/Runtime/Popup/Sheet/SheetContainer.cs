using System.Collections.Generic;
using UnityEngine;

namespace Pancake.UI
{
    public class SheetContainer : GameComponent
    {
        public static List<SheetContainer> Instances { get; } = new List<SheetContainer>();
        private CanvasGroup _canvasGroup;
        public bool Interactable { get => _canvasGroup.interactable; set => _canvasGroup.interactable = value; }
        /// <summary>
        ///     True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }
    }
}