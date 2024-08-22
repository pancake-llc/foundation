namespace Pancake.Tracking
{
    using UnityEngine;

    public abstract class ScriptableTracking : ScriptableObject, ITracking
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;

        public abstract void Track();
    }
}