using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public class CreditView : View
    {
        [SerializeField] private List<CreditElement> elements;
        [SerializeField] private Dictionary<CreditElementType, GameObject> _prefabContainer = new();
        [SerializeField] private Transform container;
        [SerializeField] private Button buttonClose;

        protected override UniTask Initialize()
        {
            foreach (var element in elements)
            {
                var prefab = _prefabContainer[element.type];
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