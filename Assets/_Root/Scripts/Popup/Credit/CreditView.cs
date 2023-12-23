using System.Collections.Generic;
using Pancake.Apex;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class CreditView : View
    {
        [SerializeField, Array] private List<CreditElement> elements;
        [SerializeField] private CreditTypeElementDictionary prefabContainer;
        [SerializeField] private Transform container;
        [SerializeField] private Button buttonClose;
        

        protected override UniTask Initialize()
        {
            foreach (CreditElement element in elements)
            {
                var prefab = prefabContainer[element.type];
                var instance = Instantiate(prefab, container);
                switch (element.type)
                {
                    case CreditElementType.Space:
                        instance.GetComponent<LayoutElement>().preferredHeight = element.space;
                        break;
                    case CreditElementType.Image:
                        instance.GetComponent<Image>().sprite = element.sprite;
                        break;
                    case CreditElementType.Message:
                        instance.GetComponent<TextMeshProUGUI>().text = element.message;
                        break;
                    case CreditElementType.Unit:
                        instance.GetComponent<CreditUnitMember>().Setup(element.titleUnit, element.unitMembers);
                        break;
                }
            }

            buttonClose.onClick.AddListener(OnButtonClosePressed);

            return UniTask.CompletedTask;
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}