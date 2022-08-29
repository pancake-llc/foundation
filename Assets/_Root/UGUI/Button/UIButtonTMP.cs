#if UNITY_EDITOR
using System.Linq;
#endif
using TMPro;
using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class UIButtonTMP : UIButton, IUniTMP
    {
        [SerializeField] private TextMeshProUGUI label;

        #region Implementation of IUniTMP

        public TextMeshProUGUI Label
        {
            get
            {
#if UNITY_EDITOR
                FindChildText();
#endif
                return label;
            }
        }

        #endregion

        #region Overrides of Selectable

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            //use Invoke to call method to avoid warning "Send Message cannot be called during Awake, or OnValidate...
            Invoke(nameof(FindChildText), 0.1f);
        }

        private void CreateChildTextTMP()
        {
            var childText = new GameObject("Label");
            childText.transform.SetParent(transform, false);
            label = childText.AddComponent<TextMeshProUGUI>();
        }

        private void FindChildText()
        {
            if (label != null) return;
            label = GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(_ => _.gameObject != gameObject);
            if (label != null) return;
            CreateChildTextTMP();
        }
#endif

        #endregion
    }
}