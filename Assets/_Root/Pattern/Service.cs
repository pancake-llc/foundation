using System;
using System.Runtime.CompilerServices;

namespace Pancake.Core
{
    /// <summary>
    /// Service - Service locator wrapper.
    /// </summary>
    public static class Service<T> where T : class
    {
        // ReSharper disable once InconsistentNaming
        private static T _instance;

        /// <summary>
        /// Gets global instance of T type.
        /// </summary>
        /// <param name="createIfNotExists">If true and instance not exists - new instance will be created.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get(bool createIfNotExists = false)
        {
            if (_instance != null)
            {
                return _instance;
            }

            if (createIfNotExists)
            {
                _instance = (T) Activator.CreateInstance(typeof(T), true);
            }

            return _instance;
        }

        /// <summary>
        /// Sets global instance of T type.
        /// </summary>
        /// <param name="instance">New instance of T type.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Set(T instance)
        {
            _instance = instance;
            return _instance;
        }
    }
}