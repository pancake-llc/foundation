using UnityEngine;
using UnityEngine.EventSystems;

namespace Pancake.MobileInput
{
    public class BlockInputOnPointerDown : MonoBehaviour, IPointerDownHandler
    {
        //[SerializeField] private BoolVariable statusTouchOnLockedArea;

        public void OnPointerDown(PointerEventData eventData)
        {
            //statusTouchOnLockedArea.Value = true;
        }
    }
}