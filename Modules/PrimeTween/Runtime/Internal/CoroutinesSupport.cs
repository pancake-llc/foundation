using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;

namespace PrimeTween {
    public partial struct Tween : IEnumerator {
        // todo add docs
        [NotNull]
        public IEnumerator ToYieldInstruction() {
            if (!IsAlive) {
                return Enumerable.Empty<object>().GetEnumerator();
            }
            var result = tween.coroutineEnumerator;
            result.SetTween(this);
            return result;
        }

        bool IEnumerator.MoveNext() {
            PrimeTweenManager.Instance.warnStructBoxingInCoroutineOnce();
            return IsAlive;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(IsAlive);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    public partial struct Sequence : IEnumerator {
        // todo add docs
        [NotNull]
        public IEnumerator ToYieldInstruction() => GetLongestOrDefault().ToYieldInstruction();

        bool IEnumerator.MoveNext() {
            PrimeTweenManager.Instance.warnStructBoxingInCoroutineOnce();
            return IsAlive;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(IsAlive);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    internal class TweenCoroutineEnumerator : IEnumerator {
        Tween tween { get; set; }
        bool isRunning;

        internal void SetTween(Tween _tween) {
            Assert.IsFalse(isRunning);
            Assert.IsTrue(!tween.IsCreated || tween.Equals(_tween));
            Assert.IsTrue(_tween.IsAlive);
            tween = _tween;
            isRunning = true;
        }

        bool IEnumerator.MoveNext() {
            var result = tween.IsAlive;
            if (!result) {
                resetEnumerator();
            }
            return result;
        }

        internal void resetEnumerator() {
            tween = default;
            isRunning = false;
        }

        object IEnumerator.Current {
            get {
                Assert.IsTrue(tween.IsAlive);
				Assert.IsTrue(isRunning);
                return null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }
}