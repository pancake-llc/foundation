using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class AutoSizeTMP : MonoBehaviour
    {
        [SerializeField] [TextArea(5, 10)] public string editText;

        public float scaleMax = 1.0f;
        public float scaleMin = 0.0f;

        private TMP_Text _tmpTextCache;

        public TMP_Text TmpText
        {
            get
            {
                if (_tmpTextCache == null) _tmpTextCache = GetComponent<TMP_Text>();
                return _tmpTextCache;
            }
        }

        public string Text
        {
            get => TmpText.text;
            set
            {
                editText = value;
                TmpText.text = value;
                UpdateScale();
            }
        }

        [Conditional("UNITY_EDITOR")]
        public void Awake() { editText = Text; }

        [Conditional("UNITY_EDITOR")]
        public void Update()
        {
            if (editText == Text) return;
            Text = editText;
        }

        private void UpdateScale()
        {
            var preferredWidth = TmpText.preferredWidth;
            var s = M.Clamp(TmpText.rectTransform.sizeDelta.x / preferredWidth, scaleMin, scaleMax);
            TmpText.rectTransform.localScale = new Vector3(s, s, s);
        }
    }
}