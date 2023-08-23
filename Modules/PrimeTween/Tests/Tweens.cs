#if TEST_FRAMEWORK_INSTALLED
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using AssertionException = UnityEngine.Assertions.AssertionException;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using SuppressMessage = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

public partial class Tests {
    [UnityTest]
    public IEnumerator QuaternionDefaultValue() {
        {
            var q = Quaternion.Euler(0, 0, 45);
            var def = new Quaternion();
            Assert.AreEqual(Quaternion.identity.normalized, def.normalized);
            Assert.AreNotEqual(Quaternion.Angle(Quaternion.identity, q), Quaternion.Angle(def, q));
            Assert.AreEqual(Quaternion.Angle(Quaternion.identity, q), Quaternion.Angle(def.normalized, q));
        }
        {
            Tween t = default;
            var def = new Quaternion();
            int numCallback = 0;
            t = Tween.Custom(def, def, 0.01f, delegate {
                numCallback++;
                var startVal = t.tween.startValue.QuaternionVal;
                var endVal = t.tween.endValue.QuaternionVal;
                // Debug.Log($"{startVal}, {endVal}");
                Assert.AreNotEqual(def, startVal);
                Assert.AreNotEqual(def, endVal);
                t.Stop();
            });
            yield return t.ToYieldInstruction();
            Assert.AreEqual(1, numCallback);
        }
    }
    
    /// This test can fail if Game window is set to 'Play Unfocused'
    [UnityTest]
    public IEnumerator StartValueIsAppliedOnFirstFrame() {
        const int iniVal = -1;
        float val = iniVal;
        const int startValue = 0;
        Tween.Custom(startValue, 1, 0.01f, newVal => val = newVal);
        Assert.AreEqual(iniVal, val);
        yield return new WaitForEndOfFrame();
        Assert.AreEqual(startValue, val);
        yield return new WaitForEndOfFrame();
        Assert.AreNotEqual(startValue, val);
    }
    
    [Test]
    public void SafetyChecksEnabled() {
        #if !PRIME_TWEEN_SAFETY_CHECKS
        Assert.Inconclusive();
        #endif
    }
    
    [UnityTest]
    public IEnumerator TweenIsDeadInOnComplete() {
        Tween t = default;
        t = Tween.Delay(0.01f, () => {
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(0, t.elapsedTime);
            Assert.AreEqual(0, t.elapsedTimeTotal);
            Assert.AreEqual(0, t.progress);
            Assert.AreEqual(0, t.progressTotal);
            Assert.AreEqual(0, t.duration);
            Assert.AreEqual(0, t.durationTotal);
        });
        yield return t.ToYieldInstruction();
    }

