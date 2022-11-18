using UnityEngine;

namespace Pancake.Feedback
{
    public abstract class FeedbackInfoCollector : MonoBehaviour
    {
        [SerializeField] protected string title;
        [SerializeField] protected int sortOrder;

        protected PopupFeedback popupFeedback;

        protected virtual void Awake()
        {
            popupFeedback = GetComponentInParent<PopupFeedback>();
            popupFeedback.CollectDataAction -= Collect;
            popupFeedback.CollectDataAction += Collect;
        }

        public abstract void Collect();
    }
}