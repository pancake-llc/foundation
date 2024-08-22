using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(menuName = "Pancake/Game/Shop/Gem Data")]
    public class GemPackData : ScriptableObject
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private int amount;
#if UNITY_EDITOR
        [SerializeField] private string editorPrice;
#endif
        [SerializeField] private IAPDataVariable iapData;

        public Sprite Icon => icon;
        public int Amount => amount;
        public IAPDataVariable IAPData => iapData;
#if UNITY_EDITOR
        public string EditorPrice => editorPrice;
#endif
    }
}