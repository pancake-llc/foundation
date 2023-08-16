#if TEST_FRAMEWORK_INSTALLED
using System;
using System.Collections;
using System.Text.RegularExpressions;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

public partial class Tests {
    [UnityTest]
    public IEnumerator ExceptionInTheMiddle() {
        expectTweenWasStoppedBecauseException();
        expectOnCompleteIgnored();
        var onCompleteDidCalled = false;
        var tweener = Tween.Custom(transform, 0, 1, 0.1f, delegate {
            throw new Exception("TEST");
        }).OnComplete(() => {
            onCompleteDidCalled = true;
        });
        while (tweener.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(onCompleteDidCalled);
    }

    [UnityTest]
    public IEnumerator ExceptionOnLastFrame() {
        expectTweenWasStoppedBecauseException();
        expectOnCompleteIgnored();
        var onCompleteDidCalled = false;
        var tweener = Tween.Custom(transform, 0, 1, 0.1f, (_, val) => {
            if (val > 0.99f) {
                throw new Exception("TEST");
            }
        }).OnComplete(() => {
            onCompleteDidCalled = true;
        });
        while (tweener.IsAlive) {
            yield return null;
        }
        Assert.IsFalse(onCompleteDidCalled);
    }

    static void expectOnCompleteIgnored() {
        LogAssert.Expect(LogType.Error, new Regex(Constants.onCompleteCallbackIgnored));
    }

    static void expectTweenWasStoppedBecauseException() {
        LogAssert.Expect(LogType.Error, new Regex("Tween was stopped because of exception"));
    }
}
#endif