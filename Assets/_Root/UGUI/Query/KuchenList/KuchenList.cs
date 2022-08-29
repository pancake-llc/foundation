using UnityEngine;

namespace Pancake.UIQuery
{
    public class KuchenList : MonoBehaviour
    {
        public IKuchenListMonoBehaviourBridge List { get; set; }

        public void LateUpdate()
        {
            List.UpdateView();
        }

        public void OnDestroy()
        {
            List.DeactivateAll();
        }
    }

    public interface IKuchenListMonoBehaviourBridge
    {
        void DeactivateAll();
        void UpdateView();
    }

    public static class KuchenListInternal
    {
        public static readonly Vector3[] ReuseCorners = new Vector3[4];
    }
}