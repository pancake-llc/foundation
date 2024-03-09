using Pancake.PlayerLoop;
using UnityEngine;

namespace Pancake
{
    public abstract class GameUnit : GameComponent, IUpdate, IPostUpdate, IFixedUpdate, IPostFixedUpdate, ILateUpdate, IPostLateUpdate
    {
        [Header("base"), SerializeField] private GameLoopType gameLoop;

        private void OnEnable() { Register(); }

        private void OnDisable() { UnRegister(); }

        protected virtual void Register()
        {
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.Update)) GameLoop.Register(this, (this as IUpdate).OnUpdate, PlayerLoopTiming.PreUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostUpdate)) GameLoop.Register(this, (this as IPostUpdate).OnPostUpdate, PlayerLoopTiming.PostUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.LateUpdate)) GameLoop.Register(this, (this as ILateUpdate).OnLateUpdate, PlayerLoopTiming.PreLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostLateUpdate)) GameLoop.Register(this, (this as IPostLateUpdate).OnPostLateUpdate, PlayerLoopTiming.PostLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.FixedUpdate)) GameLoop.Register(this, (this as IFixedUpdate).OnFixedUpdate, PlayerLoopTiming.PreFixedUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostFixedUpdate)) GameLoop.Register(this, (this as IPostFixedUpdate).OnPostFixedUpdate, PlayerLoopTiming.PostFixedUpdate);
        }

        protected virtual void UnRegister()
        {
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.Update)) GameLoop.Unregister(this, PlayerLoopTiming.PreUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PostUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.LateUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostLateUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PostLateUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.FixedUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PreFixedUpdate);
            if (C.HasFlagUnsafe(gameLoop, GameLoopType.PostFixedUpdate)) GameLoop.Unregister(this, PlayerLoopTiming.PostFixedUpdate);
        }

        public virtual void OnUpdate() { }

        public virtual void OnPostUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnPostFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnPostLateUpdate() { }
    }
}