    [Test]
    public void ShakeDuplication1() {
        var s1 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f, startDelay: 0.1f);
        var s2 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f);
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(s2.IsAlive);
    }

    [UnityTest]
    public IEnumerator ShakeDuplication2() {
        var s1 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f);
        Assert.IsTrue(s1.IsAlive);
        var s2 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f);
        Assert.IsTrue(s1.IsAlive);
        yield return null;
        // because two shakes are started the same frame, the first one completes the second one
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(s2.IsAlive);
    }

    [UnityTest]
    public IEnumerator ShakeDuplication3() {
        var s1 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f);
        yield return null;
        var s2 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f);
        yield return null;
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(s2.IsAlive);
    }

    [UnityTest]
    public IEnumerator ShakeDuplication4() {
        const float startDelay = 0.05f;
        var s1 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f, startDelay: startDelay);
        var s2 = Tween.ShakeLocalPosition(transform, Vector3.one, 0.1f, startDelay: 0.1f);
        yield return Tween.Delay(startDelay + Time.deltaTime).ToYieldInstruction();
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(s2.IsAlive);
    }

    [UnityTest]
    public IEnumerator ShakeDuplication5() {
        var target = new GameObject(nameof(ShakeDuplication5)).transform;
        target.localPosition = new Vector3(Random.value, Random.value, Random.value);
        var iniPos = target.localPosition;
        var s1 = Tween.ShakeLocalPosition(target, Vector3.one, 0.1f);
        var seq = Sequence.Create(s1);
        Assert.IsTrue(s1.IsAlive);
        var s2 = Tween.ShakeLocalPosition(target, Vector3.one, 0.1f);
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(seq.IsAlive);
        Assert.IsTrue(s2.IsAlive);

        Assert.IsTrue(s1.tween.startFromCurrent);
        Assert.IsTrue(s2.tween.startFromCurrent);
        yield return null;
        Assert.IsTrue(s1.IsAlive);
        Assert.IsTrue(seq.IsAlive);
        Assert.IsTrue(s2.IsAlive);

        Assert.IsFalse(s1.tween.startFromCurrent);
        Assert.IsFalse(s2.tween.startFromCurrent);
        Assert.AreEqual(iniPos, s1.tween.startValue.Vector3Val);
        Assert.AreEqual(iniPos, s2.tween.startValue.Vector3Val);
    }

    [Test]
    public void ShakeDuplicationDestroyedTarget() {
        var target = new GameObject(nameof(ShakeDuplicationDestroyedTarget)).transform;
        Tween.ShakeLocalPosition(target, Vector3.one, 0.1f);
        Object.DestroyImmediate(target.gameObject);
        Tween.ShakeLocalPosition(target, Vector3.one, 0.1f);
        LogAssert.Expect(LogType.Error, new Regex("Tween's UnityEngine.Object target is null"));
    }

    [UnityTest]
    public IEnumerator FramePacing() {
        var oldFps = Application.targetFrameRate;
        const int fps = 60;
        Application.targetFrameRate = fps;
        QualitySettings.vSyncCount = 0;
        Assert.AreEqual(fps, Application.targetFrameRate);
        yield return null;
        {
            var go = new GameObject();
            go.AddComponent<FramePacingTest>();
            while (go != null) {
                yield return null;
            }
        }
        Application.targetFrameRate = oldFps;
        yield return null;
    }
    
    #if UNITY_EDITOR
    [Test]
    public void GenerateCode() {
        const string path = "Packages/com.kyrylokuzyk.primetween/Editor/CodeGenerator.asset";
        var cg = UnityEditor.AssetDatabase.LoadAssetAtPath<CodeGenerator>(path);
        cg.generateAll();
    }
    #endif
    
    [Test]
    public void TweenCompleteInvokeOnCompleteParameter() {
        {
            int numCompleted = 0;
            var t = Tween.LocalScale(transform, 1.5f, 0.01f).OnComplete(() => numCompleted++);
            t.Complete();
            Assert.AreEqual(1, numCompleted);
            t.Complete();
            Assert.AreEqual(1, numCompleted);
        }
        /*{
            int numCompleted = 0;
            var t = Tween.LocalScale(transform, 1.5f, 0.01f).OnComplete(() => numCompleted++);
            t.Complete(false);
            Assert.AreEqual(0, numCompleted);
            t.Complete(false);
            Assert.AreEqual(0, numCompleted);
            t.Complete();
            Assert.AreEqual(0, numCompleted);
        }*/
    }
    
    [Test]
    public void IgnoreFromInScale() {
        var t = Tween.LocalScale(transform, 1.5f, 0.01f);
        Assert.IsTrue(t.tween.startFromCurrent);
    }
    
    [UnityTest]
    public IEnumerator FromToValues() {
        {
            var duration = 0.1f * Random.value;
            var t = Tween.Custom(0, 0, duration, ease: Ease.Linear, onValueChange: delegate { });
            while (t.IsAlive) {
                try {
                    var tween = t.tween;
                    Assert.AreEqual(t.elapsedTime, t.progress * duration, 0.001f);
                    Assert.AreEqual(t.interpolationFactor, t.progress);
                    Assert.AreEqual(t.interpolationFactor, t.progressTotal);
                } catch {
                    throw;
                }
                yield return null;
            }
        }
        var from = Random.value;
        var to = Random.value;
        var data = new TweenSettings<float>(from, to, 0.01f);
        {
            var t = Tween.LocalPositionX(transform, data);
            Assert.AreEqual(from, t.tween.startValue.FloatVal);
            Assert.AreEqual(to, t.tween.endValue.FloatVal);
        }
        {
            var t = Tween.Custom(this, data, delegate { });
            Assert.AreEqual(from, t.tween.startValue.FloatVal);
            Assert.AreEqual(to, t.tween.endValue.FloatVal);
        }
    }
    
    [UnityTest]
    public IEnumerator TweenCompleteWhenInterpolationCompleted() {
        float curVal = 0f;
        var t = Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 2, endDelay: 1f, cycleMode: CycleMode.Yoyo);
        while (!t.tween.isInterpolationCompleted) {
            yield return null;
        }
        Assert.AreEqual(0, t.cyclesDone);
        Assert.AreEqual(1f, curVal);
        t.Complete();
        Assert.AreEqual(0f, curVal);
    }
    
    [Test]
    public async Task CycleModeIncremental() {
        {
            float curVal = 0f;
            await Tween.Custom(this, 0f, 1f, 0.01f, (_, val) => curVal = val, cycles: 2, cycleMode: CycleMode.Incremental);
            Assert.AreEqual(2f, curVal);
        }
        {
            float curVal = 0f;
            await Tween.Custom(this, 0f, 1f, 0.01f, (_, val) => curVal = val, cycles: 4, cycleMode: CycleMode.Incremental);
            Assert.AreEqual(4f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.01f, (_, val) => curVal = val, cycles: 2, cycleMode: CycleMode.Incremental)
                .Complete();
            Assert.AreEqual(2f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.01f, (_, val) => curVal = val, cycles: 4, cycleMode: CycleMode.Incremental)
                .Complete();
            Assert.AreEqual(4f, curVal);
        }
    }
    
    [Test]
    public void TweenCompleteWithEvenCycles() {
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 2, cycleMode: CycleMode.Restart)
                .Complete();
            Assert.AreEqual(1f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 4, cycleMode: CycleMode.Restart)
                .Complete();
            Assert.AreEqual(1f, curVal);
        }
        
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 2, cycleMode: CycleMode.Yoyo)
                .Complete();
            Assert.AreEqual(0f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 4, cycleMode: CycleMode.Yoyo)
                .Complete();
            Assert.AreEqual(0f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 2, cycleMode: CycleMode.Rewind)
                .Complete();
            Assert.AreEqual(0f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 4, cycleMode: CycleMode.Rewind)
                .Complete();
            Assert.AreEqual(0f, curVal);
        }
    }
    
    [Test]
    public void TweenCompleteWithOddCycles() {
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 1, cycleMode: CycleMode.Yoyo)
                .Complete();
            Assert.AreEqual(1f, curVal);
        }
        {
            float curVal = 0f;
            Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 3, cycleMode: CycleMode.Yoyo)
                .Complete();
            Assert.AreEqual(1f, curVal);
        }
    }
    
    [Test]
    public async Task TweenOnCompleteIsCalledOnceForTweenInSequence() {
        float curVal = 0f;
        int numCompleted = 0;
        var t = Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 1, cycleMode: CycleMode.Yoyo)
            .OnComplete(() => numCompleted++);
        var s = t.Chain(Tween.Delay(Time.deltaTime * 5));
        await t;
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, t.tween.cyclesDone);
        Assert.IsTrue(t.tween.sequence.IsCreated);
        Assert.AreEqual(1, numCompleted);

        Assert.IsTrue(s.IsAlive);
        Assert.AreEqual(1f, curVal);
        s.Complete();
        Assert.AreEqual(1f, curVal);
        Assert.AreEqual(1, numCompleted);
    }
    
    [Test]
    public void TweenCompleteInSequence() {
        float curVal = 0f;
        var t = Tween.Custom(this, 0f, 1f, 0.05f, (_, val) => curVal = val, cycles: 1, cycleMode: CycleMode.Yoyo);
        var s = t.Chain(Tween.Delay(0.05f));
        Assert.IsTrue(t.IsAlive);
        Assert.AreEqual(0, t.tween.cyclesDone);
        Assert.IsTrue(t.tween.sequence.IsCreated);
        Assert.IsTrue(s.IsAlive);
        Assert.AreNotEqual(1f, curVal);
        s.Complete();
        Assert.AreEqual(1f, curVal);
    }
    
    [Test]
    public async Task AwaitExceptions() {
        expectError();
        await Tween.Custom(this, 0f, 1f, 1f, delegate {
            throw new Exception();
        });
        expectError();
        await Tween.Custom(this, 0f, 1f, 1f, delegate {
            Sequence.Create(default);
        });
        expectError();
        await Tween.Custom(this, 0f, 1f, 1f, delegate {
            Sequence.Create(Tween.PositionX(transform, 1f, 1f, cycles: -1));
        });
        void expectError() {
            LogAssert.Expect(LogType.Error, new Regex("Tween was stopped because of exception"));
        }
    }

    [UnityTest]
    public IEnumerator CoroutineEnumeratorNotEnumeratedToTheEnd() {
        Tween.StopAll();
        yield return null;
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        var t = Tween.Delay(Time.deltaTime * 5);
        var e = t.ToYieldInstruction();
        Assert.IsTrue(e.MoveNext());
        yield return e.Current;
        Assert.IsTrue(t.IsAlive);
        while (t.IsAlive) {
            yield return null;
        }
        yield return null;
        Assert.AreEqual(0, tweens.Count);
        testCompletedCorEnumerator(e);
    }
    
    [UnityTest]
    public IEnumerator CoroutineEnumeratorInfiniteTween() {
        {
            var t = Tween.Position(transform, Vector3.one, 0.01f, cycles: -1);
            Tween.Delay(0.05f).OnComplete(() => t.Stop());
            yield return t.ToYieldInstruction();    
        }
        {
            var t = Tween.Position(transform, Vector3.one, 0.01f, cycles: -1);
            Tween.Delay(0.05f).OnComplete(() => t.Complete());
            yield return t.ToYieldInstruction();
        }
    }
    
    [UnityTest]
    public IEnumerator CoroutineEnumeratorMultipleToYieldInstruction() {
        var t = Tween.Delay(0.01f);
        var e = t.ToYieldInstruction();
        Assert.Throws<AssertionException>(() => t.ToYieldInstruction());
        t.Complete();
        yield return e;
        Assert.IsFalse(t.IsAlive);
        testCompletedCorEnumerator(e);
    }

    [UnityTest]
    public IEnumerator CoroutineEnumeratorUsingDead() {
        var t = Tween.Delay(0.01f);
        var e = t.ToYieldInstruction();
        yield return e;
        Assert.IsFalse(t.IsAlive);
        testCompletedCorEnumerator(e);
    }

    static void testCompletedCorEnumerator(IEnumerator e) {
        Assert.IsFalse(e.MoveNext());
        Assert.Throws<AssertionException>(() => {
            // ReSharper disable once UnusedVariable
            var cur = e.Current;
        });
        Assert.Throws<NotSupportedException>(() => e.Reset());
    }

    [UnityTest]
    public IEnumerator YieldInstructionsClash2() {
        var test = new GameObject().AddComponent<YieldInstructionsClash>();
        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        while (test != null) {
            yield return null;
        }
    }
    
    [UnityTest]
    public IEnumerator YieldInstructionsClash() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        {
            var t1 = Tween.Delay(0.00001f);
            int frameStart = Time.frameCount;
            yield return t1.ToYieldInstruction();
            Assert.AreEqual(1, Time.frameCount - frameStart);
            Assert.IsFalse(t1.IsAlive);
            var t2 = Tween.Delay(0.001f);
            t2.ToYieldInstruction();
            t2.Complete();
        }
        {
            var t1 = Tween.Delay(0.00001f);
            t1.ToYieldInstruction();
            yield return null;
            yield return null;
            Assert.IsFalse(t1.IsAlive);
            var t2 = Tween.Delay(0.001f);
            t2.ToYieldInstruction();
            t2.Complete();    
        }
    }
    
    [Test]
    public void TweenDuplicateInSequence() {
        var t1 = Tween.Delay(0.1f);
        var t2 = Tween.Delay(0.1f);
        var s = t1.Chain(t2);
        expectException<Exception>(() => s.Chain(t1), "A tween can be added to a sequence only once");
    }
    
    /*[UnityTest]
    public IEnumerator WaitStopFromValueChange2() {
        Tween t = default;
        t = Tween.WaitWhile(this, _ => {
            Assert.IsTrue(t.IsAlive);
            t.Stop();
            return true;
        });
        yield return t.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator WaitCompleteFromValueChange2() {
        Tween t = default;
        int numValueChanged = 0;
        t = Tween.WaitWhile(this, _ => {
            numValueChanged++;
            switch (numValueChanged) {
                case 1:
                    Assert.IsTrue(t.IsAlive);
                    t.Complete();
                    break;
                case 2:
                    Assert.IsFalse(t.IsAlive);
                    break;
                default: throw new Exception();
            }
            return true;
        });
        yield return t.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator WaitCompleteFromValueChange() {
        var target = new GameObject();
        int numValueChanged = 0;
        yield return Tween.WaitWhile(target, _ => {
            numValueChanged++;
            switch (numValueChanged) {
                case 1:
                    Assert.AreEqual(1, Tween.CompleteAll(target, 1, 1));
                    break;
                case 2:
                    Assert.AreEqual(0, Tween.CompleteAll(target, 0, 0));
                    break;
                default: throw new Exception();
            }
            return true;
        }).ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator WaitStopFromValueChange() {
        var target = new GameObject();
        yield return Tween.WaitWhile(target, _ => {
            Assert.AreEqual(1, Tween.StopAll(target, 1, 1));
            return true;
        }).ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator TweenWait() {
        var timeStart = Time.time;
        int numOnCompleteDone = 0;
        const float duration = 0.3f;
        var t = Tween.WaitWhile(this, _ => Time.time - timeStart < duration)
            .OnComplete(() => numOnCompleteDone++);
        yield return null;
        Assert.IsTrue(t.IsAlive);
        yield return new WaitForSeconds(duration + 0.01f);
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteDone);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void TweenWaitInstantComplete() {
        int numOnCompleteDone = 0;
        var t = Tween.WaitWhile(this, _ => true).OnComplete(() => numOnCompleteDone++);
        Assert.IsTrue(t.IsAlive);
        t.Complete();
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteDone);
    }
    
    [Test]
    public void TweenWaitInstantStop() {
        int numOnCompleteDone = 0;
        var t = Tween.WaitWhile(this, _ => true).OnComplete(() => numOnCompleteDone++);
        Assert.IsTrue(t.IsAlive);
        t.Stop();
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(0, numOnCompleteDone);
    }

    [Test]
    public void TweenWaitException() {
        var t = Tween.WaitWhile(this, _ => throw new Exception()).OnComplete(Assert.Fail);
        Assert.IsTrue(t.IsAlive);
        LogAssert.Expect(LogType.Error, new Regex("Tween was stopped because of exception"));
        LogAssert.Expect(LogType.Error, new Regex(Constants.onCompleteCallbackIgnored));
        t.Complete();
        Assert.IsFalse(t.IsAlive);
    }

    [UnityTest]
    public IEnumerator TweenWaitDuration() {
        // Application.targetFrameRate = 60;
        var t = Tween.WaitWhile(this, _ => true);
        validate();
        Assert.AreEqual(0, t.elapsedTime);
        Assert.AreEqual(0, t.elapsedTimeTotal);
        yield return null;
        Assert.AreNotEqual(0, t.elapsedTime);
        Assert.AreNotEqual(0, t.elapsedTimeTotal);
        // for (int i = 0; i < 60; i++) {
        //     yield return null;
        //     Debug.Log($"{t.elapsedTime}, {t.elapsedTimeTotal}");
        // }
        validate();
        t.Complete();
        void validate() {
            Assert.AreEqual(-1, t.cyclesTotal);
            Assert.AreEqual(0, t.cyclesDone);
            Assert.IsTrue(float.IsPositiveInfinity(t.duration));
            Assert.IsTrue(float.IsPositiveInfinity(t.durationTotal));
            Assert.AreEqual(0, t.progress);
            Assert.AreEqual(0, t.progressTotal);
            Assert.AreEqual(0, t.interpolationFactor);
        }
    }*/

    [UnityTest]
    public IEnumerator WaitDurationForOther() {
        var dur = Random.value;
        var t1 = Tween.Delay(dur);
        var t2 = Tween.waitFor(t1);
        Assert.AreEqual(dur, t1.duration);
        Assert.AreEqual(dur, t1.durationTotal);
        Assert.AreEqual(0, t2.duration);
        Assert.AreEqual(0, t2.durationTotal);
        Assert.AreEqual(dur, t2.tween.calcDurationWithWaitDependencies());
        Assert.IsTrue(t1.IsAlive);
        Assert.IsTrue(t2.IsAlive);
        yield return null;
        Assert.AreEqual(dur, t1.duration);
        Assert.AreEqual(dur, t1.durationTotal);
        Assert.AreEqual(0, t2.duration);
        Assert.AreEqual(0, t2.durationTotal);
        Assert.IsTrue(t1.IsAlive);
        Assert.IsTrue(t2.IsAlive);
        t1.Stop();
        Assert.IsFalse(t1.IsAlive);
        Assert.IsTrue(t2.IsAlive);
        yield return null;
        Assert.IsFalse(t2.IsAlive);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void ZeroDurationWarning() {
        var oldSetting = PrimeTweenConfig.warnZeroDuration;
        try {
            PrimeTweenConfig.warnZeroDuration = true;
            LogAssert.Expect(LogType.Warning, new Regex(nameof(PrimeTweenManager.warnZeroDuration)));
            Tween.Custom(this, 0, 1, 0, delegate{});
            PrimeTweenConfig.warnZeroDuration = false;
            Tween.Custom(this, 0, 1, 0, delegate{});
            LogAssert.NoUnexpectedReceived();
        } finally {
            PrimeTweenConfig.warnZeroDuration = oldSetting;
        }
    }
    
    [Test]
    public void CompleteTweenTwice() {
        int numCompleted = 0;
        var t = createCustomTween(1)
            .OnComplete(() => numCompleted++);
        t.Complete();
        Assert.AreEqual(1, numCompleted);
        t.Complete();
        Assert.AreEqual(1, numCompleted);
    }
    
    [UnityTest]
    public IEnumerator FromValueShouldNotChangeBetweenCycles() {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var boxCollider = cube.GetComponent<BoxCollider>(); // prevent Engine Code stripping
        Assert.IsNotNull(boxCollider);
        var origPos = new Vector3(10, 10, 10);
        cube.transform.position = origPos;
        var tween = Tween.Position(cube.transform, new Vector3(20, 10, 10), 0.01f, cycles: 5);
        Assert.IsTrue(tween.IsAlive);
        Assert.IsNotNull(tween.tween.getter);
        while (tween.IsAlive) {
            yield return null;
            Assert.AreEqual(origPos, tween.tween.startValue.Vector3Val, "'From' should not change after a cycle. This can happen if tween resets startFromCurrent after a cycle.");
        }
        Object.Destroy(cube);
    }

    [Test]
    public void SettingIsPausedOnTweenInSequenceDisplayError() {
        var target = new object();
        var t = Tween.Delay(target, 0.01f);
        Sequence.Create(t);
        expectError();
        t.IsPaused = true;

        expectError();
        Tween.SetPausedAll(true, target);

        expectError();
        Tween.SetPausedAll(false, target);

        void expectError() {
            LogAssert.Expect(LogType.Error, Constants.setPauseOnTweenInsideSequenceError);
        }
    }

    [Test]
    public void SettingCyclesOnDeadTweenDisplaysError() {
        var t = createTween();
        Assert.IsTrue(t.IsAlive);
        t.Complete();
        Assert.IsFalse(t.IsAlive);
        LogAssert.Expect(LogType.Error, new Regex(Constants.tweenIsDeadMessage));
        t.SetCycles(5);
    }

    [Test]
    public void TestDeadTween() {
        var t = createDeadTween();

        expectError();
        t.IsPaused = true;
        
        t.Stop();
        t.Complete();
        // t.Revert();
        
        expectError();
        t.SetCycles(10);
        
        expectError();
        t.OnComplete(delegate{});
        
        expectError();
        t.OnComplete(this, delegate { });

        void expectError() {
            LogAssert.Expect(LogType.Error, Constants.tweenIsDeadMessage);
        }
    }

    static Tween createDeadTween() {
        var t = createCustomTween(0.1f);
        t.Complete();
        Assert.IsFalse(t.IsAlive);
        return t;
    }
    
    [UnityTest]
    public IEnumerator TweenIsPaused() {
        var val = 0f;
        var t = Tween.Custom(this, 0, 1, 1, (_, newVal) => {
            val = newVal;
        });
        t.IsPaused = true;
        yield return null;
        Assert.AreEqual(0, val);
        yield return null;
        Assert.AreEqual(0, val);
        yield return null;
        Assert.AreEqual(0, val);
        t.IsPaused = false;
        yield return null;
        Assert.AreNotEqual(0, val);
    }
    
    [UnityTest]
    public IEnumerator SequenceIsPaused() {
        var val = 0f;
        var t = Tween.Custom(this, 0, 1, 1, (_, newVal) => {
            val = newVal;
        });
        var s = Sequence.Create(t);
        s.IsPaused = true;
        yield return null;
        Assert.AreEqual(0, val);
        yield return null;
        Assert.AreEqual(0, val);
        yield return null;
        Assert.AreEqual(0, val);
        s.IsPaused = false;
        yield return null;
        Assert.AreNotEqual(0, val);
    }

    const int capacityForTest = 100;
    
    [UnityTest]
    public IEnumerator TweensCapacity() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, tweens.Count);
        PrimeTweenConfig.SetTweensCapacity(capacityForTest);
        Assert.AreEqual(capacityForTest, tweens.Capacity);
        PrimeTweenConfig.SetTweensCapacity(0);
        Assert.AreEqual(0, tweens.Capacity);
        LogAssert.Expect(LogType.Warning, new Regex("Please increase the capacity"));
        Tween.Delay(0.0001f);
        Tween.Delay(0.0001f); // created before set capacity
        PrimeTweenConfig.SetTweensCapacity(capacityForTest);
        Assert.AreEqual(capacityForTest, tweens.Capacity);
        var delay = Tween.Delay(0.0001f);
        yield return delay.ToYieldInstruction(); // should not display warning
        Assert.IsFalse(delay.IsAlive);
        yield return null; // the yielded tween is not yet released when the coroutine completes. The release will happen only in a frame
        Assert.AreEqual(0, tweens.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator ListResize() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        var list = new List<ReusableTween>();
        test(2, 2);
        Assert.AreNotEqual(list[0], list[1]);
        test(0, 2);
        test(10, 10);
        test(2, 5);
        test(10, 20);
        test(5,  30);
        test(6, 29);
        test(4, 31);
        test(5, 32);
        test(5, 31);
        test(4, 31);
        test(3, 31);
        test(0, 31);
        test(31, 31);
        Assert.Throws<AssertionException>(() => test(32, 31));
        test(0, 0);
        test(10, 10);
        void test(int newCount, int newCapacity) {
            PrimeTweenManager.resizeAndSetCapacity(list, newCount, newCapacity);
            Assert.AreEqual(newCount, list.Count);
            Assert.AreEqual(newCapacity, list.Capacity);
            
            PrimeTweenConfig.SetTweensCapacity(newCapacity);
            Assert.AreEqual(newCapacity, PrimeTweenManager.Instance.buffer.Capacity);
            Assert.AreEqual(newCapacity, PrimeTweenManager.Instance.tweens.Capacity);
        }
    }

    static ShakeSettings shakeSettings {
        get {
            if (Random.value < 0.5f) {
                return new ShakeSettings(Vector3.one, 1f, 10f, false);
            }
            return new ShakeSettings(Vector3.one, 1f, 10f, false, Ease.Linear);
        }
    }

    [UnityTest]
    public IEnumerator ShakeCompleteIfStartDelayIsNotElapsed() {
        var target = new GameObject(nameof(ShakeCompleteIfStartDelayIsNotElapsed)).transform;
        var iniPos = Random.value * Vector3.one;
        target.localPosition = iniPos;
        var t = Tween.ShakeLocalPosition(target, Vector3.one, 0.1f, startDelay: 0.1f);
        yield return null;
        Assert.AreEqual(iniPos, target.localPosition);
        t.Complete();
        Assert.AreEqual(iniPos, target.localPosition);
    }

    [UnityTest]
    public IEnumerator ShakeLocalScale() {
        var shakeTransform = new GameObject("shake target").transform;
        shakeTransform.position = Vector3.one;
        Assert.AreEqual(shakeTransform.localScale, Vector3.one);
        var t = Tween.ShakeLocalScale(shakeTransform, shakeSettings);
        yield return null;
        Assert.AreNotEqual(shakeTransform.localScale, Vector3.one);
        t.Complete();
        Assert.IsTrue(shakeTransform.localScale == Vector3.one);
        Object.DestroyImmediate(shakeTransform.gameObject);
    }

    [UnityTest]
    public IEnumerator ShakeLocalRotation() {
        var shakeTransform = new GameObject("shake target").transform;
        shakeTransform.position = Vector3.one;
        Assert.AreEqual(shakeTransform.localRotation, Quaternion.identity);
        var t = Tween.ShakeLocalRotation(shakeTransform, shakeSettings);
        yield return null;
        Assert.AreNotEqual(shakeTransform.localRotation, Quaternion.identity);
        t.Complete();
        Assert.IsTrue(shakeTransform.localRotation == Quaternion.identity);
        Object.DestroyImmediate(shakeTransform.gameObject);
    }

    [UnityTest]
    public IEnumerator ShakeLocalPosition() {
        var shakeTransform = new GameObject("shake target").transform;
        shakeTransform.position = Vector3.one;
        Assert.AreEqual(shakeTransform.position, Vector3.one);
        var t = Tween.ShakeLocalPosition(shakeTransform, shakeSettings);
        yield return null;
        Assert.AreNotEqual(shakeTransform.position, Vector3.one);
        t.Complete();
        Assert.IsTrue(shakeTransform.position == Vector3.one, shakeTransform.position.ToString());
        Object.DestroyImmediate(shakeTransform.gameObject);
    }

    [UnityTest]
    public IEnumerator ShakeCustom() {
        var shakeTransform = new GameObject("shake target").transform;
        var iniPos = Vector3.one;
        shakeTransform.position = iniPos;
        Assert.AreEqual(iniPos, shakeTransform.position);
        var t = Tween.ShakeCustom(shakeTransform, iniPos, shakeSettings, (target, val) => target.localPosition = val);
        yield return null;
        Assert.AreNotEqual(iniPos, shakeTransform.position);
        t.Complete();
        Assert.IsTrue(iniPos == shakeTransform.position, iniPos.ToString());
    }

    [UnityTest]
    public IEnumerator CreateShakeWhenTweenListHasNull() {
        Tween.StopAll();
        yield return null;
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        Tween.Delay(0.0001f);
        LogAssert.Expect(LogType.Warning, "Shake's frequency is 0.");
        LogAssert.Expect(LogType.Warning, "Shake's strength is (0, 0, 0).");
        Tween.Delay(0.0001f)
            .OnComplete(() => {
                Assert.AreEqual(1, tweens.Count(_ => _ == null));
                Tween.ShakeLocalPosition(transform, default).Complete();
            });
        yield return null;
        yield return null;
        yield return null;
        Assert.AreEqual(0, tweens.Count);
    }
    
    [UnityTest]
    public IEnumerator DelayNoTarget() {
        int numCallbackCalled = 0;
        var t = Tween.Delay(0.1f, () => numCallbackCalled++);
        Assert.AreEqual(0, numCallbackCalled);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.AreEqual(1, numCallbackCalled);
    }
    
    [UnityTest]
    public IEnumerator DelayFirstOverload() {
        int numCallbackCalled = 0;
        var t = Tween.Delay(this, 0.1f, () => numCallbackCalled++);
        Assert.AreEqual(0, numCallbackCalled);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.AreEqual(1, numCallbackCalled);
    }
    
    [UnityTest]
    public IEnumerator DelaySecondOverload() {
        int numCallbackCalled = 0;
        var t = Tween.Delay(this, 0.1f, _ => numCallbackCalled++);
        Assert.AreEqual(0, numCallbackCalled);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.AreEqual(1, numCallbackCalled);
    }

    [UnityTest]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public IEnumerator NewTweenCreatedFromManualOnComplete() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, tweens.Count);
        var t1 = createTween().OnComplete(() => createTween());
        createTween();
        t1.Complete();
        Assert.AreEqual(3, tweens.Count);
        Assert.IsTrue(tweens.OrderBy(_ => _.id).SequenceEqual(tweens));
        yield return null;
        Assert.AreEqual(2, tweens.Count);
        Assert.IsTrue(tweens.OrderBy(_ => _.id).SequenceEqual(tweens));
    }

    [UnityTest]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public IEnumerator NewTweenCreatedFromNormalOnComplete() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, tweens.Count);
        int numOnCompleteCalled = 0;
        var t1 = createCustomTween(0.01f).OnComplete(() => {
            numOnCompleteCalled++;
            createCustomTween(0.01f);
            createCustomTween(0.01f);
            createCustomTween(0.01f);
        });
        Assert.AreEqual(1, tweens.Count);
        Assert.IsTrue(tweens.OrderBy(_ => _.id).SequenceEqual(tweens));
        while (t1.IsAlive) {
            yield return null;
        }
        Assert.AreEqual(1, numOnCompleteCalled);
        Assert.AreEqual(3, tweens.Count);
        Assert.IsTrue(tweens.OrderBy(_ => _.id).SequenceEqual(tweens));
    }

    [UnityTest]
    public IEnumerator SetAllPaused() {
        var instanceTweens = PrimeTweenManager.Instance.tweens;
        if (instanceTweens.Any()) {
            var aliveCount = instanceTweens.Count(_ => _ != null && _.isAlive);
            Tween.StopAll(null, aliveCount, aliveCount);
            yield return null;
        }
        Assert.AreEqual(0, instanceTweens.Count);
        const int count = 10;
        var tweens = new List<Tween>(); 
        for (int i = 0; i < count; i++) {
            tweens.Add(createCustomTween(1));
        }
        Assert.IsTrue(tweens.All(_ => !_.IsPaused));
        Tween.SetPausedAll(true, null, count, count);
        Assert.IsTrue(tweens.All(_ => _.IsPaused));
        Tween.SetPausedAll(false, null, count, count);
        Assert.IsTrue(tweens.All(_ => !_.IsPaused));
        Assert.IsTrue(tweens.All(_ => _.IsAlive));
        yield return null;
        Assert.IsTrue(tweens.All(_ => _.IsAlive));
        foreach (var _ in tweens) {
            _.Complete();
        }
        Assert.IsFalse(tweens.All(_ => _.IsAlive));
    }

    [UnityTest]
    public IEnumerator StopAllCalledFromOnValueChange() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        int numOnValueChangeCalled = 0;
        var t = Tween.Custom(this, 0, 1, 1f, delegate {
            Assert.AreEqual(0, numOnValueChangeCalled);
            numOnValueChangeCalled++;
            var numStopped = Tween.StopAll(this, 1, 1);
            Assert.AreEqual(1, numStopped);
        });
        Assert.IsTrue(t.IsAlive);
        yield return null;
        yield return null;
        Assert.IsFalse(t.IsAlive);
        LogAssert.NoUnexpectedReceived();
    }
    
    [UnityTest]
    public IEnumerator RecursiveCompleteCallFromOnValueChange() {
        int numOnValueChangeCalled = 0;
        int numOnCompleteCalled = 0;
        Tween t = default;
        t = Tween.Custom(this, 0, 1, 1f, delegate {
            // Debug.Log(val);
            numOnValueChangeCalled++;
            Assert.IsTrue(numOnValueChangeCalled <= 2);
            t.Complete();
        }).OnComplete(() => numOnCompleteCalled++);
        Assert.IsTrue(t.IsAlive);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteCalled);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator RecursiveCompleteAllCallFromOnValueChange() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        int numOnValueChangeCalled = 0;
        int numOnCompleteCalled = 0;
        var t = Tween.Custom(this, 0, 1, 1f, delegate {
            // Debug.Log(val);
            numOnValueChangeCalled++;
            switch (numOnValueChangeCalled) {
                case 1: {
                    var numCompleted = Tween.CompleteAll(this, 1, 1);
                    Assert.AreEqual(1, numCompleted);
                    break;
                }
                case 2: {
                    var numCompleted = Tween.CompleteAll(this, 0, 0);
                    Assert.AreEqual(0, numCompleted);
                    break;
                }
                default:
                    throw new Exception();
            }
        }).OnComplete(() => numOnCompleteCalled++);
        Assert.IsTrue(t.IsAlive);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteCalled);
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        Assert.AreEqual(2, numOnValueChangeCalled);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator RecursiveCompleteCallFromOnComplete() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        int numOnCompleteCalled = 0;
        Tween t = default;
        t = Tween.Custom(this, 0, 1, 0.00001f, delegate {
        }).OnComplete(() => {
            numOnCompleteCalled++;
            t.Complete();
        });
        Assert.IsTrue(t.IsAlive);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteCalled);
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
    }

    [UnityTest]
    public IEnumerator RecursiveCompleteAllCallFromOnComplete() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        int numOnCompleteCalled = 0;
        var t = Tween.Custom(this, 0, 1, 0.00001f, delegate {
        }).OnComplete(() => {
            numOnCompleteCalled++;
            var numCompleted = Tween.CompleteAll(this, 0, 0);
            Assert.AreEqual(0, numCompleted);
        });
        Assert.IsTrue(t.IsAlive);
        while (t.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnCompleteCalled);
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
    }

    [UnityTest]
    public IEnumerator StopAllCalledFromOnValueChange2() {
        int numOnValChangedCalled = 0;
        var t = Tween.Custom(this, 0, 1, 0.0001f, (_, val) => {
            // Debug.Log(val);
            Assert.AreEqual(0, val);
            Assert.AreEqual(0, numOnValChangedCalled);
            numOnValChangedCalled++;
            var numStopped = Tween.StopAll(this, 1, 1);
            Assert.AreEqual(1, numStopped);
        });
        Assert.IsTrue(t.IsAlive);
        yield return null;
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnValChangedCalled);
    }

    [UnityTest]
    public IEnumerator TweenCanBeNullInProcessAllMethod() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        Tween.Custom(this, 0, 1, 0.0001f, delegate {
            // Debug.Log($"t1 val {val}");
        });
        Tween.Custom(this, 0, 1, 0.0001f, delegate {
            // Debug.Log($"t2 val {val}");
            Assert.AreEqual(0, tweens.Count(_ => _ == null));
            Tween.StopAll(this, 2, 2);
        });
        yield return null;
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TweenCanBeNullInOnComplete() {
        Tween.StopAll();
        yield return null;
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        int numOnCompleteCalled = 0;
        Tween.Custom(this, 0, 1, 0.0001f, delegate{});
        Tween.Custom(this, 0, 1, 0.0001f, delegate {
        }).OnComplete(() => {
            numOnCompleteCalled++;
            Assert.AreEqual(1, tweens.Count(_ => _ == null));
            var numStopped = Tween.StopAll(this, 0, 0);
            Assert.AreEqual(0, numStopped);
        });
        yield return null;
        Assert.AreEqual(1, numOnCompleteCalled);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TweenShouldBeDeadInOnValueChangeAfterCallingComplete() {
        // Debug.Log(nameof(TweenShouldBeDeadInOnValueChangeAfterCallingComplete));
        var target = new GameObject(nameof(TweenShouldBeDeadInOnValueChangeAfterCallingComplete));
        int numOnValueChangeCalled = 0;
        Tween t = default;
        t = Tween.Custom(target, 0, 1, 0.00001f, (_, val) => {
            // Debug.Log(val);
            Assert.IsTrue(val == 0 || val == 1);
            numOnValueChangeCalled++;
            switch (numOnValueChangeCalled) {
                case 1:
                    Assert.IsTrue(t.IsAlive);
                    if (Random.value < 0.5f) {
                        t.Complete();
                    } else {
                        Tween.CompleteAll(target, 1, 1);
                    }
                    break;
                case 2:
                    // when Complete() is called, it's expected that onValueChange will be reported once again
                    break;
                default: throw new Exception();
            }
        });
        Assert.AreEqual(1, Tween.SetPausedAll(true, target, 1, 1));
        Assert.AreEqual(1, Tween.SetPausedAll(false, target, 1, 1));
        yield return null;
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(2, numOnValueChangeCalled);
    }

    [UnityTest]
    public IEnumerator NumProcessed() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        var target1 = new object();
        var target2 = new object();

        createWithTarget1(); // 1
        createWithTarget1(); // 2
        createWithTarget1(); // 3
        createWithTarget2();   // 1
        createWithTarget2();   // 2
        createWithTarget1(); // 4
        createWithTarget2();   // 3
        
        Assert.AreEqual(4, Tween.SetPausedAll(true, target1, 4, 4));
        Assert.AreEqual(4, Tween.SetPausedAll(false, target1, 4, 4));
        Assert.AreEqual(4, Tween.StopAll(target1, 4, 4));
        Assert.AreEqual(0, Tween.StopAll(target1, 0, 0));
        Assert.AreEqual(0, Tween.CompleteAll(target1, 0, 0));
        Assert.AreEqual(0, Tween.SetPausedAll(true, target1, 0, 0));
        
        Assert.AreEqual(3, Tween.SetPausedAll(true, target2, 3, 3));
        Assert.AreEqual(3, Tween.SetPausedAll(false, target2, 3, 3));
        Assert.AreEqual(3, Tween.CompleteAll(target2, 3, 3));
        Assert.AreEqual(0, Tween.CompleteAll(target2, 0, 0));
        Assert.AreEqual(0, Tween.StopAll(target2, 0, 0));
        
        void createWithTarget1() => Tween.Custom(target1, 0, 1, 0.0001f, delegate { });
        void createWithTarget2() => Tween.Custom(target2, 0, 1, 0.0001f, delegate { });
    }

    [UnityTest]
    public IEnumerator TweenIsAliveForWholeDuration() {
        int numOnValueChangedCalled = 0;
        int numOnValueChangedCalledAfterComplete = 0;
        Tween t = default;
        var target = new object();
        bool isCompleteCalled = false;
        const float duration = 0.3f;
        t = Tween.Custom(target, 0, 1, duration, (_, val) => {
            // Debug.Log(val);
            numOnValueChangedCalled++;
            if (isCompleteCalled) {
                numOnValueChangedCalledAfterComplete++;
            }
            Assert.AreEqual(!isCompleteCalled, t.IsAlive);
            if (val > duration / 2) {
                isCompleteCalled = true;
                t.Complete();
            }
        }).OnComplete(() => {
            Assert.IsTrue(t.IsCreated);
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(0, Tween.StopAll(target, 0, 0));
        });
        while (t.IsAlive) {
            yield return null;
        }
        Assert.IsTrue(numOnValueChangedCalled > 1);
        Assert.AreEqual(1, numOnValueChangedCalledAfterComplete);
    }

    [Test]
    public void SetPauseAll() {
        var target = new object();
        var t = Tween.Custom(target, 0, 1, 1, delegate{});
        Assert.AreEqual(0, Tween.SetPausedAll(false, target, 0, 0));
        Assert.AreEqual(1, Tween.SetPausedAll(true, target, 1, 1));
        Assert.AreEqual(0, Tween.SetPausedAll(true, target, 0, 0));
        Assert.AreEqual(1, Tween.SetPausedAll(false, target, 1, 1));
        Assert.AreEqual(0, Tween.SetPausedAll(false, target, 0, 0));
        t.Stop();
        Assert.AreEqual(0, Tween.SetPausedAll(true, target, 0, 0));
    }

    [UnityTest]
    public IEnumerator StopByTargetFromOnValueChange() {
        var target = new GameObject();
        int numOnValueChangeCalled = 0;
        var t = Tween.Custom(target, 0, 1, 1, delegate {
            numOnValueChangeCalled++;
            var numStopped = Tween.StopAll(target, 1, 1);
            Assert.AreEqual(1, numStopped);
        });
        Assert.AreEqual(0, numOnValueChangeCalled);
        Assert.IsTrue(t.IsAlive);
        yield return null;
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnValueChangeCalled);
    }
    
    [UnityTest]
    public IEnumerator TweenPropertiesDefault() {
        var tweens = PrimeTweenManager.Instance.tweens;
        if (tweens.Any()) {
            Tween.StopAll();
            yield return null;
            Assert.AreEqual(0, tweens.Count);
        }
        {
            yield return Tween.Delay(0.001f).ToYieldInstruction();
            Assert.AreEqual(0, tweens.Count);
            yield return null;
            Assert.AreEqual(0, tweens.Count);
        }
        {
            var t = Tween.Delay(0f);
            Assert.IsTrue(t.IsAlive);
            validate(t);
        }
        {
            var t = new Tween();
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(0, t.cyclesTotal);
            validate(t);
        }
        {
            var t = Tween.Delay(1);
            t.Complete();
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(0, t.cyclesTotal);
            validate(t);
        }
        {
            var t = Tween.Delay(1);
            t.Stop();
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(0, t.cyclesTotal);
            validate(t);
        }
        void validate(Tween t) {
            Assert.AreEqual(0, t.elapsedTime);
            Assert.AreEqual(0, t.elapsedTimeTotal);
            Assert.AreEqual(0, t.cyclesDone);
            Assert.AreEqual(0, t.duration);
            Assert.AreEqual(0, t.durationTotal);

            Assert.AreEqual(0, t.progress);
            Assert.AreEqual(0, t.progressTotal);
            Assert.AreEqual(0, t.interpolationFactor);
        }
        {
            const float duration = 0.123f;
            var t = Tween.PositionY(transform, 0, duration, Ease.Linear, -1);
            Assert.AreEqual(duration, t.duration);
            Assert.IsTrue(float.IsPositiveInfinity(t.durationTotal));
            Assert.AreEqual(0, t.progress);
            Assert.AreEqual(0, t.progressTotal);
            t.Stop();
            validate(t);
        }
    }

    [UnityTest]
    public IEnumerator TweenProperties() {
        float duration = 0.1f * Random.value;
        int numCyclesExpected = Random.Range(1, 4);
        int numCyclesDone = 0;
        Tween t = default;
        float startDelay = 0.1f * Random.value;
        float endDelay = 0.1f * Random.value;
        float durationExpected = startDelay + duration + endDelay;
        float totalDurationExpected = durationExpected * numCyclesExpected;
        t = Tween.Custom(this, 1f, 2f, duration, ease: Ease.Linear, cycles: numCyclesExpected, startDelay: startDelay, endDelay: endDelay, onValueChange: (_, val) => {
            val -= 1f;
            var elapsedTimeExpected = startDelay + val * duration;
            var elapsedTimeTotalExpected = elapsedTimeExpected + durationExpected * numCyclesDone;
            // Debug.Log($"val: {val}, progress: {t.progress}, elapsedTimeExpected: {elapsedTimeExpected}, elapsedTimeTotalExpected: {elapsedTimeTotalExpected}");
            var tolerance = 0.001f;
            if (val < 1) {
                Assert.AreEqual(elapsedTimeExpected, t.elapsedTime, tolerance);
                Assert.AreEqual(elapsedTimeTotalExpected, t.elapsedTimeTotal, tolerance, $"val: {val},duration: {duration}, numCyclesExpected: {numCyclesExpected}");
                Assert.AreEqual(Mathf.Min(elapsedTimeTotalExpected / totalDurationExpected, 1f), t.progressTotal, tolerance);
                Assert.AreEqual(elapsedTimeExpected / durationExpected, t.progress, tolerance);
            }
            Assert.AreEqual(numCyclesExpected, t.cyclesTotal);
            Assert.AreEqual(numCyclesDone, t.cyclesDone);
            Assert.AreEqual(durationExpected, t.duration);
            Assert.AreEqual(totalDurationExpected, t.durationTotal);
            if (val == 1) {
                numCyclesDone++;
            }
            Assert.AreEqual(t.interpolationFactor, val, tolerance);
        });
        yield return t.ToYieldInstruction();
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(numCyclesExpected, numCyclesDone);
        Assert.AreEqual(0, t.progress);
        Assert.AreEqual(0, t.progressTotal);

        var infT = Tween.Position(transform, Vector3.one, 0.00001f, cycles: -1);
        Assert.IsTrue(infT.IsAlive);
        Assert.AreEqual(-1, infT.cyclesTotal);
        infT.Complete();
    }

    [UnityTest]
    public IEnumerator ZeroDurationOnTweenShouldReportValueAtLeastOnce() {
        Tween.StopAll();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
        Assert.AreEqual(capacityForTest, PrimeTweenManager.Instance.tweens.Capacity);

        const float p1 = 0.345f;
        Tween.PositionZ(transform, 0, p1, 0f).Complete();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p1, transform.position.z);
        
        const float p2 = 0.123f;
        Tween.PositionZ(transform, p2, 0).Complete();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p2, transform.position.z);
        
        const float p3 = 0.456f;
        Tween.PositionZ(transform, p3, 0);
        yield return null;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p3, transform.position.z);

        yield return Tween.PositionZ(transform, p1, 0).OnComplete(() => { }).ToYieldInstruction();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p1, transform.position.z);
    }

    [UnityTest]
    public IEnumerator OneShouldBeReportedExactlyOnce() {
        int numOneReported = 0;
        const int cycles = 3;
        yield return Tween.Custom(this, 0, 1, 0.1f, startDelay: 0.01f, endDelay: 0.01f, cycles: cycles, onValueChange: (_, val) => {
            if (val == 1) {
                numOneReported++;
            }
        }).ToYieldInstruction();
        Assert.AreEqual(cycles, numOneReported, "This test may fail on high fps, like 1000.");
        
        numOneReported = 0;
        yield return Tween.Custom(this, 0, 1, 0f, startDelay: 0.01f, endDelay: 0.01f, cycles: cycles, onValueChange: (_, val) => {
            if (val == 1) {
                numOneReported++;
            }
        }).ToYieldInstruction();
        Assert.AreEqual(cycles, numOneReported);
        
        numOneReported = 0;
        yield return Tween.Custom(this, 0, 1, 0f, cycles: cycles, onValueChange: (_, val) => {
            if (val == 1) {
                numOneReported++;
            }
        }).ToYieldInstruction();
        Assert.AreEqual(cycles, numOneReported);
        
        numOneReported = 0;
        yield return Tween.Custom(this, 0, 1, 0f, (_, val) => {
            if (val == 1) {
                numOneReported++;
            }
        }).ToYieldInstruction();
        Assert.AreEqual(1, numOneReported);
        
        yield return Tween.PositionY(transform, 3.14f, 0.01f).ToYieldInstruction();
        yield return Tween.PositionY(transform, 3.14f, 0f).ToYieldInstruction();
        yield return Tween.PositionY(transform, 0, 3.14f, 0f).ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator SingleFrameTween() {
        int numOnValueChangeCalled = 0;
        yield return Tween.Custom(this, 0, 1, 0.0001f, (_, val) => {
            numOnValueChangeCalled++;
            Assert.IsTrue(val == 0 || val == 1);
        }).ToYieldInstruction();
        Assert.AreEqual(2, numOnValueChangeCalled);
    }

    [UnityTest]
    public IEnumerator TweensWithDurationOfDeltaTime() {
        var go = new GameObject();
        go.AddComponent<TweensWithDurationOfDeltaTime>();
        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        while (go != null) {
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator TweenWithExactDurationOfDeltaTime1() {
        yield return Tween.Delay(this, Time.deltaTime).ToYieldInstruction();
    }
    
    [UnityTest]
    public IEnumerator TweenWithExactDurationOfDeltaTime2() {
        int numOnCompleteCalled = 0;
        yield return Tween.Delay(this, Time.deltaTime, () => numOnCompleteCalled++).ToYieldInstruction();
        Assert.AreEqual(1, numOnCompleteCalled);
    }

    [Test]
    public void TotalDurationWithCycles() {
        var duration = Random.value;
        var startDelay = Random.value;
        var endDelay = Random.value;
        var cycles = Random.Range(1, 10);
        var t = Tween.LocalPositionY(transform, new TweenSettings<float>(0, 1, duration, cycles: cycles, startDelay: startDelay, endDelay: endDelay));
        var durationTotalExpected = duration + startDelay + endDelay;
        Assert.AreEqual(durationTotalExpected, t.duration);
        Assert.AreEqual(durationTotalExpected * cycles, t.durationTotal);
        Assert.AreEqual(durationTotalExpected * cycles, t.durationTotal);
        t.Complete();
    }
    
    [Test]
    public void DurationWithWaitDependencies() {
        var t1Dur = Random.value;
        var t1Cycles = Random.Range(1, 20);
        var t2Dur = Random.value;
        var t2Cycles = Random.Range(1, 20);
        // var t1Dur = 1;
        // var t1Cycles = 2;
        // var t2Dur = 2;
        // var t2Cycles = 2;
        var t1 = Tween.LocalPositionX(transform, 1, t1Dur, cycles: t1Cycles);
        var t2 = Tween.LocalPositionX(transform, 1, t2Dur, cycles: t2Cycles);
        var s = t1.Chain(t2);
        Assert.IsTrue(t1.IsAlive);
        Assert.IsTrue(t2.IsAlive);
        Assert.AreEqual(t1Dur * t1Cycles, t1.durationTotal);
        Assert.AreEqual(t2Dur * t2Cycles, t2.durationTotal);
        Assert.AreEqual(t1Dur * t1Cycles, t1.tween.calcDurationWithWaitDependencies());
        Assert.AreEqual(t1Dur * t1Cycles + t2Dur * t2Cycles, t2.tween.calcDurationWithWaitDependencies(), 0.001f);
        s.Complete();
    }

    
    [Test]
    public void AwaitingDeadCompletesImmediately() {
        bool isCompleted = false;
        AwaitingDeadCompletesImmediatelyAsync(() => isCompleted = true);
        Assert.IsTrue(isCompleted);
    }

    static async void AwaitingDeadCompletesImmediatelyAsync([NotNull] Action callback) {
        var frame = Time.frameCount;
        await new Tween();
        await new Sequence();
        Assert.AreEqual(frame, Time.frameCount);
        callback();
    }

    [UnityTest]
    public IEnumerator TestAwaitByCallback() {
        bool isCompleted = false;
        var t = Tween.Delay(0.1f);
        waitForTweenAsync(t, () => isCompleted = true);
        Assert.IsFalse(isCompleted);
        yield return null;
        Assert.IsFalse(isCompleted);
        yield return t.ToYieldInstruction();
        Assert.IsTrue(isCompleted);
    }

    static async void waitForTweenAsync(Tween tween, [NotNull] Action callback) {
        await tween;
        callback();
    }
    
    [Test]
    public async Task AwaitTweenWithCallback() {
        bool isCompleted = false;
        var t = Tween.Delay(0.1f,() => isCompleted = true);
        Assert.IsTrue(t.IsAlive);
        Assert.IsTrue(t.tween.HasOnComplete);
        await t;
        Assert.IsFalse(t.IsAlive);
        Assert.IsTrue(isCompleted);
    }
    
    [Test]
    public async Task AwaitTweenWithCallbackDoesntPostpone() {
        bool isCompleted = false;
        var t = Tween.Delay(0.00001f,() => isCompleted = true);
        Assert.IsTrue(t.IsAlive);
        Assert.IsTrue(t.tween.HasOnComplete);
        var frameStart = Time.frameCount;
        await t;
        Assert.AreEqual(1, Time.frameCount - frameStart);
        Assert.IsFalse(t.IsAlive);
        Assert.IsTrue(isCompleted);
    }
    
    [Test]
    public async Task AwaitTweenWithoutCallback() {
        bool isCompleted = false;
        var t = Tween.Delay(0.1f,() => isCompleted = true);
        var toAwait = Tween.waitFor(t);
        Assert.IsFalse(toAwait.tween.HasOnComplete);
        var frameStart = Time.frameCount;
        await toAwait;
        Assert.IsTrue(Time.frameCount - frameStart >= 1);
        Assert.IsTrue(isCompleted);
    }

    [Test]
    public async Task AwaitTweenWithoutCallbackDoesntPostpone() {
        bool isCompleted = false;
        var t = Tween.Delay(0.00001f,() => isCompleted = true);
        var toAwait = Tween.waitFor(t);
        Assert.IsFalse(toAwait.tween.HasOnComplete);
        var frameStart = Time.frameCount;
        await toAwait;
        Assert.AreEqual(1, Time.frameCount - frameStart);
        Assert.IsTrue(isCompleted);
    }

    [Test]
    public async Task AwaitSequence() {
        bool isCompleted1 = false;
        bool isCompleted2 = false;
        await Sequence.Create(Tween.Delay(0.01f, () => isCompleted1 = true))
            .Chain(Tween.Delay(0.02f, () => isCompleted2 = true));
        Assert.IsTrue(isCompleted1);
        Assert.IsTrue(isCompleted2);
    }

    [Test]
    public async Task AwaitSequence2() {
        var t1 = Tween.Delay(0.01f);
        var t2 = Tween.Delay(0.1f);
        await t1.Chain(t2);
        Assert.IsFalse(t1.IsAlive);
        Assert.IsFalse(t2.IsAlive);
    }

    [UnityTest]
    public IEnumerator ToYieldInstruction() {
        var t = Tween.Delay(0.1f);
        var e = t.ToYieldInstruction();
        var frameStart = Time.frameCount;
        while (e.MoveNext()) {
            yield return e.Current;
            t.Complete();
        }
        Assert.AreEqual(1, Time.frameCount - frameStart);
        Assert.IsFalse(t.IsAlive);
        yield return t.ToYieldInstruction();
        
        Tween defaultTween = default;
        defaultTween.ToYieldInstruction();

        Sequence defaultSequence = default;
        defaultSequence.ToYieldInstruction();
        
        t.Complete();
    }

    [UnityTest]
    public IEnumerator ImplicitConversionToIterator() {
        {
            var t2 = Tween.Delay(0.0001f);
            var frameStart = Time.frameCount;
            yield return t2;
            Assert.AreEqual(1, Time.frameCount - frameStart);
            Assert.IsFalse(t2.IsAlive);
        }
        {
            var s = Sequence.Create(Tween.Delay(0.0001f));
            var frameStart = Time.frameCount;
            yield return s;
            Assert.AreEqual(1, Time.frameCount - frameStart);
            Assert.IsFalse(s.IsAlive);
        }
    }

    [Test]
    public async Task AwaitInfiniteTweenComplete() {
        Tween t = default;
        int numCompleted = 0;
        t = Tween.Custom(this, 0, 1, 1, cycles: -1, onValueChange: delegate { t.Complete(); })
            .OnComplete(() => numCompleted++);
        await t;
        Assert.AreEqual(1, numCompleted);
    }

    [Test]
    public async Task AwaitInfiniteTweenStop() {
        Tween t = default;
        int numOnValueChanged = 0;
        t = Tween.Custom(this, 0, 1, 1f, cycles: -1, onValueChange: delegate {
            // Debug.Log(numOnValueChanged);
            numOnValueChanged++;
            Assert.AreEqual(1, numOnValueChanged);
            Assert.IsTrue(t.IsAlive);
            t.Stop();
        });
        Assert.IsTrue(t.IsAlive);
        await t;
        Assert.IsFalse(t.IsAlive);
        Assert.AreEqual(1, numOnValueChanged);
    }

    [Test]
    public async Task TweenStoppedTweenWhileAwaiting() {
        var t = Tween.Delay(0.05f);
        #pragma warning disable CS4014
        Tween.Delay(0.01f).OnComplete(() => t.Stop());
        #pragma warning restore CS4014
        Assert.IsTrue(t.IsAlive);
        await t;
        Assert.IsFalse(t.IsAlive);
    }

    [Test]
    public void InvalidDurations() {
        Assert.Throws<AssertionException>(() => Tween.Delay(float.NaN), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.Delay(float.PositiveInfinity), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.Delay(float.NegativeInfinity), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, startDelay: float.NaN))), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, startDelay: float.PositiveInfinity))), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, startDelay: float.NegativeInfinity))), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, endDelay: float.NaN))), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, endDelay: float.PositiveInfinity))), Constants.durationInvalidError);
        Assert.Throws<AssertionException>(() => Tween.PositionZ(transform, new TweenSettings<float>(0, 1, new TweenSettings(1, endDelay: float.NegativeInfinity))), Constants.durationInvalidError);
    }

    [Test]
    public void MaterialTweens() {
        {
            var s = Shader.Find("Standard");
            if (s == null) {
                Assert.Ignore();
                return;
            }
            var m = new Material(s);

            {
                const string propName = "_EmissionColor";
                #if UNITY_2021_1_OR_NEWER
                Assert.IsTrue(m.HasColor(propName));
                #endif
                var to = Color.red;
                Tween.MaterialColor(m, Shader.PropertyToID(propName), to, 1f).Complete();
                Assert.AreEqual(to, m.GetColor(propName));
            }
            {
                const string propName = "_EmissionColor";
                #if UNITY_2021_1_OR_NEWER
                Assert.IsTrue(m.HasColor(propName));
                #endif
                var iniColor = new Color(Random.value, Random.value, Random.value, Random.value);
                m.SetColor(propName, iniColor);
                var toAlpha = Random.value;
                Tween.MaterialAlpha(m, Shader.PropertyToID(propName), toAlpha, 1f).Complete();
                var col = m.GetColor(propName);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(col.r, iniColor.r);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(col.g, iniColor.g);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(col.b, iniColor.b);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(col.a, toAlpha);
            }
            {
                const string propName = "_Cutoff";
                #if UNITY_2021_1_OR_NEWER
                Assert.IsTrue(m.HasFloat(propName));
                #endif
                var to = Random.value;
                Tween.MaterialProperty(m, Shader.PropertyToID(propName), to, 1f).Complete();
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(to, m.GetFloat(propName));
            }
            {
                const string propName = "_MainTex";
                #if UNITY_2021_1_OR_NEWER
                Assert.IsTrue(m.HasTexture(propName));
                #endif
                var to = Random.value * Vector2.one;
                Tween.MaterialTextureOffset(m, Shader.PropertyToID(propName), to, 1f).Complete();
                Assert.AreEqual(to, m.GetTextureOffset(propName));
            }
            {
                const string propName = "_MainTex";
                #if UNITY_2021_1_OR_NEWER
                Assert.IsTrue(m.HasTexture(propName));
                #endif
                var to = Random.value * Vector2.one;
                Tween.MaterialTextureScale(m, Shader.PropertyToID(propName), to, 1f).Complete();
                Assert.IsTrue(to == m.GetTextureScale(propName));
            }
        }

        {
            var m = Resources.Load<Material>("Custom_TestShader");
            Assert.IsNotNull(m);
            const string propName = "_TestVectorProp";
            var to = Random.value * Vector4.one;
            Tween.MaterialProperty(m, Shader.PropertyToID(propName), to, 1f).Complete();
            Assert.IsTrue(to == m.GetVector(propName));
        }
    }

    /// passing the serialized UnityEngine.Object reference that is not populated behaves like passing destroyed object
    [Test]
    public void PassingDestroyedUnityTarget() {
        LogAssert.NoUnexpectedReceived();
        
        var target = new GameObject().transform;
        Object.DestroyImmediate(target.gameObject);

        var s = Sequence.Create();
        expectError();
        s.ChainCallback(target, delegate { });
        
        expectError();
        Assert.IsFalse(Tween.Delay(target, 0.1f, delegate { }).IsCreated);
        expectError();
        Assert.IsFalse(Tween.Delay(target, 0.1f).IsCreated);
        expectError();
        Assert.IsFalse(Tween.Delay(target, 0.1f, () => {}).IsCreated);

        expectError();
        Assert.IsFalse(Tween.Position(target, new TweenSettings<Vector3>(default, default, 0.1f)).IsCreated);
        
        expectError();
        var deadTween = Tween.Custom(target, 0f, 0, 0.1f, delegate { });
        Assert.IsFalse(deadTween.IsAlive);
        Assert.Throws<AssertionException>(() => Sequence.Create(deadTween));
        
        expectError();
        Tween.ShakeLocalPosition(target, default);
        
        void expectError() {
            LogAssert.Expect(LogType.Error, new Regex("Tween's UnityEngine.Object target is null"));
        }
    }

    [Test]
    public void ShakeSettings() {
        {
            var s = new ShakeSettings(Vector3.one, 1f, 1);
            Assert.IsTrue(s.enableFalloff);
        }
        {
            var s = new ShakeSettings(Vector3.one, 1f, 1, true, Ease.InBack);
            Assert.IsTrue(s.enableFalloff);
        }
        {
            var s = new ShakeSettings(Vector3.one, 1f, 1, AnimationCurve.Linear(0,0,1,1));
            Assert.IsTrue(s.enableFalloff);
        }
    }

    [UnityTest]
    public IEnumerator ForceCompleteWhenWaitingForEndDelay() {
        var t = Tween.ShakeLocalPosition(transform, new ShakeSettings(Vector3.one, 0.05f, 10f, endDelay: 1f));
        while (!t.tween.isInterpolationCompleted) {
            yield return null;
        }
        Assert.IsTrue(t.IsAlive);
        Assert.IsTrue(t.tween.isInterpolationCompleted);
        t.Complete();
    }

    [UnityTest]
    public IEnumerator StopAtEvenOrOddCycle() {
        var t = Tween.Rotation(transform, Vector3.one, 0.001f, cycles: 10);
        int cyclesDone = 0;
        while (t.cyclesDone != 2) {
            yield return null;
        }
        t.SetCycles(true);
        Assert.AreEqual(2, t.cyclesDone);
        Assert.AreEqual(3, t.cyclesTotal);
        t.SetCycles(false);
        Assert.AreEqual(2, t.cyclesDone);
        var expectedCycles = 4;
        Assert.AreEqual(expectedCycles, t.cyclesTotal);
        while (t.IsAlive) {
            cyclesDone = t.cyclesDone;
            yield return null;
        }
        Assert.AreEqual(expectedCycles - 1, cyclesDone); // the last cyclesDone is never reported because tween finishes earlier
    }

    [UnityTest]
    public IEnumerator SetCycles() {
        var t = Tween.Rotation(transform, Vector3.one, 0.001f, cycles: 10);
        while (t.cyclesDone != 2) {
            yield return null;
        }
        t.SetCycles(3);
        Assert.AreEqual(2, t.cyclesDone);
        Assert.AreEqual(5, t.cyclesTotal);
        t.Complete();
    }
}
#endif