using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;

namespace PrimeTween {
    public partial struct Tween : IEnumerator {
        /// <summary>Use this method to wait for a Tween in coroutines.</summary>
        /// <example><code>
        /// IEnumerator Coroutine() {
        ///     yield return Tween.Delay(1).ToYieldInstruction();
        /// }
        /// </code></example>
        [NotNull]
        public IEnumerator ToYieldInstruction() {
            if (!isAlive) {
                return Enumerable.Empty<object>().GetEnumerator();
            }
            var result = tween.coroutineEnumerator;
            result.Setup(this);
            return result;
        }

        bool IEnumerator.MoveNext() {
            PrimeTweenManager.Instance.warnStructBoxingInCoroutineOnce();
            return isAlive;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(isAlive);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    public partial struct Sequence : IEnumerator {
        /// <summary>Use this method to wait for a Sequence in coroutines.</summary>
        /// <example><code>
        /// IEnumerator Coroutine() {
        ///     var sequence = Sequence.Create(Tween.Delay(1)).ChainCallback(() =&gt; Debug.Log("Done!"));
        ///     yield return sequence.ToYieldInstruction();
        /// }
        /// </code></example>
        [NotNull]
        public IEnumerator ToYieldInstruction() {
            if (!isAlive) {
                return Enumerable.Empty<object>().GetEnumerator();
            }
            var result = first.tween.coroutineEnumerator;
            result.Setup(this);
            return result;
        }

        bool IEnumerator.MoveNext() {
            PrimeTweenManager.Instance.warnStructBoxingInCoroutineOnce();
            return isAlive;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(isAlive);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    internal class TweenCoroutineEnumerator : IEnumerator {
        Tween tween;
        Sequence sequence;
        bool isRunning;

        internal void Setup(Tween _tween) {
            Assert.IsFalse(isRunning);
            Assert.IsTrue(!tween.IsCreated || tween.Equals(_tween));
            Assert.IsTrue(_tween.isAlive);
            Assert.IsFalse(sequence.IsCreated);
            tween = _tween;
            isRunning = true;
            Assert.IsTrue(isAlive);
        }

        internal void Setup(Sequence _sequence) {
            Assert.IsFalse(isRunning);
            Assert.IsTrue(!sequence.IsCreated || sequence.Equals(_sequence));
            Assert.IsTrue(_sequence.isAlive);
            Assert.IsFalse(tween.IsCreated);
            sequence = _sequence;
            isRunning = true;
            Assert.IsTrue(isAlive);
        }

        bool IEnumerator.MoveNext() {
            var result = isAlive;
            if (!result) {
                resetEnumerator();
            }
            return result;
        }

        internal bool isAlive {
            get {
                Assert.IsFalse(tween.IsCreated && sequence.IsCreated);
                return tween.isAlive || sequence.isAlive;
            }
        }
        
        internal void resetEnumerator() {
            tween = default;
            sequence = default;
            isRunning = false;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(isAlive);
				Assert.IsTrue(isRunning);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }
}