using Pancake.Common;
using Pancake.IAP;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public class GemItemVisual : MonoBehaviour
    {
        [SerializeField] private Button buttonPurchase;
        [SerializeField] private Image imageIcon;
        [SerializeField] private GameObject extraObject;
        [SerializeField] private TextMeshProUGUI textExtra;
        [SerializeField] private TextMeshProUGUI textValue;
        [SerializeField] private TextMeshProUGUI textCost;
        [SerializeField] private bool controlPositonTextValue;

        [SerializeField, ShowIf(nameof(controlPositonTextValue)), Indent]
        private float postionWhenNotExtra;

        [SerializeField, ShowIf(nameof(controlPositonTextValue)), Indent]
        private float postionWhenExtra;

        private IAPDataVariable _iap;

        public void Setup(GemPackData data)
        {
            _iap = data.IAPData;
            buttonPurchase.onClick.AddListener(OnButtonPurchasePressed);
            imageIcon.sprite = data.Icon;
            textValue.text = $"{data.Amount}";
            bool isFirstPurchase = UserData.GetFirstPurchase();
            if (!isFirstPurchase)
            {
                extraObject.SetActive(true);
                textExtra.text = $"Extra +{data.Amount}";
                if (controlPositonTextValue) textValue.transform.SetLocalPositionY(postionWhenExtra);
            }
            else
            {
                extraObject.SetActive(false);
                if (controlPositonTextValue) textValue.transform.SetLocalPositionY(postionWhenNotExtra);
            }

#if UNITY_EDITOR
            textCost.text = data.EditorPrice;
#else
            textCost.text = data.IAPData.localizedPrice;
#endif
        }

        private void OnButtonPurchasePressed() { _iap.OnPurchaseCompleted(OnPurchaseCompleted).Purchase(); }

        private void OnPurchaseCompleted()
        {
            // todo
            Debug.Log($"IAP: pruchase {_iap.id} success");
        }
    }
}