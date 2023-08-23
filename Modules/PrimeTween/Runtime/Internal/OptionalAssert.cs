using System.Diagnostics;
using UnityEngine.Assertions;

namespace PrimeTween {
    internal static class OptionalAssert {
        const string PRIME_TWEEN_SAFETY_CHECKS = "PRIME_TWEEN_SAFETY_CHECKS";
        
        [Conditional(PRIME_TWEEN_SAFETY_CHECKS)]
        internal static void IsTrue(bool condition) {
            Assert.IsTrue(condition);
        }

        [Conditional(PRIME_TWEEN_SAFETY_CHECKS)]
        internal static void AreEqual<T>(T expected, T actual) {
            Assert.AreEqual(expected, actual);
        }

        [Conditional(PRIME_TWEEN_SAFETY_CHECKS)]
        internal static void IsFalse(bool condition) {
            Assert.IsFalse(condition);
        }
        
        [Conditional(PRIME_TWEEN_SAFETY_CHECKS)]
        internal static void IsNotNull<T>(T value) where T : class {
            Assert.IsNotNull(value);
        }
        
        [Conditional(PRIME_TWEEN_SAFETY_CHECKS)]
        internal static void IsNull<T>(T value, string msg) where T : class {
            Assert.IsNull(value, msg);
        }
    }
}