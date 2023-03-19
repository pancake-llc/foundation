using System;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Is the place to store the seats of the passengers on the Bus
    /// </summary>
    public static class Cabin
    {
        private static readonly MagicLinkedList<Entity> Entities = new MagicLinkedList<Entity>();

        /// <summary>
        /// GetComponent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponent<T>() where T : Entity { return (T) GetComponent(typeof(T)); }

        /// <summary>
        /// GetComponent
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Entity GetComponent(Type type)
        {
            var current = Entities.First;
            while (current != null)
            {
                if (current.Value.GetType() == type) return current.Value;

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// GetComponent
        /// </summary>
        /// <param name="typeName">name of component to get</param>
        /// <returns></returns>
        public static Entity GetComponent(string typeName)
        {
            var current = Entities.First;
            while (current != null)
            {
                var type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName) return current.Value;

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// Shutdown bus
        /// </summary>
        /// <param name="type"></param>
        public static void Shutdown(ShutdownType type)
        {
            // todo
        }

        internal static void Register(Entity passenger)
        {
            var type = passenger.GetType();
            var current = Entities.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    Debug.LogError("Passenger component type '{0}' is already exist.".Format(type.FullName));
                    return;
                }

                current = current.Next;
            }

            Entities.AddLast(passenger);
        }
    }
}