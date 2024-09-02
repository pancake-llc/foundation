using Pancake.Game.Interfaces;
using Sisus.Init;

namespace Pancake.Game
{
    public sealed class Collectable : MonoBehaviour<IEventTrigger>, ICollectable
    {
        /// <summary>
        /// Event invoked when the object is collected.
        /// </summary>
        private IEventTrigger _onCollected;

        protected override void Init(IEventTrigger arg) { _onCollected = arg; }

        public void Collect()
        {
            _onCollected.Trigger();
            gameObject.SetActive(false);
        }
    }
}