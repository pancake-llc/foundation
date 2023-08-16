#if PRIME_TWEEN_INSPECTOR_DEBUGGING && UNITY_EDITOR
#define ENABLE_SERIALIZATION
#endif
using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrimeTween {
    /// <summary>The main API of the PrimeTween library.<br/><br/>
    /// Use static Tween methods to start animations (tweens).<br/>
    /// Use the returned Tween struct to control the running tween and access its properties.<br/><br/>
    /// Tweens are non-reusable. That is, when a tween completes (or is stopped manually), it becomes 'dead' (<see cref="IsAlive"/> == false) and can no longer be used to control the tween or access its properties.<br/>
    /// To restart the animation from the beginning (or play in the opposite direction), simply start a new Tween. Starting tweens is very fast and doesn't allocate garbage,
    /// so you can start hundreds of tweens per seconds with no performance overhead.</summary>
    /// <example><code>
    /// var tween = Tween.LocalPositionX(transform, endValue: 1.5f, duration: 1f);
    /// // Let the tween run for some time...
    /// if (tween.IsAlive) {
    ///     Debug.Log($"Animation is still running, elapsed time: {tween.elapsedTime}.");
    /// } else {
    ///     Debug.Log("Animation is already completed.");
    /// }
    /// </code></example>
    #if ENABLE_SERIALIZATION
    [Serializable]
    #endif
    public 
        #if !ENABLE_SERIALIZATION
        readonly
        #endif
        partial struct Tween : IEquatable<Tween> {
        /// Uniquely identifies the tween.
        /// Can be observed from the Debug Inspector if PRIME_TWEEN_INSPECTOR_DEBUGGING is defined. Use only for debugging purposes.
        internal
            #if !ENABLE_SERIALIZATION
            readonly
            #endif
            int id;
            
        internal readonly ReusableTween tween;
        
        /// This should not be part of public API. When tween IsCreated and !IsAlive, it's not guaranteed to be IsCompleted, it can also be stopped.
        internal bool IsCreated => id != 0;

        internal Tween([NotNull] ReusableTween tween) {
            Assert.IsNotNull(tween);
            id = tween.id;
            this.tween = tween;
        }

        /// A tween is 'alive' when it has been created and is not stopped or completed yet. Paused tween is also considered 'alive'.
        public bool IsAlive => id != 0 && tween.id == id && tween.isAlive;

        /// Elapsed time of the current cycle.
        public float elapsedTime => IsAlive ? tween.elapsedTimeInCurrentCycle : 0;
        /// The total number of cycles. Returns -1 to indicate infinite number cycles.
        public int cyclesTotal => IsAlive ? tween.settings.cycles : 0;
        public int cyclesDone => IsAlive ? tween.cyclesDone : 0;
        /// The duration of one cycle.
        public float duration {
            get {
                if (!IsAlive) {
                    return 0;
                }
                var result = tween.totalDuration;
                TweenSettings.validateFiniteDuration(result);
                return result;
            }
        }

        [NotNull]
        public override string ToString() {
            return IsAlive ? tween.GetDescription() : $"DEAD / id {id}";
        }

        SharedProps sharedProps => new SharedProps(IsAlive, elapsedTime, cyclesTotal, cyclesDone, duration);
        /// Elapsed time of all cycles.
        public float elapsedTimeTotal => sharedProps.elapsedTimeTotal;
        /// <summary>The duration of all cycles. If cycles == -1, returns <see cref="float.PositiveInfinity"/>.</summary>
        public float durationTotal => sharedProps.durationTotal;
        /// Normalized progress of the current cycle expressed in 0..1 range.
        public float progress => sharedProps.progress;
        /// Normalized progress of all cycles expressed in 0..1 range.
        public float progressTotal => sharedProps.progressTotal;

        /// <summary>The current percentage of change between 'startValue' and 'endValue' values in 0..1 range.</summary>
        public float interpolationFactor {
            get {
                if (!IsAlive) {
                    return 0;
                }
                return tween.easedInterpolationFactor;
            }
        }

        public bool IsPaused {
            get => IsAlive && tween.isPaused;
            set {
                if (tryManipulate()) {
                    tween.trySetPause(value);
                }
            }
        }

        /// Interrupts the tween, ignoring onComplete callback. 
        public void Stop() {
            if (IsAlive) {
                tween.kill();
                tween.updateSequenceAfterKill();
            }
        }

        /// Sets the tween to the endValue and calls onComplete.
        public void Complete() {
            // don't warn that tween is dead because dead tween means that it's already 'completed'
            if (IsAlive && tween.tryManipulate()) { 
                tween.ForceComplete();
                tween.updateSequenceAfterKill();
            }
        }

        bool tryManipulate() {
            if (IsAlive) {
                return tween.tryManipulate();
            }
            Debug.LogError(Constants.tweenIsDeadMessage);
            return false;
        }

        /// <summary>Stops the tween when it reaches 'startValue' or 'endValue' for the next time.<br/>
        /// For example, if you have an infinite tween (cycles == -1) with CycleMode.Yoyo/Rewind, and you wish to stop it when it reaches the 'endValue' (odd cycle), then set <see cref="stopAtEndValue"/> to true.
        /// To stop the animation at the 'startValue' (even cycle), set <see cref="stopAtEndValue"/> to false.</summary>
        public void SetCycles(bool stopAtEndValue) {
            if (IsAlive && (tween.settings.cycleMode == CycleMode.Restart || tween.settings.cycleMode == CycleMode.Incremental)) {
                Debug.LogWarning(nameof(SetCycles) + "(bool " + nameof(stopAtEndValue) + ") is meant to be used with CycleMode.Yoyo or Rewind. Please consider using the overload that accepts int instead.");
            }
            SetCycles(tween.cyclesDone % 2 == 0 == stopAtEndValue ? 1 : 2);
        }
        
        /// <summary>Sets the number of remaining cycles.
        /// This method modifies the <see cref="cyclesTotal"/> so that the tween will complete after the number of <see cref="cycles"/>.
        /// Setting cycles to -1 will repeat the tween indefinitely.</summary>
        public void SetCycles(int cycles) {
            Assert.IsTrue(cycles >= -1);
            if (!tryManipulate()) {
                return;
            }
            if (cycles == -1) {
                if (tween.sequence.IsCreated) {
                    Debug.LogError(Constants.infiniteTweenInSequenceError);
                    return;
                }
                tween.settings.cycles = -1;
            } else {
                TweenSettings.setCyclesTo1If0(ref cycles);
                tween.settings.cycles = tween.cyclesDone + cycles;
            }
        }

        /// <summary>Adds completion callback. Please consider using <see cref="OnComplete{T}"/> to prevent a possible capture of variable into a closure.</summary>
        public Tween OnComplete([NotNull] Action onComplete) {
            if (canAddOnComplete()) {
                tween.OnComplete(onComplete);
            }
            return this;
        }

        /// <summary>Adds completion callback.</summary>
        /// <example>The example shows how to destroy the object after the completion of a tween.
        /// Please note: we're using the '_transform' variable from the onComplete callback to prevent garbage allocation. Using the 'transform' variable directly will capture it into a closure and generate garbage.
        /// <code>
        /// Tween.PositionX(transform, endValue: 1.5f, duration: 1f)
        ///     .OnComplete(transform, _transform =&gt; Destroy(_transform.gameObject));
        /// </code></example>
        public Tween OnComplete<T>([NotNull] T target, [NotNull] Action<T> onComplete) where T : class {
            if (canAddOnComplete()) {
                tween.OnComplete(target, onComplete);
            }
            return this;
        }

        bool canAddOnComplete() {
            if (!IsAlive) {
                Debug.LogError(Constants.tweenIsDeadMessage);
                return false;
            }
            if (tween.warnIfTargetDestroyed()) {
                return false;
            }
            return true;
        }
        
        public override bool Equals(object obj) {
            return obj is Tween other && Equals(other);
        }

        public bool Equals(Tween other) {
            return id == other.id;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public Sequence Group(Tween _tween) => Sequence.Create(this).Group(_tween);
        public Sequence Chain(Tween _tween) => Sequence.Create(this).Chain(_tween);
        public Sequence Group(Sequence sequence) => Sequence.Create(this).Group(sequence);
        public Sequence Chain(Sequence sequence) => Sequence.Create(this).Chain(sequence);
    }
}