using System;
using System.Runtime.CompilerServices;

namespace Pancake
{
    public static class Error
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowArgumentNullException<T>(this T value, string paramName) where T : class
        {
            if (value == null) ThrowArgumentNull(paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNull(string paramName) { throw new ArgumentNullException(paramName); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception ThrowArgumentOutOfRange(string paramName) { throw new ArgumentOutOfRangeException(paramName); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception ThrowNoElements() { throw new InvalidOperationException("Source sequence doesn't contain any elements."); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception ThrowMoreThanOneElement() { throw new InvalidOperationException("Source sequence contains more than one element."); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(string message) { throw new ArgumentException(message); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotYetCompleted() { throw new InvalidOperationException("Not yet completed."); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNotYetCompleted<T>() { throw new InvalidOperationException("Not yet completed."); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowWhenContinuationIsAlreadyRegistered<T>(T continuationField) where T : class
        {
            if (continuationField != null) ThrowInvalidOperationExceptionCore("continuation is already registered.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationExceptionCore(string message) { throw new InvalidOperationException(message); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowOperationCanceledException() { throw new OperationCanceledException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception ThrowMoreThanOneMatch() { throw new InvalidOperationException("Sequence contains more than one matching element"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception ThrowNoMatch() { throw new InvalidOperationException("Sequence contains no matching element"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception ThrowNotFound(string key) { throw new Exception($"No data saved with key: {key}"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception ThrowNotSupported() { throw new NotSupportedException(); }
    }
}