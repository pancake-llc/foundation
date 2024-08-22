using Pancake.Common;
using Pancake.PlayerLoop;
using UnityEngine;

namespace Pancake
{
    public abstract class GameUnit : GameComponent, IUpdate, IFixedUpdate, ILateUpdate
    {
        [SerializeField] private EGameLoopType gameLoop;
        
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
            if (gameLoop.HasFlagUnsafe(EGameLoopType.Update)) GameLoop.Register(this, OnUpdate, PlayerLoopTiming.PreUpdate);
            if (gameLoop.HasFlagUnsafe(EGameLoopType.LateUpdate)) GameLoop.Register(this, OnLateUpdate, PlayerLoopTiming.PreLateUpdate);
            if (gameLoop.HasFlagUnsafe(EGameLoopType.FixedUpdate)) GameLoop.Register(this, OnFixedUpdate, PlayerLoopTiming.PreFixedUpdate);
        }

        protected virtual void UnRegister()
        {
            if (gameLoop.HasFlagUnsafe(EGameLoopType.Update)) GameLoop.Unregister(this, PlayerLoopTiming.PreUpdate);
            if (gameLoop.HasFlagUnsafe(EGameLoopType.LateUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreLateUpdate);
            if (gameLoop.HasFlagUnsafe(EGameLoopType.FixedUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreFixedUpdate);
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
    }
}