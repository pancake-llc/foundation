namespace Pancake.Game
{
    using UnityEngine;

    public class PickupHandler : MonoBehaviour
    {
        public void OnItemCollected() { Debug.Log("PickupHandler::OnItemCollected"); }
    }
}