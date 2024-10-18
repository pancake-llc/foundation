using System.Collections.Generic;
using Pancake.ExTag;
using Pancake.PlayerLoop;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.AI
{
    public abstract class Sensor : GameComponent, IFixedUpdate
    {
        [InfoBox("Raycast after raycastRate frame\nraycastRate = 1 => Raycast after every frame\nraycastRate = 2 => Raycast every 2 frames")]
        [SerializeField, Range(1, 8)]
        protected int raycastRate = 1;

        [SerializeField] protected LayerMask layer;
        [SerializeField] private bool detectOnStart = true;
        [SerializeField] protected bool newTagSystem = true;
        public List<StringConstant> tags = new();

        protected bool isPlaying;
        protected int frames;

        private void OnEnable()
        {
            GameLoop.Register(this, OnFixedUpdate, PlayerLoopTiming.PreFixedUpdate);
            OnEnabled();
        }

        private void OnDisable()
        {
            GameLoop.Unregister(this, PlayerLoopTiming.PreFixedUpdate);
            OnDisabled();
        }

        protected virtual void Start()
        {
            if (detectOnStart) Pulse();
        }

        protected virtual void OnEnabled() { }

        protected virtual void OnDisabled() { }

        protected abstract void Procedure();

        public abstract void Pulse();

        public abstract Transform GetClosestTarget(StringConstant tag);

        public abstract Transform GetClosestTarget();

        public virtual void Stop() { isPlaying = false; }

        public void OnFixedUpdate()
        {
            if (!isPlaying) return;
            frames++;
            if (frames % raycastRate != 0) return;
            frames = 0;
            Procedure();
        }

        /// <summary>
        /// The process of checking whether to use Tags to filter out colliding objects
        ///
        /// If <see cref="Sensor.tags"/> empty or tag of <paramref name="hit"/> have in <see cref="Sensor.tags"/> return true else return false
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        protected bool TagVerify(Component hit)
        {
            if (tags.Count <= 0) return true;

            var check = false;
            foreach (var t in tags)
            {
                if (newTagSystem)
                {
                    if (!hit.gameObject.HasTag(t.Value)) continue;
                }
                else
                {
                    if (!hit.CompareTag(t.Value)) continue;
                }

                check = true;
                break;
            }

            return check;
        }
    }
}