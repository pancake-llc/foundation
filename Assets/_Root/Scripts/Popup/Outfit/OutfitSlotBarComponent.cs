using Pancake.Apex;

namespace Pancake.SceneFlow
{
    using Pancake;
    using UnityEngine;

    public class OutfitSlotBarComponent : GameComponent
    {
        [SerializeField, Array] private OutfitSlotElement[] elements;

        public void Setup(OutfitUnitVariable[] datas)
        {
            for (var i = 0; i < datas.Length; i++)
            {
                var element = elements[i];
                element.Init(datas[i]);
                element.gameObject.SetActive(true);
            }

            if (datas.Length < elements.Length)
            {
                for (int i = datas.Length; i < elements.Length; i++)
                {
                    elements[i].gameObject.SetActive(false);
                }
            }
        }
    }
}