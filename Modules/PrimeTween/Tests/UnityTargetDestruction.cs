#if TEST_FRAMEWORK_INSTALLED
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;

public partial class Tests {
    Transform transform;
    readonly TweenSettings<Vector3> settingsVector3 = new TweenSettings<Vector3>(Vector3.zero, Vector3.one, 1);

    [OneTimeSetUp]
    public void setup() {
        Application.targetFrameRate = -1;
        transform = new GameObject().transform;
        PrimeTweenConfig.SetTweensCapacity(capacityForTest);
    }

    [UnityTest]
    public IEnumerator CreatingTweenWithDestroyedTargetReturnsTweenToPool() {
        var tweens = PrimeTweenManager.Instance.tweens;
        if (tweens.Any()) {
            Tween.StopAll();
            yield return null;
        }
        Assert.AreEqual(0, tweens.Count);
        var target = new GameObject();
        var targetTr = target.transform;
        Object.DestroyImmediate(target);

        var pool = PrimeTweenManager.Instance.pool;
        var poolCount = pool.Count;
        {
            LogAssert.Expect(LogType.Error, new Regex("Tween's UnityEngine.Object target is null"));
            var t = Tween.Delay(target, 0.0001f);
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(poolCount, pool.Count);
        }
        {
            LogAssert.Expect(LogType.Error, new Regex("Tween's UnityEngine.Object target is null"));
            var t = Tween.Custom(target, 0, 0, 1, delegate { });
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(poolCount, pool.Count);
        }
        {
            LogAssert.Expect(LogType.Error, new Regex("Tween's UnityEngine.Object target is null"));
            Assert.IsTrue(targetTr == null);
            var t = Tween.Position(targetTr, default, 1);
            Assert.IsFalse(t.IsAlive);
            Assert.AreEqual(poolCount, pool.Count);
        }
        Assert.AreEqual(0, tweens.Count);
    }
    
    [UnityTest]
    public IEnumerator TweenTargetDestroyedInSequenceWithCallbacks() {
        var target = new GameObject("t1");
        yield return Sequence.Create()
            .Group(Tween.Custom(target, 0, 1, 0.1f, delegate { }))
            .Chain(Tween.Custom(target, 0, 1, 0.1f, (_target, _) => Object.DestroyImmediate(_target)).OnComplete(() => {}))
            .Chain(Tween.Delay(target, 0.1f))
            .ToYieldInstruction();
        yield return null;
        Assert.AreEqual(0, PrimeTweenManager.Instance.tweens.Count, "TweenCoroutineEnumerator should not check the target's destruction.");
        LogAssert.Expect(LogType.Warning, new Regex("Tween's onComplete callback was ignored"));
        LogAssert.NoUnexpectedReceived();
    }
    
    [UnityTest]
    public IEnumerator TweenTargetDestroyedInSequence() {
        var target = new GameObject("t1");
        yield return Sequence.Create()
            .Group(Tween.Custom(target, 0, 1, 0.1f, delegate { }))
            .Chain(Tween.Custom(target, 0, 1, 0.1f, (_target, _) => Object.DestroyImmediate(_target)))
            .Chain(Tween.Delay(target, 0.1f))
            .ToYieldInstruction();
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void SequenceTargetDestroyedBeforeCallingStop() {
        var tweener = createTweenAndDestroyTargetImmediately(false);
        Sequence.Create(tweener).Stop();
    }

    [Test]
    public void SequenceTargetDestroyedBeforeCallingComplete() {
        var tweener = createTweenAndDestroyTargetImmediately();
        Sequence.Create(tweener).Complete();
    }

    [UnityTest]
    public IEnumerator TargetDestroyedBeforeCallingCompleteAll() {
        createTweenAndDestroyTargetImmediately();
        Tween.CompleteAll();
        yield return null;
        Assert.AreEqual(0, getCurrentTweensCount());
    }

    static int getCurrentTweensCount() => PrimeTweenManager.Instance.tweens.Count;
    
    [UnityTest]
    public IEnumerator TargetDestroyedBeforeCallingCompleteByTarget() {
        var tweener = createTweenAndDestroyTargetImmediately();
        Tween.CompleteAll(tweener.tween.unityTarget, 0, 0);
        yield return null;
        Assert.AreEqual(0, getCurrentTweensCount());
    }

    /*[Test]
    public void TargetDestroyedBeforeCallingRevertByTarget() {
        var tweener = createTweenAndDestroyTargetImmediately();
        Tween.RevertAll(tweener.tween.target);
    }*/

    [Test]
    public void TargetDestroyedBeforeCallingComplete() {
        createTweenAndDestroyTargetImmediately().Complete();
    }

    /*[Test]
    public void TargetDestroyedBeforeCallingRevert() {
        createTweenAndDestroyTargetImmediately().Revert();
    }*/

    [Test]
    public void TargetDestroyedBeforeAddingOnComplete1() {
        var t = createTweenAndDestroyTargetImmediately();
        t.OnComplete(delegate { });
    }

    [Test]
    public void TargetDestroyedBeforeAddingOnComplete2() {
        var t = createTweenAndDestroyTargetImmediately();
        t.OnComplete(this, delegate { });
    }

    [Test]
    public void TargetDestroyedBeforeSettingIsPausedDisplaysError() {
        var t = createTweenAndDestroyTargetImmediately();
        t.IsPaused = true;
    }
    
    Tween createTweenAndDestroyTargetImmediately(bool expectWarnings = true) {
        if (expectWarnings) {
            LogAssert.Expect(LogType.Warning, new Regex(Constants.targetDestroyed));
            LogAssert.Expect(LogType.Warning, new Regex(Constants.onCompleteCallbackIgnored));
        }
        var tempTransform = new GameObject().transform;
        var tweener = Tween.LocalPosition(tempTransform, settingsVector3).OnComplete(Assert.Fail);
        Object.DestroyImmediate(tempTransform.gameObject);
        Assert.IsTrue(tweener.IsAlive);
        return tweener;
    }

    [UnityTest]
    public IEnumerator OnCompleteIsNotCalledIfTargetDestroyed() {
        LogAssert.Expect(LogType.Warning, new Regex(Constants.onCompleteCallbackIgnored));
        var tempTransform = new GameObject().transform;
        var tweener = Tween.LocalPosition(tempTransform, settingsVector3).OnComplete(Assert.Fail);
        Object.DestroyImmediate(tempTransform.gameObject);
        while (tweener.IsAlive) {
            yield return null;
        }
    }
    
    
    [Test]
    public void SettingCyclesOnDestroyedTweenDisplaysError() {
        var t = createTweenAndDestroyTargetImmediately();
        t.SetCycles(5);
    }
}
#endif