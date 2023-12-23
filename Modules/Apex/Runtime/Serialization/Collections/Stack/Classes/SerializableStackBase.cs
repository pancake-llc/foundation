using UnityEngine;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    /// <summary>
    /// Represents base class for all serialized Stack(T) collections.
    /// </summary>
    public abstract class SerializableStackBase : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Called after engine deserializes this object.
        /// 
        /// Implement this method to receive a callback after engine deserializes this object.
        /// </summary>
        public abstract void OnAfterDeserialize();

        /// <summary>
        /// Called before engine serializes this object.
        /// 
        /// Implement this method to receive a callback before engine serializes this object.
        /// </summary>
        public abstract void OnBeforeSerialize();
    }
}