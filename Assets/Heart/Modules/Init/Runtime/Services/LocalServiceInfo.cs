using UnityEngine;

namespace Sisus.Init.Internal
{
    internal sealed class LocalServiceInfo
    {
        /// <summary>
        /// A reference to the service itself, or an object that can provide the service
        /// (e.g. an initializer, or a value provider).
        /// </summary>
        public readonly Object serviceOrProvider;

        /// <summary>
        /// A reference to the object that that is responsible for registering the service.
        /// <para>
        /// A <see cref="Services"/>, <see cref="ServiceTag"/> or <see cref="Initializer"/> component.
        /// </para>
        /// </summary>
        public readonly Component registerer;

        /// <summary>
        /// Specifies which clients have access to the service.
        /// <para>
        /// If null, then this is unknown; could be everyone, or could be none.
        /// </para>
        /// </summary>
        public readonly Clients toClients;

        public LocalServiceInfo(Object serviceOrProvider, Clients toClients, Component registerer)
        {
            this.serviceOrProvider = serviceOrProvider;
            this.toClients = toClients;
        }
    }
}