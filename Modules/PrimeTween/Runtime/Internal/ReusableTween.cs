using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrimeTween {
    [Serializable]
    internal class ReusableTween {
        #if UNITY_EDITOR
        [SerializeField, HideInInspector] internal string debugDescription;
        #endif
        internal int id;
        /// Holds a reference to tween's target. If the target is UnityEngine.Object, the tween will gracefully stop when the target is destroyed. That is, destroying object with running tweens is perfectly ok.
        /// Keep in mind: when animating plain C# objects (not derived from UnityEngine.Object), the plugin will hold a strong reference to the object for the entire tween duration.
        ///     If plain C# target holds a reference to UnityEngine.Object and animates its properties, then it's user's responsibility to ensure that UnityEngine.Object still exists.
        [CanBeNull] internal object target;
        bool targetIsUnityObject;
        [SerializeField, CanBeNull] internal UnityEngine.Object unityTarget; 
        [SerializeField] internal bool isPaused;
        internal bool isAlive;
        [SerializeField] internal float elapsedTime;
        internal float elapsedTimeInCurrentCycle => elapsedTime;
        internal float easedInterpolationFactor;
        internal float totalDuration;
        internal PropType propType;
        internal TweenType tweenType;
        [SerializeField] internal ValueContainer startValue;
        [SerializeField] internal ValueContainer endValue;
        ValueContainer diff;
        [SerializeField] internal TweenSettings settings;
        [SerializeField] internal int cyclesDone;

        internal object customOnValueChange;
        internal int intParam;

        internal Tween.ShakeData shakeData;
        
        Action<ReusableTween> onValueChange;
        
        [CanBeNull] internal Action<ReusableTween> onComplete;
        [CanBeNull] object onCompleteCallback;
        [CanBeNull] object onCompleteTarget;
        
        internal Tween waitFor;
        internal Sequence sequence;
        internal Tween nextInSequence;
        internal int sequenceCycles;
        internal int sequenceCyclesDone;
        internal Sequence parentSequence;
        internal Sequence childSequence;

        internal Func<ReusableTween, ValueContainer> getter;
        internal bool startFromCurrent;

        bool stoppedEmergently;
        internal bool isInterpolationCompleted;
        internal readonly TweenCoroutineEnumerator coroutineEnumerator = new TweenCoroutineEnumerator();

        internal bool updateAndCheckIfRunning(float dt) {
            const bool isRunning = true;
            const bool shouldRemove = !isRunning;
            if (!isAlive) {
                if (sequence.IsCreated && sequence.first.id == id && !isPaused) {
                    elapsedTime += dt; // update elapsedTime after death because Sequence relies on elapsedTime when calculates Sequence.elapsedTime and Sequence.elapsedTimeTotal
                }
                return shouldRemove;
            }
            if (waitFor.IsAlive) {
                return isRunning;
            }
            if (isPaused) {
                return isRunning;
            }
            elapsedTime += dt;
            var isWaitingForStartDelay = elapsedTime < settings.startDelay;
            if (isWaitingForStartDelay) {
                return isRunning;
            }
            if (isUnityTargetDestroyed()) {
                warnOnCompleteIgnored(LogType.Warning, true);
                // No need to warn that target was destroyed during animation, this is normal.
                // 'target' is the tween's 'owner' so tweens should be tied to the target's lifetime.
                EmergencyStop();
                return shouldRemove;
            }
            var halfDt = dt * 0.5f;
            if (!isInterpolationCompleted) {
                var startDelayAndDuration = settings.startDelay + settings.duration;
                isInterpolationCompleted = elapsedTime >= startDelayAndDuration - halfDt;
                float interpolationFactor;
                if (isInterpolationCompleted) {
                    interpolationFactor = 1;
                } else {
                    var duration = settings.duration;
                    var _elapsedTimeInterpolating = elapsedTime - settings.startDelay;
                    OptionalAssert.IsTrue(duration > 0 && _elapsedTimeInterpolating >= 0 && _elapsedTimeInterpolating <= duration);
                    interpolationFactor = _elapsedTimeInterpolating / duration;
                }
                OptionalAssert.IsTrue(interpolationFactor <= 1);
                // ReportOnValueChange() calls onValueChange(), and onValueChange() can execute any code, including Tween.StopAll() or Tween.Stop().
                // So we have to check if a tween wasn't killed after the calling ReportOnValueChange()
                if (!ReportOnValueChange(interpolationFactor) || !isAlive) {
                    return shouldRemove;
                }
            }
            var isWaitingForEndDelay = elapsedTime < totalDuration - halfDt;
            if (isWaitingForEndDelay) {
                return isRunning;
            }
            Assert.AreNotEqual(0, settings.cycles);
            cyclesDone++;
            elapsedTime = 0; // after completing a cycle it's reasonable that elapsedTime should be reset to 0 because new cycle has begun
            if (cyclesDone == settings.cycles) {
                kill();
                ReportOnComplete();
                updateSequenceAfterKill();
                return shouldRemove;
            }
            
            // no need to reset startFromCurrent here because getter should be used only once on tween start
            // var isCustomTween = getter == null;
            // startFromCurrent = settings.startFromCurrentValue && !isCustomTween;
            
            Assert.IsFalse(startFromCurrent);
            onCycleComplete();
            return isRunning;
        }

        internal void onCycleComplete() {
            if (settings.cycleMode == CycleMode.Incremental) {
                increment();
            }
            shakeData.onCycleComplete(this);
            isInterpolationCompleted = false;
        }

        void increment() {
            if (propType == PropType.Quaternion) {
                startValue.CopyFrom(ref endValue.QuaternionVal);
                endValue.QuaternionVal = (endValue.QuaternionVal * diff.QuaternionVal).normalized; // normalize the result because the float imprecision accumulates over time turning the quaternion to zero
            } else {
                startValue = endValue;
                endValue.Vector4Val += diff.Vector4Val;
            }
        }

        internal void rewindIncrementalTween() {
            Assert.AreEqual(settings.cycles, cyclesDone);
            if (settings.cycleMode != CycleMode.Incremental) {
                return;
            }
            var oldDiff = diff;
            ValueContainer invertedDiff = default;
            if (propType == PropType.Quaternion) {
                invertedDiff.QuaternionVal = Quaternion.Inverse(oldDiff.QuaternionVal);
            } else {
                invertedDiff.Vector4Val = -oldDiff.Vector4Val;
            }
            diff = invertedDiff;
            for (int i = 0; i < settings.cycles; i++) {
                increment();
            }
            diff = oldDiff;
        }
        
        internal void Reset() {
            Assert.AreEqual(0, aliveTweensInSequence);
            Assert.AreEqual(0, sequenceCycles);
            Assert.AreEqual(0, sequenceCyclesDone);
            Assert.IsFalse(isAlive);
            Assert.IsFalse(sequence.IsCreated);
            Assert.IsFalse(nextInSequence.IsCreated);
            Assert.IsFalse(IsInSequence());
            Assert.IsFalse(parentSequence.IsCreated);
            Assert.IsFalse(childSequence.IsCreated);
            #if UNITY_EDITOR
            debugDescription = null;
            #endif
            target = null;
            unityTarget = null;
            propType = PropType.None;
            settings.customEase = null;
            customOnValueChange = null;
            shakeData.Reset();
            onValueChange = null;
            onComplete = null;
            onCompleteCallback = null;
            onCompleteTarget = null;
            getter = null;
            stoppedEmergently = false;
            isInterpolationCompleted = false;
            waitFor = default;
            coroutineEnumerator.resetEnumerator();
            tweenType = TweenType.None;
        }

        internal void OnComplete([NotNull] Action _onComplete) {
            Assert.IsNotNull(_onComplete);
            validateOnCompleteAssignment();
            onCompleteCallback = _onComplete;
            onComplete = tween => {
                var callback = tween.onCompleteCallback as Action;
                Assert.IsNotNull(callback);
                try {
                    callback();
                } catch (Exception e) {
                    Debug.LogError($"Tween's onComplete callback raised exception, target: {tween.GetDescription()}, exception:\n{e}", tween.unityTarget);
                }
            };
        }

        internal void OnComplete<T>(T _target, [NotNull] Action<T> _onComplete) where T : class {
            Assert.IsNotNull(_onComplete);
            validateOnCompleteAssignment();
            onCompleteTarget = _target;
            onCompleteCallback = _onComplete;
            onComplete = tween => {
                var callback = tween.onCompleteCallback as Action<T>;
                Assert.IsNotNull(callback);
                var _onCompleteTarget = tween.onCompleteTarget as T;
                try {
                    callback(_onCompleteTarget);
                } catch (Exception e) {
                    Debug.LogError($"Tween's onComplete callback raised exception, target: {tween.GetDescription()}, exception:\n{e}", tween.unityTarget);
                }
            };
        }

        void validateOnCompleteAssignment() {
            const string msg = "Tween already has an onComplete callback. Adding more callbacks is not allowed.\n" +
                               "Workaround: add tween to Sequence and use ChainCallback().\n";
            Assert.IsNull(onCompleteTarget, msg);
            Assert.IsNull(onCompleteCallback, msg);
            Assert.IsNull(onComplete, msg);
        }

        /// _getter is null for custom tweens
        internal void Setup([CanBeNull] object _target, ref TweenSettings _settings, [NotNull] Action<ReusableTween> _onValueChange, [CanBeNull] Func<ReusableTween, ValueContainer> _getter, bool _startFromCurrent) {
            Assert.IsTrue(_settings.cycles >= -1);
            Assert.IsNotNull(_onValueChange);
            Assert.IsNull(getter);
            Assert.AreNotEqual(PropType.None, propType);
            if (_settings.ease == Ease.Default) {
                _settings.ease = PrimeTweenManager.Instance.defaultEase;
            } else if (_settings.ease == Ease.Custom) {
                if (_settings.customEase == null || !TweenSettings.ValidateCustomCurveKeyframes(_settings.customEase)) {
                    Debug.LogError($"Ease type is Ease.Custom, but {nameof(TweenSettings.customEase)} is not configured correctly.");
                    _settings.ease = PrimeTweenManager.Instance.defaultEase;
                }
            }
            target = _target;
            setUnityTarget(_target);
            elapsedTime = 0f;
            easedInterpolationFactor = 0f;
            isPaused = false;
            revive();

            cyclesDone = 0;
            _settings.SetValidValues();
            settings.CopyFrom(ref _settings);
            recalculateTotalDuration();
            Assert.IsTrue(totalDuration >= 0);
            onValueChange = _onValueChange;
            Assert.IsFalse(_startFromCurrent && _getter == null);
            startFromCurrent = _startFromCurrent;
            getter = _getter;
            if (!_startFromCurrent) {
                cacheDiff();
            }
        }

        internal void setUnityTarget(object _target) {
            var unityObject = _target as UnityEngine.Object;
            unityTarget = unityObject;
            targetIsUnityObject = unityObject != null;
        }

        /// Tween.Custom and Tween.ShakeCustom try-catch the <see cref="onValueChange"/> and calls <see cref="ReusableTween.EmergencyStop"/> if an exception occurs.
        /// <see cref="ReusableTween.EmergencyStop"/> sets <see cref="stoppedEmergently"/> to true.
        internal bool ReportOnValueChange(float _interpolationFactor) {
            if (tweenType == TweenType.Delay) {
                return true;
            }
            if (startFromCurrent) {
                startFromCurrent = false;
                startValue = Tween.tryGetStartValueFromOtherShake(this) ?? getter(this);
                cacheDiff();
            }
            easedInterpolationFactor = calcEasedT(_interpolationFactor);
            onValueChange(this);
            if (stoppedEmergently) {
                warnOnCompleteIgnored(LogType.Error, false);
                return false;
            }
            return true;
        }

        void ReportOnComplete() {
            Assert.IsTrue(isInterpolationCompleted);
            Assert.IsFalse(startFromCurrent);
            Assert.AreEqual(settings.cycles, cyclesDone);
            Assert.IsFalse(isAlive);
            onComplete?.Invoke(this);
        }

        internal bool tryManipulate() {
            if (warnIfTargetDestroyed()) {
                EmergencyStop();
                return false;
            }
            return true;
        }

        internal bool warnIfTargetDestroyed() {
            if (isUnityTargetDestroyed()) {
                Debug.LogWarning($"{Constants.targetDestroyed} Tween: {GetDescription()}");
                warnOnCompleteIgnored(LogType.Warning, true);
                return true;
            }
            return false;
        }

        internal bool isUnityTargetDestroyed() {
            return targetIsUnityObject && unityTarget == null;
        }
        
        internal bool HasOnComplete => onComplete != null;

        [NotNull]
        internal string GetDescription() {
            string result = "";
            if (target != null) {
                result += $"{(unityTarget != null ? unityTarget.name : target.GetType().Name)} / ";
            }
            if (tweenType == TweenType.Delay && settings.duration == 0) {
            } else {
                result += $"{(tweenType != TweenType.None ? tweenType.ToString() : propType.ToString())} / duration {settings.duration:0.##} / ";
            }
            result += $"id {id}";
            if (sequence.IsCreated) {
                result += $" / sequence {sequence.id}";
            }
            return result;
        }
    
        internal float calcDurationWithWaitDependencies() {
            var result = 0f;
            var current = new Tween(this);
            while (true) {
                var currentTween = current.tween;
                var cycles = currentTween.settings.cycles;
                Assert.AreNotEqual(-1, cycles, "It's impossible to calculate the duration of an infinite tween (cycles == -1).");
                Assert.AreNotEqual(0, cycles);
                result += currentTween.totalDuration * cycles;
                var waitDependency = currentTween.waitFor;
                if (!waitDependency.IsCreated) {
                    break;
                }
                current = waitDependency;
            }
            return result;
        }

        internal void recalculateTotalDuration() {
            totalDuration = settings.startDelay + settings.duration + settings.endDelay;
        }

        internal void updateSequenceAfterKill() {
            if (sequence.IsCreated) {
                sequence.onTweenKilled(id);
            }
        }

        internal float FloatVal => startValue.x + diff.x * easedInterpolationFactor;
        internal Vector2 Vector2Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector2(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT);
            }
        }
        internal Vector3 Vector3Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector3(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT);
            }
        }
        internal Vector4 Vector4Val {
            get {
                var easedT = easedInterpolationFactor;
                return new Vector4(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Color ColorVal {
            get {
                var easedT = easedInterpolationFactor;
                return new Color(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Rect RectVal {
            get {
                var easedT = easedInterpolationFactor;
                return new Rect(
                    startValue.x + diff.x * easedT,
                    startValue.y + diff.y * easedT,
                    startValue.z + diff.z * easedT,
                    startValue.w + diff.w * easedT);
            }
        }
        internal Quaternion QuaternionVal => Quaternion.SlerpUnclamped(startValue.QuaternionVal, endValue.QuaternionVal, easedInterpolationFactor);

        float calcEasedT(float t) {
            var ease = settings.ease;
            var customEase = settings.customEase;
            var isForwardCycle = cyclesDone % 2 == 0;
            if (isForwardCycle) {
                return Easing.Evaluate(t, ease, customEase);
            }
            switch (settings.cycleMode) {
                case CycleMode.Restart:
                case CycleMode.Incremental:
                    return Easing.Evaluate(t, ease, customEase);
                case CycleMode.Yoyo:
                    return 1 - Easing.Evaluate(t, ease, customEase);
                case CycleMode.Rewind:
                    return Easing.Evaluate(1 - t, ease, customEase);
                default:
                    throw new Exception();
            }
        }

        internal void cacheDiff() {
            Assert.IsFalse(startFromCurrent);
            Assert.AreNotEqual(PropType.None, propType);
            if (propType == PropType.Quaternion) {
                diff.QuaternionVal = Quaternion.Inverse(startValue.QuaternionVal) * endValue.QuaternionVal;
            } else {
                diff.x = endValue.x - startValue.x;
                diff.y = endValue.y - startValue.y;
                diff.z = endValue.z - startValue.z;
                diff.w = endValue.w - startValue.w;
            }
        }

        /// This method can be called at any time (for example, immediately after destroying the unityTarget), so before calling it, we should guarantee that <see cref="isUnityTargetDestroyed()"/> == false.
        internal void ForceComplete() {
            Assert.IsFalse(isUnityTargetDestroyed());
            kill(); // protects from recursive call

            if (settings.cycleMode == CycleMode.Incremental) {
                var incrementsLeft = settings.cycles - cyclesDone - 1;
                for (int i = 0; i < incrementsLeft; i++) {
                    increment();
                }
            }
            
            cyclesDone = settings.cycles - 1; // simulate the last cycle so calcEasedT() calculates the correct value depending on cycleMode
            if (!ReportOnValueChange(1)) {
                return;
            }
            
            isInterpolationCompleted = true;
            cyclesDone = settings.cycles;
            ReportOnComplete();
            Assert.IsTrue(sequence.IsCreated || !isAlive);
        }

        internal void warnOnCompleteIgnored(LogType logType, bool isTargetDestroyed) {
            if (HasOnComplete && PrimeTweenManager.Instance.warnDestroyedTweenHasOnComplete) {
                onComplete = null;
                var msg = $"{Constants.onCompleteCallbackIgnored} Tween: {GetDescription()}.\n";
                if (isTargetDestroyed) {
                    msg += $"To prevent this warning, please {nameof(Tween.Stop)}/{nameof(Tween.Complete)}() the tween/sequence manually before destroying the target.\n" +
                           $"{Constants.buildWarningCanBeDisabledMessage(nameof(PrimeTweenManager.warnDestroyedTweenHasOnComplete))}\n";
                }
                Debug.unityLogger.Log(logType, (object)msg, unityTarget);
            }
        }

        internal void EmergencyStop() {
            if (sequence.IsCreated) {
                sequence.emergencyStop();
                Assert.IsFalse(isAlive);
            } else if (isAlive) {
                // EmergencyStop() can be called after ForceComplete() and a caught exception in Tween.Custom()
                kill();
            }
            stoppedEmergently = true;
            Assert.IsFalse(isAlive);
            Assert.IsFalse(sequence.IsAlive);
        }

        internal void kill() {
            // Debug.Log($"{Time.frameCount} kill {GetDescription()}");
            Assert.IsTrue(isAlive);
            isAlive = false;
        }

        internal void revive() {
            // Debug.Log($"{Time.frameCount} revive {GetDescription()}");
            OptionalAssert.IsFalse(isAlive);
            isAlive = true;
        }

        internal bool IsInSequence() {
            return sequence.IsCreated || nextInSequence.IsCreated;
        }

        internal void setNextInSequence(Tween? tween) {
            if (!tween.HasValue) {
                Assert.IsTrue(sequence.IsCreated);
                nextInSequence = default;
                return;
            }
            Assert.IsTrue(sequence.IsAlive);
            Assert.IsFalse(nextInSequence.IsCreated);
            nextInSequence = tween.Value;
            sequence.first.tween.addAliveTweensInSequence(1, tween.Value.id);
        }

        internal void setWaitFor(Tween tween) {
            Assert.IsFalse(waitFor.IsCreated);
            Assert.IsTrue(tween.IsCreated);
            Assert.AreNotEqual(id, tween.id);
            waitFor = tween;
        }

        internal bool trySetPause(bool _isPaused) {
            if (sequence.IsCreated) {
                Debug.LogError(Constants.setPauseOnTweenInsideSequenceError);
                return false;
            }
            if (isPaused == _isPaused) {
                return false;
            }
            isPaused = _isPaused;
            return true;
        }

        internal Vector3 shakeStrengthPerAxis {
            get => endValue.Vector3Val;
            set => endValue.Vector3Val = value;
        }
        
        internal float shakeFrequency {
            get => endValue.w;
            set => endValue.w = value;
        }

        // ReSharper disable once UnusedParameter.Global
        internal void addAliveTweensInSequence(int _diff, int tweenId) {
            aliveTweensInSequence += _diff;
        }
        
        internal int aliveTweensInSequence;
    }
}