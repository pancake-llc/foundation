// ReSharper disable AnnotateNotNullParameter
#if TEST_FRAMEWORK_INSTALLED
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using AssertionException = UnityEngine.Assertions.AssertionException;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Tests {
    [UnityTest]
    public IEnumerator ElapsedTime() {
        var timeStart = Time.time;
        var seq = Tween.Custom(0, 1, 0.1f, delegate { }, cycles: 2)
            .Chain(Tween.Custom(0, 1, 0.05f, delegate { }, cycles: 3))
            .SetCycles(2);
        while (seq.IsAlive) {
            var expected = Time.time - timeStart;
            // Debug.Log(expected);
            float tolerance = Time.deltaTime * 3;
            try {
                if (seq.cyclesDone == 0) {
                    Assert.AreEqual(expected, seq.elapsedTime, tolerance);
                }
                Assert.AreEqual(expected, seq.elapsedTimeTotal, tolerance); // fails with lower tolerance because Sequence doesn't measure the elapsedTimeTotal, but calculates it from duration * cyclesDone
            } catch {
                throw;
            }
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator SequenceNestingCycles() {
        {
            int numCompleted = 0;
            var s1 = Sequence.Create(Tween.Delay(0.01f, () => numCompleted++));
            yield return Sequence.Create(Tween.Delay(0.01f))
                .Chain(s1)
                .ToYieldInstruction();
            Assert.AreEqual(1, numCompleted);
        }
        {
            int numCompleted = 0;
            var s1 = Sequence.Create(Tween.Delay(0.01f, () => numCompleted++));
            yield return Sequence.Create(Tween.Delay(0.01f))
                .Chain(s1)
                .SetCycles(2)
                .ToYieldInstruction();
            Assert.AreEqual(2, numCompleted);
        }
        {
            int numCompleted = 0;
            var s1 = Sequence.Create(Tween.Delay(0.01f, () => numCompleted++))
                .SetCycles(2);
            yield return Sequence.Create(Tween.Delay(0.01f))
                .Chain(s1)
                .SetCycles(2)
                .ToYieldInstruction();
            Assert.AreEqual(4, numCompleted);
        }
    }

    [UnityTest]
    public IEnumerator SequenceNestingDepsChain() {
        var s1 = Sequence.Create(Tween.Delay(0.1f));
        var t1 = Tween.Delay(0.05f);
        var t2 = Tween.Delay(0.05f);
        var s2 = Sequence.Create(t1).Group(t2);
        s1.Chain(s2);
        yield return null;
        Assert.AreEqual(0, t1.elapsedTime);
        Assert.AreEqual(0, t2.elapsedTime);
    }

    [UnityTest]
    public IEnumerator SequenceNestingDepsGroup() {
        var t1 = Tween.Delay(0.05f);
        var t2 = Tween.Delay(0.05f);
        var s = Sequence.Create(t1).Group(t2);
        Sequence.Create(Tween.Delay(0.01f))
            .ChainDelay(0.01f)
            .Group(s);
        yield return null;
        Assert.AreEqual(0, t1.elapsedTime);
        Assert.AreEqual(0, t2.elapsedTime);
    }

    [UnityTest]
    public IEnumerator SequenceNestingAfterTweenChainOp() {
        var s1 = Sequence.Create(Tween.Delay(0.01f)); // longest
        var s2 = Sequence.Create();
        s1.Chain(s2);

        var t1 = Tween.Delay(0.01f);
        s1.Chain(t1);
        Assert.AreEqual(s1.first, t1.tween.waitFor);
        
        var t2 = Tween.Delay(0.01f);
        s1.Group(t2);
        Assert.AreEqual(s1.first, t2.tween.waitFor);
        
        yield return s1.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator SequenceNestingChainOrder() {
        var t1 = Tween.Delay(0.01f);
        var s1 = Sequence.Create(t1);
        var t2 = Tween.Delay(0.05f);
        s1.Chain(t2);

        var t3 = Tween.Delay(0.01f);
        var t4 = Tween.Delay(0.01f);
        var s2 = Sequence.Create(t3).Group(t4);
        s1.Chain(s2);
        Assert.AreEqual(t2, t3.tween.waitFor);
        Assert.AreEqual(t2, t4.tween.waitFor);
        
        var t5 = Tween.Delay(0.01f);
        s1.Group(t5);
        Assert.AreEqual(t2, t5.tween.waitFor);

        yield return s1.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator SequenceNestingGroupOrder() {
        var t1 = Tween.Delay(0.01f);
        var s1 = Sequence.Create(t1);
        var t2 = Tween.Delay(0.05f);
        s1.Chain(t2);
        Assert.AreEqual(t1, t2.tween.waitFor);
        var t3 = Tween.Delay(0.01f);
        s1.Group(t3);
        Assert.AreEqual(t1, t3.tween.waitFor);
        
        var s2 = Sequence.Create(Tween.Delay(0.01f));
        var t4 = Tween.Delay(0.01f);
        s2.Group(t4);

        s1.Group(s2);
        Assert.AreEqual(t4.tween.waitFor, t1);

        var t5 = Tween.Delay(0.01f);
        s1.Group(t5);
        Assert.AreEqual(t5.tween.waitFor, t1);
        
        var t6 = Tween.Delay(0.01f);
        var longest = s1.GetLongestOrDefault();
        Assert.AreEqual(t2, longest);
        s1.Chain(t6);
        Assert.AreEqual(t6.tween.waitFor, longest);
        
        yield return s1.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator SequenceNestingRestart() {
        int numCallback = 0;
        yield return Sequence.Create(Tween.Delay(0.01f))
            .Chain(Sequence.Create(Tween.Delay(0.01f)))
            .ChainCallback(() => numCallback++)
            .SetCycles(2)
            .ToYieldInstruction();
        Assert.AreEqual(2, numCallback);
    }

    [Test]
    public void SequenceNestingPause2() {
        {
            var s1 = Sequence.Create();
            s1.IsPaused = true;
            Assert.IsTrue(s1.IsPaused);
            var s2 = Sequence.Create();
            s1.Group(s2);
            Assert.IsTrue(s2.IsPaused);
            s1.Complete();
        }
        {
            var s1 = Sequence.Create();
            s1.IsPaused = true;
            Assert.IsTrue(s1.IsPaused);
            var s2 = Sequence.Create();
            s1.Chain(s2);
            Assert.IsTrue(s2.IsPaused);
            s1.Complete();
        }
    }

    [Test]
    public void SequenceNestingPause() {
        var s = Sequence.Create(Tween.Delay(0.01f))
            .Chain(Sequence.Create(Tween.Delay(0.01f)));
        Assert.AreEqual(1, numNestedSequences(s));
        s.IsPaused = true;
        int count = 0;
        foreach (var t in s.getEnumerator(true)) {
            count++;
            Assert.IsTrue(t.IsPaused);
        }
        Assert.AreEqual(2, count);
        
        s.IsPaused = false;
        count = 0;
        foreach (var t in s.getEnumerator(true)) {
            count++;
            Assert.IsFalse(t.IsPaused);
        }
        Assert.AreEqual(2, count);
    }

    [Test]
    public void SequenceNestingLongestTween() {
        var t = Tween.Delay(0.1f);
        var sequence = Sequence.Create(Tween.Delay(0.1f))
            .Chain(Sequence.Create(Tween.Delay(0.1f)))
            .Chain(Sequence.Create(t));
        var longest = sequence.GetLongestOrDefault();
        Assert.AreEqual(t, longest);
        sequence.Stop();
        Assert.IsFalse(t.IsAlive);
    }

    [UnityTest]
    public IEnumerator SequenceNestingStopChildInTheMiddle() {
        int numS1Completed = 0;
        int numS3Completed = 0;
        var s1 = Sequence.Create(Tween.Delay(0.1f, () => {
            Assert.AreEqual(0, numS1Completed);
            Assert.AreEqual(0, numS3Completed);
            numS1Completed++;
        }));
        var s2 = Sequence.Create(Tween.Delay(0.1f, Assert.Fail));
        var s3 = Sequence.Create(Tween.Delay(0.1f, () => {
            Assert.AreEqual(1, numS1Completed);
            Assert.AreEqual(0, numS3Completed);
            numS3Completed++;
        }));
        s1.Chain(s2).Chain(s3);
        Assert.AreEqual(2, numNestedSequences(s1));
        s2.Stop();
        Assert.AreEqual(1, numNestedSequences(s1));
        yield return s1.ToYieldInstruction();
        Assert.AreEqual(1, numS1Completed);
        Assert.AreEqual(1, numS3Completed);
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
    }

    [UnityTest]
    public IEnumerator SequenceNestingCompleteLastChild() {
        var s1 = Sequence.Create(Tween.Delay(0.1f));
        var s2 = Sequence.Create(Tween.Delay(0.1f));
        s1.Chain(s2);
        Assert.AreEqual(1, numNestedSequences(s1));
        s2.Complete();
        Assert.AreEqual(0, numNestedSequences(s1));
        Assert.IsFalse(s2.IsAlive);
        Assert.IsTrue(s1.IsAlive);
        yield return s1.ToYieldInstruction();
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
    }

    static int numNestedSequences(Sequence seq) {
        Assert.IsTrue(seq.IsAlive);
        int result = -1;
        do {
            result++;
            seq = seq.childSequence;
        } while (seq.IsCreated);
        Assert.IsTrue(result >= 0);
        return result;
    }
    
    [UnityTest]
    public IEnumerator SequenceNesting() {
        var tweens = PrimeTweenManager.Instance.tweens;
        if (tweens.Count > 0) {
            Tween.StopAll();
            yield return null;
            Assert.AreEqual(0, tweens.Count);
        }
        int numS1Completed = 0;
        int numS2Completed = 0;
        var s1 = Sequence.Create(Tween.Delay(0.2f, () => {
            // Debug.Log("1");
            Assert.AreEqual(0, numS1Completed);
            Assert.AreEqual(0, numS2Completed);
            numS1Completed++;
        }));
        var s2 = Sequence.Create(Tween.Delay(0.1f, () => {
            // Debug.Log("2");
            Assert.AreEqual(1, numS1Completed);
            Assert.AreEqual(0, numS2Completed);
            numS2Completed++;
        }));
        s1.Chain(s2);
        Assert.AreEqual(1, numNestedSequences(s1));
        yield return s1.ToYieldInstruction();
        Assert.AreEqual(1, numS1Completed);
        Assert.AreEqual(1, numS2Completed);
        yield return null;
        Assert.AreEqual(0, tweens.Count);
    }

    [UnityTest]
    public IEnumerator GetSequenceElapsedTimeWhenAllTargetsDestroyed() {
        var target = new GameObject();
        var s = Sequence.Create(Tween.Delay(target, 0.1f));
        Object.DestroyImmediate(target);
        Assert.IsTrue(s.IsAlive);
        var _ = s.elapsedTime; 
        yield return s.ToYieldInstruction();
    }

    [UnityTest]
    public IEnumerator StopAllWhenSequenceHasDeadTweenAndTargetDestroyed() {
        var target = new GameObject();
        var t = Tween.Delay(target, 0.1f);
        int numCompleted = 0;
        var s = Sequence.Create(t)
            .Chain(Tween.Delay(target, 0.1f, () => numCompleted++));
        yield return t.ToYieldInstruction();
        Assert.IsFalse(t.IsAlive);
        Assert.IsTrue(s.IsAlive);
        int aliveCount = 0;
        foreach (var _ in s.getEnumerator(true)) {
            if (_.IsAlive) {
                aliveCount++;
            }
        }
        Assert.AreEqual(1, aliveCount);
        Object.DestroyImmediate(target);
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(2, tweens.Count);
        Tween.CompleteAll(null, 0, 0); // the target is destroyed, so no tweens should be processed
        Assert.AreEqual(0, numCompleted);
        Assert.AreEqual(2, tweens.Count);
        yield return null;
        Assert.AreEqual(0, tweens.Count);
        Assert.AreEqual(0, numCompleted);
    }

    [UnityTest]
    public IEnumerator StopAllWhenSequenceHasDeadTween() {
        var t = Tween.Delay(0.1f);
        var s = Sequence.Create(t)
            .ChainDelay(0.1f);
        yield return t.ToYieldInstruction();
        Assert.IsFalse(t.IsAlive);
        Assert.IsTrue(s.IsAlive);
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(2, tweens.Count);
        Tween.StopAll();
        Assert.AreEqual(2, tweens.Count);
        yield return null;
        Assert.AreEqual(0, tweens.Count);
    }
    
    #if PRIME_TWEEN_DOTWEEN_ADAPTER
    [UnityTest]
    public IEnumerator PrependInterval() {
        yield return Sequence.Create(Tween.Delay(0.01f)).PrependInterval(0.01f).ToYieldInstruction();
    }
    #endif
    
    [UnityTest]
    public IEnumerator StopSequenceFromCallback() {
        Sequence s = default;
        s = Sequence.Create()
            .ChainCallback(() => {
                s.Stop();
                Assert.IsFalse(s.IsAlive);
            });
        yield return s.ToYieldInstruction();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);
    }
    
    [Test]
    public async Task SequenceLongestTween() {
        {
            var t1 = Tween.Delay(0.01f);
            var t2 = Tween.Delay(0.02f);
            var s = t1.Group(t2);
            Assert.AreEqual(t2, s.GetLongestOrDefault());
            await s;
            Assert.AreEqual(new Tween(), s.GetLongestOrDefault());
        }
        {
            var t1 = Tween.Delay(0.01f);
            var t2 = Tween.Delay(0.02f);
            Assert.AreEqual(t2, t2.Group(t1).GetLongestOrDefault());    
        }
        {
            var t1 = Tween.Delay(0.01f);
            var t2 = Tween.Delay(0.02f);
            Assert.AreEqual(t2, t1.Chain(t2).GetLongestOrDefault());    
        }
        {
            var t1 = Tween.Delay(0.01f);
            var t2 = Tween.Delay(0.02f);
            Assert.AreEqual(t1, t2.Chain(t1).GetLongestOrDefault());    
        }
        {
            var t1 = Tween.PositionX(transform, 1f, 0.1f, cycles: 3);
            var t2 = Tween.PositionX(transform, 1f, 0.1f);
            Assert.AreEqual(t1, t1.Group(t2).GetLongestOrDefault());    
        }
        {
            var t1 = Tween.PositionX(transform, 1f, 0.1f, cycles: 3);
            var t2 = Tween.PositionX(transform, 1f, 0.1f);
            Assert.AreEqual(t2, t1.Chain(t2).GetLongestOrDefault());    
        }
    }

    [Test]
    public async Task AwaitFinishedSequence() {
        var t = Tween.Delay(0.01f);
        var s = Sequence.Create(t);
        await s;
        Assert.IsTrue(s.IsCreated);
        Assert.IsFalse(s.IsAlive);
        await s;
    }
    
    [UnityTest]
    public IEnumerator SequenceRestartFromTo() {
        var target = new GameObject(nameof(SequenceRestartFromTo)).transform;
        Assert.AreEqual(Vector3.zero, target.localPosition);
        var s = Tween.LocalPositionX(target, new TweenSettings<float>(0f, 1f, 0.05f))
            .Chain(Tween.LocalPositionX(target, new TweenSettings<float>(1f, 0.5f, 0.05f)))
            .SetCycles(2);
        while (s.cyclesDone != 1) {
            yield return null;
        }
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0f, target.localPosition.x);
        s.Complete();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.5f, target.localPosition.x);
        Object.Destroy(target.gameObject);
    }
    
    [UnityTest]
    public IEnumerator SequenceRestartTo() {
        var target = new GameObject(nameof(SequenceRestartTo)).transform;
        Assert.AreEqual(Vector3.zero, target.localPosition);
        var s = Tween.LocalPositionX(target, 1f, 0.05f)
            .Chain(Tween.LocalPositionX(target, 0.5f, 0.05f))
            .SetCycles(2);
        while (s.cyclesDone != 1) {
            yield return null;
        }
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0f, target.localPosition.x);
        s.Complete();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.5f, target.localPosition.x);
        Object.Destroy(target.gameObject);
    }
    
    [UnityTest]
    public IEnumerator SequenceRestartWhenTweenHaveStartDelay() {
        var data = new TweenSettings<Vector3>(Vector3.zero, Vector3.one, 0.01f, startDelay: 0.01f);
        var t1 = Tween.Position(transform, data);
        var t2 = Tween.Position(transform, data);
        PrimeTweenConfig.warnStructBoxingAllocationInCoroutine = true;
        LogAssert.Expect(LogType.Warning, new Regex("Please use Tween/Sequence.ToYieldInstruction"));
        yield return t1.Chain(t2).SetCycles(2);
        LogAssert.NoUnexpectedReceived();
    }
    
    [UnityTest]
    public IEnumerator StopAllWhenTweenInSequenceIsCompleted() {
        var target = new object();
        var t1 = Tween.Delay(target, 0.05f);
        var t2 = Tween.Delay(target, 0.05f);
        t1.Chain(t2);
        yield return t1.ToYieldInstruction();
        Tween.StopAll(target);
    }
    
    [UnityTest]
    public IEnumerator SequenceElapsedTime() {
        var oldFps = Application.targetFrameRate;
        var oldVsync = QualitySettings.vSyncCount;
        const int fps = 60;
        Application.targetFrameRate = fps;
        QualitySettings.vSyncCount = 0;
        yield return null;
        const float dt = 1f / fps;
        var dur1 = dt * Random.Range(2, 10);
        var dur2 = dt * Random.Range(2, 10);
        var cycles1 = Random.Range(1, 4);
        var cycles2 = Random.Range(1, 4);
        var t1 = Tween.Position(transform, Vector3.one, dur1, cycles: cycles1);
        var t2 = Tween.Position(transform, Vector3.one, dur2, cycles: cycles2);
        var s = Sequence.Create(t1)
            .Chain(t2);

        float timeStart = Time.time;
        const float tolerance = dt;
        Assert.IsTrue(t1.IsAlive);
        yield return t1;
        Assert.AreEqual(dur1 * cycles1, Time.time - timeStart, tolerance);
        
        Assert.IsTrue(s.IsAlive);
        yield return s;
        Assert.AreEqual(dur1 * cycles1 + dur2 * cycles2 - Time.deltaTime, Time.time - timeStart, tolerance);

        Application.targetFrameRate = oldFps;
        QualitySettings.vSyncCount = oldVsync;
    }
    
    [UnityTest]
    public IEnumerator DefaultPropertiesOfSequence() {
        {
            var tweens = PrimeTweenManager.Instance.tweens;
            Assert.AreEqual(0, tweens.Count);
            var duration = 0.001f;
            var s = Sequence.Create(Tween.Delay(duration)).SetCycles(-1);
            Assert.AreEqual(-1, s.cyclesTotal);
            Assert.AreEqual(duration, s.duration);
            Assert.IsTrue(float.IsPositiveInfinity(s.durationTotal));
            
            yield return null;
            Assert.AreEqual(1, tweens.Count);
            yield return null;
            Assert.AreEqual(1, tweens.Count);
            yield return null;
            Assert.AreEqual(1, tweens.Count);
            s.Stop();
            Assert.AreEqual(1, tweens.Count);
            yield return null;
            Assert.AreEqual(0, tweens.Count);
        }
        {
            var s = Sequence.Create();
            Assert.AreEqual(1, s.cyclesTotal);
            validate(s);
        }
        {
            var s = Sequence.Create();
            validate(s);
        }
        
        void validate(Sequence s) {
            Assert.AreEqual(0, s.duration);
            Assert.AreEqual(0, s.durationTotal);
            Assert.AreEqual(0, s.progress);
            Assert.AreEqual(0, s.progressTotal);
            Assert.AreEqual(0, s.cyclesDone);
            Assert.AreEqual(0, s.elapsedTime);
            Assert.AreEqual(0, s.elapsedTimeTotal);
        }
    }

    [Test]
    public async Task SequenceProperties() {
        // Application.targetFrameRate = 60;
        // EditorApplication.isPaused = true;
        // await Tween.Delay(0.01f);
        int cyclesDone = 0;
        Tween tween = default;
        var duration = 0.1f * Random.value;
        var sequenceCycles = Random.Range(2, 7);
        Sequence sequence = default;
        validateSequenceDefaultProps();
        tween = Tween.Custom(transform, 0, 1, duration, ease: Ease.Linear, onValueChange: (_, val) => {
            Assert.IsTrue(tween.IsAlive);
            Assert.IsTrue(sequence.IsAlive);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.duration, duration);
            var durationTotalExpected = duration * sequenceCycles;
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.durationTotal, durationTotalExpected);
            Assert.AreEqual(sequence.cyclesDone, cyclesDone);
            // Debug.Log($"elapsedTimeTotal: {sequence.elapsedTimeTotal}, val: {val}");
            Assert.AreEqual(tween.duration, duration);
            Assert.AreEqual(tween.durationTotal, duration);
            var elapsedTimeExpected = val * duration;
            if (val < 1f) {
                if (cyclesDone == 0) {
                    UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.elapsedTime, tween.elapsedTime);
                    UnityEngine.Assertions.Assert.AreApproximatelyEqual(tween.elapsedTime, elapsedTimeExpected);
                    UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.elapsedTime, elapsedTimeExpected);
                }
                var elapsedTimeTotalExpected = elapsedTimeExpected + duration * cyclesDone;
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.elapsedTimeTotal, elapsedTimeTotalExpected);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.progress, elapsedTimeExpected / duration);
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(sequence.progressTotal, elapsedTimeTotalExpected / durationTotalExpected);
            }
        });
        sequence = Sequence.Create(tween);
        await sequence.ChainCallback(() => cyclesDone++)
            .SetCycles(sequenceCycles);
        Assert.AreEqual(sequenceCycles, cyclesDone);
        Assert.IsFalse(sequence.IsAlive);
        validateSequenceDefaultProps();
        Assert.IsFalse(tween.IsAlive);

        void validateSequenceDefaultProps() {
            Assert.AreEqual(0, sequence.elapsedTime);
            Assert.AreEqual(0, sequence.elapsedTimeTotal);
            Assert.AreEqual(0, sequence.cyclesDone);
            Assert.AreEqual(0, sequence.cyclesTotal);
            Assert.AreEqual(0, sequence.duration);
            Assert.AreEqual(0, sequence.durationTotal);
            Assert.AreEqual(0, sequence.progress);
            Assert.AreEqual(0, sequence.progressTotal);
        }
    }

    [Test]
    public async Task SequenceIncrementalTween() {
        var t = Tween.Position(transform, Vector3.one, 0.001f, cycles: 2, cycleMode: CycleMode.Incremental);
        int cyclesDone = 0;
        const int sequenceCycles = 2;
        await Sequence.Create(t)
            .ChainCallback(() => {
                cyclesDone++;
                Assert.IsFalse(t.IsAlive);
            })
            .SetCycles(sequenceCycles);
        Assert.AreEqual(sequenceCycles, cyclesDone);
    }
    
    [Test]
    public void SettingInfiniteLoopsToTweenInSequence() {
        var t = createTween();
        var s = Sequence.Create(t);
        LogAssert.Expect(LogType.Error, Constants.infiniteTweenInSequenceError);
        t.SetCycles(-1);
        s.Complete();
    }
    
    [Test]
    public void TweenIsReleasedFromSequenceOnReleaseAll() {
        test(() => Tween.StopAll());        
        test(() => Tween.CompleteAll());
        // test(() => Tween.RevertAll());  

        void test(Action action) {
            var t = createTween();
            var s = Sequence.Create(t);
            Assert.IsTrue(t.IsAlive);
            Assert.IsTrue(s.IsAlive);
            action();
            Assert.IsFalse(t.IsAlive);
            Assert.IsFalse(s.IsAlive);
            Assert.IsFalse(t.tween.sequence.IsCreated);    
        }
    }
    
    [Test]
    public void SequenceComplete() {
        int numTweenCompleted = 0;
        var s = Sequence.Create(createCustomTween(0.01f).OnComplete(() => numTweenCompleted++));
        Assert.IsTrue(s.IsAlive);
        s.Complete();
        Assert.IsFalse(s.IsAlive);
        Assert.AreEqual(1, numTweenCompleted);
    }
    
    [Test]
    public void SequenceStop() {
        var s = Sequence.Create(createCustomTween(0.01f));
        Assert.IsTrue(s.IsAlive);
        s.Stop();
        Assert.IsFalse(s.IsAlive);
        s.Stop();
        Assert.IsFalse(s.IsAlive);
    }
    
    /*[UnityTest]
    public IEnumerator SequenceRevert() {
        var val = 0f;
        var t = Tween.Custom(this, 0, 1, 1, (_, newVal) => {
            val = newVal;
        });
        var s = Sequence.Create(t);
        yield return null;
        Assert.AreNotEqual(0, val);
        s.Revert();
        Assert.IsFalse(s.IsAlive);
        Assert.AreEqual(0, val);
    }*/
    
    [UnityTest]
    public IEnumerator CompletedTweenInsideSequenceDoesntCompleteSecondTimeOnCompletingSequence() {
        var numFirstCompleted = 0;
        var numSecondCompleted = 0;
        var first = createCustomTween(0.01f).OnComplete(() => numFirstCompleted++);
        var second = createCustomTween(0.1f).OnComplete(() => numSecondCompleted++);
        var sequence = Sequence.Create(first)
            .Group(second);
        while (first.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(first.IsAlive);
        Assert.IsFalse(first.tween.isAlive);
        Assert.AreEqual(1, numFirstCompleted);
        
        Assert.IsTrue(second.IsAlive);
        Assert.IsTrue(sequence.IsAlive);
        sequence.Complete();
        Assert.AreEqual(1, numFirstCompleted);
        Assert.AreEqual(1, numSecondCompleted);
    }

    static Tween createCustomTween(float duration) {
        return Tween.Custom(PrimeTweenManager.Instance, 0, 1, duration, delegate{});
    }

    [Test]
    public void CompletingSequenceCompletesAllTweensInside() {
        var numFirstCompleted = 0;
        var numSecondCompleted = 0;
        Sequence.Create(createTween().OnComplete(() => numFirstCompleted++))
            .Group(createTween().OnComplete(() => numSecondCompleted++))
            .Complete();
        Assert.AreEqual(1, numFirstCompleted);
        Assert.AreEqual(1, numSecondCompleted);
    }
    
    [UnityTest]
    public IEnumerator Cycles() {
        var tweenCyclesDone = 0;
        var sequenceCyclesDone = 0;
        const int sequenceCycles = 5;
        const int tweenCycles = 3;
        // tween cycles doesn't matter because OnComplete will only be executed when all cycles complete
        var tween = Tween.Custom(this, 0, 1, 0.000001f, cycles: tweenCycles, onValueChange: delegate{})
            .OnComplete(() => {
                // Debug.Log($"{Time.time} tweenCycles++");
                tweenCyclesDone++;
            });
        Sequence sequence = default;
        sequence = Sequence.Create(tween)
            .ChainCallback(() => {
                // Debug.Log($"{Time.time} sequenceCycles++");
                Assert.AreEqual(sequenceCyclesDone, sequence.cyclesDone);
                Assert.AreEqual(sequenceCycles, sequence.cyclesTotal);
                sequenceCyclesDone++;
            })
            .SetCycles(sequenceCycles);
        const int numFrames = tweenCycles * sequenceCycles;
        for (int i = 0; i < numFrames; i++) {
            Assert.IsTrue(tween.IsAlive);
            Assert.IsTrue(sequence.IsAlive);    
            yield return null;
        }
        Assert.IsFalse(tween.IsAlive);
        Assert.IsFalse(sequence.IsAlive);
        Assert.AreEqual(sequenceCycles, tweenCyclesDone);
        Assert.AreEqual(sequenceCycles, sequenceCyclesDone);
    }

    /*static IEnumerator Timeout([NotNull] Func<bool> continueWaiting, float timeout) {
        var timeStarted = Time.time;
        while (continueWaiting()) {
            if (Time.time - timeStarted > timeout) {
                throw new Exception($"Timeout of {timeout} seconds reached.");
            }
            yield return null;
        }
    }*/

    [Test]
    public void GroupDeadTweenToSequenceThrows() {
        var tweener = createTween();
        tweener.Complete();
        Assert.Throws<AssertionException>(() => {
            Sequence.Create(Tween.Delay(0.001f)).Group(tweener);
        });
    }

    Tween createTween() {
        var t = Tween.LocalPosition(transform, Vector3.one, 1);
        Assert.IsTrue(t.IsAlive);
        return t;
    }

    [Test]
    public void ChainDeadTweenToSequenceThrows() {
        var tweener = Tween.LocalPosition(transform, Vector3.one, 1);
        tweener.Complete();
        Assert.IsFalse(tweener.IsAlive);
        Assert.Throws<AssertionException>(() => {
            Sequence.Create(Tween.Delay(0.0001f)).Chain(tweener);
        });
    }

    /// it's allowed to Stop/Complete() tweens inside sequences now
    [UnityTest]
    public IEnumerator ManipulatingTweensInsideSequence() {
        var go = new GameObject();
        Tween tween;
        
        createSequence();
        tween.Stop();
        
        createSequence();
        tween.Complete();
        
        // tween.Revert();
        
        createSequence();
        Tween.StopAll(go, 1, 1);
        
        createSequence();
        Tween.CompleteAll(go, 1, 1);
        
        // Tween.RevertAll(go);

        LogAssert.NoUnexpectedReceived();

        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count);

        void createSequence() {
            tween = Tween.Custom(go, 0, 1, 1, delegate{});
            Sequence.Create(tween);    
            Assert.IsTrue(tween.IsAlive);
            const string message = Constants.setPauseOnTweenInsideSequenceError;
            LogAssert.Expect(LogType.Error, message);
            Assert.AreEqual(0, Tween.SetPausedAll(true, go));
            LogAssert.Expect(LogType.Error, message);
            Assert.AreEqual(0, Tween.SetPausedAll(false, go));
        }
    }

    [Test]
    public void AddingInfiniteTweenToSequenceThrows() {
        var infiniteTween = createInfiniteTween();
        var message = "It's not allowed to have infinite tweens";
        expectException<AssertionException>(() => {
            Sequence.Create(infiniteTween);
        }, message);
        expectException<AssertionException>(() => {
            Sequence.Create(infiniteTween);
        }, message);
        expectException<AssertionException>(() => {
            Sequence.Create(Tween.Delay(0.0001f)).Chain(infiniteTween);
        }, message);
    }
    
    static void expectException<T>(Action code, [NotNull] string message) where T: Exception {
        Assert.IsFalse(string.IsNullOrEmpty(message));
        try {
            code();
        } catch (T e) {
            if (!e.Message.Contains(message)) {
                Debug.LogException(e);
                Assert.Fail();
            }
            return;
        }
        Assert.Fail();
    }

    [Test]
    public void TestDeadSequence() {
        var s = createDeadSequence();
        
        expectError();
        s.Group(createCustomTween(1));
        
        expectError();
        s.ChainCallback(delegate { });
        
        expectError();
        s.ChainCallback(this, delegate { });
        
        expectError();
        s.Chain(createTween());

        #if PRIME_TWEEN_DOTWEEN_ADAPTER
        expectError();
        s.ChainLast(createTween());
        #endif

        s.Stop();
        s.Complete();
        // s.Revert();
        
        expectError();
        s.SetCycles(5);
        
        expectError();
        s.IsPaused = true;
        
        void expectError() {
            LogAssert.Expect(LogType.Error, Constants.sequenceIsDeadMessage);
        }
    }
    
    static Sequence createDeadSequence() {
        var s = Sequence.Create(createCustomTween(1));
        s.Complete();
        Assert.IsFalse(s.IsAlive);
        return s;
    }

    [Test]
    public void AddingTweenToSequenceModifiesIsPausedToMatchSequence() {
        var s = Sequence.Create(createTween());
        Assert.IsFalse(s.IsPaused);
        
        var t = createTween();
        Assert.IsFalse(t.IsPaused);
        t.IsPaused = true;
        Assert.IsTrue(t.IsPaused);
        
        s.Group(t);
        Assert.IsFalse(t.IsPaused);
    }


    [Test]
    public void DuplicateTweenAddedToSequenceThrows() {
        var t = createTween();
        var s = Sequence.Create(t);
        var error = "A tween can be added to a sequence only once and can only belong to one sequence.";
        expectException<Exception>(() => s.Group(t), error);

        var s2 = Sequence.Create();
        expectException<Exception>(() => s2.Group(t), error);
        
        expectException<Exception>(() => Sequence.Create(t), error);
    }
    
    [UnityTest]
    public IEnumerator TweenIsNotReleasedFromSequenceUntilSequenceReleasesAllTweens() {
        var t = createCustomTween(0.0001f);
        var delay = Tween.Delay(0.05f);
        Sequence.Create(t).Group(delay);
        Assert.IsTrue(t.IsAlive);
        yield return t.ToYieldInstruction();
        Assert.IsFalse(t.IsAlive);
    }
    
    [UnityTest]
    public IEnumerator ProcessAllDoesntLeaveUnreleasableTweens() {
        Tween.StopAll();
        yield return null;
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        var t1 = createCustomTween(0.0001f);
        var t2 = createCustomTween(0.1f);
        if (Random.value < 0.5f) {
            t1.Group(t2);
        } else {
            t1.Chain(t2);
        }
        yield return t1.ToYieldInstruction();
        Assert.IsFalse(t1.IsAlive);
        Assert.IsTrue(t2.IsAlive);
        Assert.AreEqual(2, tweens.Count);
        var numStopped = Tween.StopAll();
        Assert.AreEqual(1, numStopped);
        yield return null;
        Assert.AreEqual(0, tweens.Count);
    }
    
    [UnityTest]
    public IEnumerator ProcessAllDoesntLeaveUnreleasableTweensWhenTargetDestroyed() {
        var tweens = PrimeTweenManager.Instance.tweens;
        Assert.AreEqual(0, tweens.Count);
        var target1 = new GameObject();
        var t1 = Tween.Custom(target1, 0, 1,0.0001f, delegate{});
        var t2 = createCustomTween(0.1f);
        t1.Group(t2);
        Object.DestroyImmediate(target1);
        Assert.AreEqual(2, tweens.Count);
        Assert.IsTrue(t1.IsAlive);
        Tween.StopAll(null, 2, 2);
        yield return null;
        Assert.AreEqual(0, tweens.Count);
    }

    [Test]
    public void SequenceCompleteWhenMoreThanTwo() {
        createTween()
            .Group(createTween())
            .Group(createTween())
            .Complete();
    }

    [Test]
    public void SequenceDuration() {
        var dur1 = Random.value;
        var cycles1 = Random.Range(1, 5);
        var s = Sequence.Create(Tween.Custom(this, 0, 1, dur1, cycles: cycles1, onValueChange: delegate{}));
        Assert.AreEqual(dur1 * cycles1, s.duration);
        Assert.AreEqual(dur1 * cycles1, s.durationTotal);
        var dur2 = dur1 + Random.value;
        var cycles2 = cycles1 + Random.Range(1, 5);
        s.Group(Tween.Custom(this, 0, 1, dur2, cycles: cycles2, onValueChange: delegate{}));
        Assert.AreEqual(dur2 * cycles2, s.duration);
        Assert.AreEqual(dur2 * cycles2, s.durationTotal);
        s.Chain(Tween.Delay(1));
        var expected = dur2 * cycles2 + 1;
        Assert.AreEqual(expected, s.duration);
        Assert.AreEqual(expected, s.durationTotal);
        var sequenceCycles = Random.Range(1, 100);
        s.SetCycles(sequenceCycles);
        Assert.AreEqual(expected, s.duration);
        Assert.AreEqual(expected * sequenceCycles, s.durationTotal, 0.01f);
        s.Complete();
    }

    [UnityTest]
    public IEnumerator SequenceCycles() {
        int cyclesDone = 0;
        var iniCycles = Random.Range(3, 10);
        var s = Sequence.Create(Tween.PositionX(transform, 3.14f, 0.05f, cycles: 2))
            .ChainCallback(this, _ => cyclesDone++)
            .SetCycles(iniCycles);
        Assert.AreEqual(iniCycles, s.cyclesTotal);
        Assert.AreEqual(0, s.cyclesDone);
        while (cyclesDone == 0) {
            yield return null;
        }
        Assert.AreEqual(1, cyclesDone);
        Assert.AreEqual(iniCycles, s.cyclesTotal);
        Assert.AreEqual(1, s.cyclesDone);
        var pendingCycles = Random.Range(2, 10);
        s.SetCycles(pendingCycles);
        var expectedCycles = cyclesDone + pendingCycles;
        Assert.AreEqual(expectedCycles, s.cyclesTotal);
        Assert.AreEqual(1, s.cyclesDone);
        while (cyclesDone == 1) {
            yield return null;
        }
        Assert.AreEqual(2, cyclesDone);
        Assert.AreEqual(expectedCycles, s.cyclesTotal);
        Assert.AreEqual(2, s.cyclesDone);
        s.Complete();
    }

    [Test]
    public void AwaitNewlyCreatedSequenceDoesntCompleteSyncButInSameFrame() {
        bool isCompleted = false;
        #pragma warning disable CS4014
        AwaitNewlyCreatedSequenceDoesntCompleteSyncButInSameFrame_internal(() => isCompleted = true);
        #pragma warning restore CS4014
        Assert.IsFalse(isCompleted);
    }

    static async Task AwaitNewlyCreatedSequenceDoesntCompleteSyncButInSameFrame_internal([NotNull] Action callback) {
        int frameStarted = Time.frameCount;
        await Sequence.Create();
        Assert.AreEqual(frameStarted, Time.frameCount); // completes in same frame, but not immediately
        callback();
    }

    [Test]
    public void UsingDefaultSequenceCtorShouldThrow() {
        var t = Tween.Delay(0.0001f);
        expectException<AssertionException>(() => new Sequence().Group(t), "please use Sequence.Create()");
        expectException<AssertionException>(() => new Sequence().Chain(t), "please use Sequence.Create()");
        #if PRIME_TWEEN_DOTWEEN_ADAPTER
        expectException<AssertionException>(() => new Sequence().ChainLast(t), "please use Sequence.Create()");
        #endif
    }

    [Test]
    public async Task OnCompleteCallbackIsCalledOnlyOnce() {
        int numT1Completed = 0;
        var t1 = Tween.Delay(0.05f).OnComplete(() => numT1Completed++);
        var s = t1.Chain(Tween.Delay(0.05f));
        await t1;
        Assert.IsFalse(t1.IsAlive);
        Assert.AreEqual(1, numT1Completed);
        s.Complete();
        Assert.AreEqual(1, numT1Completed);
    }
}
#endif