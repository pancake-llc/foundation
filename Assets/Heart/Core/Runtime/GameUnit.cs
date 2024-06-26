using Pancake.Common;
using Pancake.PlayerLoop;
using UnityEngine;

namespace Pancake
{
    public abstract class GameUnit : GameComponent, IUpdate, IFixedUpdate, ILateUpdate
    {
        [Header("base"), SerializeField] private EGameLoopType gameLoop;

        private void OnEnable()
        {
            Register();
            OnEnabled();
        }

        private void OnDisable()
        {
            UnRegister();
            OnDisabled();
        }

        protected virtual void Register()
        {
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.Update)) GameLoop.Register(this, OnUpdate, PlayerLoopTiming.PreUpdate);
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.LateUpdate)) GameLoop.Register(this, OnLateUpdate, PlayerLoopTiming.PreLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.FixedUpdate)) GameLoop.Register(this, OnFixedUpdate, PlayerLoopTiming.PreFixedUpdate);
        }

        protected virtual void UnRegister()
        {
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.Update)) GameLoop.Unregister(this, PlayerLoopTiming.PreUpdate);
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.LateUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, EGameLoopType.FixedUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreFixedUpdate);
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
    }
}