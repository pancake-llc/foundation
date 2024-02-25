using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class CollectionButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text text;

        public bool Interactable { get => button.interactable; set => button.interactable = value; }

        public string Text { get => text.text; set => text.text = value; }

        public Color TextColor { get => text.color; set => text.color = value; }

        public Button.ButtonClickedEvent Clicked => button.onClick;
    }
}