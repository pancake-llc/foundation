using TMPro;
using UnityEngine;

namespace Pancake.Game.UI
{
    public class ShopItemReward : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textValue;

        public void Setup(string value) { textValue.text = value; }
    }
}