#if NET_4_6 || NET_STANDARD_2_0


using UnityEngine;

namespace Pancake.Apex.Serialization.Collections.Generic
{
    public abstract class SerializableHashSetBase : ISerializationCallbackReceiver
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
#endif