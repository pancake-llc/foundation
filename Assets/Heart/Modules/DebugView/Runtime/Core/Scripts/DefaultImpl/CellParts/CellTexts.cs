using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class CellTexts : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Text subText;

        public string Text
        {
            get => text.text;
            set
            {
                if (text.text == value) return;

                text.text = value;
                RefreshTransform();
            }
        }

        public string SubText
        {
            get => subText.text;
            set
            {
                if (subText.text == value) return;

                subText.text = value;
                RefreshTransform();
            }
        }

        public Color TextColor { get => text.color; set => text.color = value; }

        public Color SubTextColor { get => subText.color; set => subText.color = value; }

        private void Start() { RefreshTransform(); }

        private void RefreshTransform()
        {
            var rectTrans = (RectTransform) transform;
            var hasSubText = !string.IsNullOrEmpty(subText.text);
            var height = hasSubText ? 68 : 42;
            subText.gameObject.SetActive(hasSubText);
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}