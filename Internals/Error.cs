using System;
using System.Runtime.CompilerServices;

namespace Pancake
{
    public static class Error
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowArgumentNullException<T>(this T value, string paramName) where T : class
        {
            if (value == null) ThrowArgumentNullExceptionCore(paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullExceptionCore(string paramName) { throw new ArgumentNullException(paramName); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception ArgumentOutOfRange(string paramName) { return new ArgumentOutOfRangeException(paramName); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception NoElements() { return new InvalidOperationException("Source sequence doesn't contain any elements."); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception MoreThanOneElement() { return new InvalidOperationException("Source sequence contains more than one element."); }

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
        internal static Exception ArgumentNull(string s) { return new ArgumentNullException(s); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception MoreThanOneMatch() { return new InvalidOperationException("Sequence contains more than one matching element"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception NoMatch() { return new InvalidOperationException("Sequence contains no matching element"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception NotFound(string key) { return new Exception($"No data saved with key: {key}"); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception NotSupported() { return new NotSupportedException(); }
    }
}