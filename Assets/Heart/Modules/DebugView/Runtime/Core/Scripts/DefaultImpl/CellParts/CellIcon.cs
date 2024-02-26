using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class CellIcon : MonoBehaviour
    {
        [SerializeField] private Image image;

        public Sprite Sprite { get => image.sprite; set => image.sprite = value; }

        public Color Color { get => image.color; set => image.color = value; }
    }
}