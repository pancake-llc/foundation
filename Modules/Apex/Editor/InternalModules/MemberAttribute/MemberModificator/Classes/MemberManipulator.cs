using Pancake.Apex;

namespace Pancake.ApexEditor
{
    public abstract class MemberManipulator : IManipulatorInitializaton
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public virtual void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute) { }

        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public virtual void OnBeforeGUI() { }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public virtual void OnAfterGUI() { }
    }
}