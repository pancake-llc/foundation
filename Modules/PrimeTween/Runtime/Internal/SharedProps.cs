using UnityEngine;

namespace PrimeTween {
    internal readonly struct SharedProps {
        readonly bool IsAlive;
        readonly float elapsedTime;
        readonly int cyclesTotal;
        readonly int cyclesDone;
        readonly float duration;

        internal SharedProps(bool IsAlive, float elapsedTime, int cyclesTotal, int cyclesDone, float duration) {
            this.IsAlive = IsAlive;
            this.elapsedTime = elapsedTime;
            this.cyclesTotal = cyclesTotal;
            this.cyclesDone = cyclesDone;
            this.duration = duration;
        }

        internal float elapsedTimeTotal => IsAlive ? elapsedTime + cyclesDone * duration : 0;

        internal float durationTotal {
            get {
                if (!IsAlive) {
                    return 0;
                }
                if (cyclesTotal == -1) {
                    return float.PositiveInfinity;
                }
                Assert.AreNotEqual(0, cyclesTotal);
                return duration * cyclesTotal;
            }
        }

        internal float progress {
            get {
                if (!IsAlive) {
                    return 0;
                }
                if (duration == 0) {
                    return 0;
                }
                return Mathf.Min(elapsedTime / duration, 1f);
            }
        }

        internal float progressTotal {
            get {
                if (!IsAlive) {
                    return 0;
                }
                if (cyclesTotal == -1) {
                    return 0;
                }
                var _totalDuration = durationTotal;
                Assert.IsFalse(float.IsInfinity(_totalDuration));
                if (_totalDuration == 0) {
                    return 0;
                }
                return Mathf.Min(elapsedTimeTotal / _totalDuration, 1f);
            }
        }
    }
}