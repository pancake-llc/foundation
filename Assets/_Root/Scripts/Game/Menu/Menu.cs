using System;
using Alchemy.Inspector;
using Pancake;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game
{
    [EditorIcon("icon_entry")]
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button buttonSetting;

        [HorizontalLine, SerializeField, PopupPickup] private string settingPopupKey;

        private void Start()
        {
            buttonSetting.onClick.RemoveListener(OnButtonSettingPressed);
            buttonSetting.onClick.AddListener(OnButtonSettingPressed);
        }

        private void OnButtonSettingPressed() { MainUIContainer.In.GetMain<PopupContainer>().Push(settingPopupKey, true); }
    }
}