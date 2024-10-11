using System;

namespace Pancake.Common
{
    public static class Checker
    {
        public static T CheckNotNull<T>(T reference) { return CheckNotNull(reference, string.Empty); }

        public static T CheckNotNull<T>(T reference, string message)
        {
            if (reference is UnityEngine.Object obj && obj.OrNull() == null) throw new ArgumentNullException(message);
            if (reference is null) throw new ArgumentNullException(message);
            return reference;
        }
    }
}