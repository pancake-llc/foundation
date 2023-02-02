using System;

namespace Pancake.EventBus
{
    /// <summary>
    /// Marker attribute for EventSystem singleton collection function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ListenerAttribute : Attribute
    {
        /// <summary> Should this method/class be collected </summary>
        public bool Active { get; set; } = true;
        /// <summary> Listener order </summary>
        public int Order { get; set; } = GlobalBus.k_DefaultPriority;
        /// <summary> Listener name/id, if not set it will be set to the MethodInfo name </summary>
        public string Name { get; set; } = GlobalBus.k_DefaultName;
    }
}