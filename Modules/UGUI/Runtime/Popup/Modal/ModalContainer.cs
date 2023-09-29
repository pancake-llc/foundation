using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectMask2D))]
    public class ModalContainer : GameComponent
    {
        public static List<ModalContainer> Instances { get; } = new List<ModalContainer>();
        private CanvasGroup _canvasGroup;
        public bool Interactable { get => _canvasGroup.interactable; set => _canvasGroup.interactable = value; }
        /// <summary>
        ///     True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }
    }
}