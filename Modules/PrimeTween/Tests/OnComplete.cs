#if TEST_FRAMEWORK_INSTALLED
using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

public partial class Tests {
    /*[Test]
    public void OnCompleteIsNotCalledAfterCallingRevert() {
        createTween().OnComplete(() => Assert.Fail()).Revert();
    }*/
    
    [Test]
    public void OnCompleteIsCalledImmediatelyAfterCallingComplete() {
        var onCompleteIsCalled = false;
        var t = createTween().OnComplete(() => onCompleteIsCalled = true);
        Assert.IsFalse(onCompleteIsCalled);
        t.Complete();
        Assert.IsTrue(onCompleteIsCalled);
    }
    
    [Test]
    public void OnCompleteDuplicationThrows() {
        var t = createTween().OnComplete(() => {});
        try {
            t.OnComplete(() => { });
        } catch (Exception e) {
            Assert.IsTrue(e.Message.Contains("Tween already has an onComplete callback"));
            return;
        }
        Assert.Fail();
    }

    [Test]
    public void AddingOnCompleteToInfiniteTween() {
        int numCompleted = 0;
        createInfiniteTween().OnComplete(() => numCompleted++).Complete();
        Assert.AreEqual(1, numCompleted);
    }

    Tween createInfiniteTween() {
        return Tween.Custom(this, 0, 1, 0.01f, cycles: -1, onValueChange: delegate { });
    }

    [Test]
    public void AddingOnCompleteOnDeadTweenDisplaysError() {
        var t = createTween();
        Assert.IsTrue(t.IsAlive);
        t.Complete();
        Assert.IsFalse(t.IsAlive);
        LogAssert.Expect(LogType.Error, new Regex(Constants.tweenIsDeadMessage));
        t.OnComplete(delegate { });
        LogAssert.Expect(LogType.Error, new Regex(Constants.tweenIsDeadMessage));
        t.OnComplete(this, delegate { });
    }
}
#endif