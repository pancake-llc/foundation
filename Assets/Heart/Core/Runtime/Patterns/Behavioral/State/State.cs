namespace Pancake.Pattern
{
    /// <summary> State base class. </summary>
    public abstract class State : IState
    {
        /// <inheritdoc/>
        public virtual void OnEnter() { }

        /// <inheritdoc/>
        public virtual void OnUpdate() { }

        /// <inheritdoc/>
        public virtual void OnExit() { }
    }
}