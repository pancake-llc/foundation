#if TEST_FRAMEWORK_INSTALLED
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using AssertionException = UnityEngine.Assertions.AssertionException;

public class EditModeTests {
    [Test]
    public void TestEditMode() {
        expectError();
        Tween.Custom(0, 1, 1, delegate {});
        var go = new GameObject();
        {
            expectError();
            Tween.Alpha(go.AddComponent<SpriteRenderer>(), 0, 1);
            expectError();
            Tween.Delay(1);
            expectError();
            Tween.Delay(0);
            expectError();
            Tween.CompleteAll();
            expectError();
            Tween.StopAll();
            expectError();
            Tween.SetPausedAll(true);
            expectError();
            Tween.ShakeLocalPosition(go.transform, Vector3.one, 1);
            expectError();
            Tween.ShakeCustom(go, Vector3.zero, new ShakeSettings(Vector3.one, 1), delegate {});
            expectError();
            Sequence.Create();
            #if PRIME_TWEEN_EXPERIMENTAL
            expectError();
            Tween.GlobalTimeScale(0.5f, 0.1f);
            #endif
            expectError();
            Tween.GetTweensCount(this);
            expectError();
            Tween.GetTweensCount();
            
            Sequence.Create(default);
            TweenSettings.ValidateCustomCurveKeyframes(AnimationCurve.Linear(0, 0, 1, 1));
            Assert.Throws<AssertionException>(() => PrimeTweenConfig.SetTweensCapacity(10));
            Assert.Throws<AssertionException>(() => PrimeTweenConfig.defaultEase = Ease.InCirc);
        }
        Object.DestroyImmediate(go);
        void expectError() {
            LogAssert.Expect(LogType.Warning, Constants.editModeWarning);
        }
        LogAssert.NoUnexpectedReceived();
    }
}
#endif