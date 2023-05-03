using Pancake.Apex;

namespace Pancake.ApexEditor
{
    public interface IManipulatorInitializaton
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute);
    }
}