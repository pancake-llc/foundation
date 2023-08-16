#if TEST_FRAMEWORK_INSTALLED
using System.Collections;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;

internal class TweensWithDurationOfDeltaTime : MonoBehaviour {
    int frame;

    void Update() {
        switch (frame) {
            case 0:
                // print($"{Time.frameCount} start test");
                StartCoroutine(TweenWithExactDurationOfDeltaTime3());
                StartCoroutine(TweenWithExactDurationOfDeltaTime4());
                break;
            case 1:
                Assert.AreEqual(0, Tween.StopAll(this));
                Destroy(gameObject);
                break;
        }
        frame++;
    }

    IEnumerator TweenWithExactDurationOfDeltaTime3() {
        int numValueChanged = 0;
        int numCompleted = 0;
        Tween t = default;
        var duration = Time.deltaTime;
        // Debug.Log($"{Time.frameCount}, duration: {duration}");
        t = Tween.Custom(this, 0, 1, duration, (_, val) => {
            Assert.IsTrue(val == 0 || val == 1, $"elapsedTime: {t.elapsedTime}, progress: {t.progress}, interpolationFactor: {t.interpolationFactor}, dt: {Time.deltaTime}, duration: {duration}");
            numValueChanged++;
        }).OnComplete(() => numCompleted++);
        yield return t.ToYieldInstruction();
        Assert.AreEqual(2, numValueChanged);
        Assert.AreEqual(1, numCompleted);
    }

    IEnumerator TweenWithExactDurationOfDeltaTime4() {
        int numValueChanged = 0;
        int numCompleted = 0;
        Tween t = default;
        var duration = Time.deltaTime;
        t = Tween.Custom(this, 0, 1, duration, (_, val) => {
            Assert.IsTrue(val == 0 || val == 1, $"elapsedTime: {t.elapsedTime}, progress: {t.progress}, interpolationFactor: {t.interpolationFactor}, dt: {Time.deltaTime}, duration: {duration}");
            numValueChanged++;
        }).OnComplete(this, _ => numCompleted++);
        yield return t.ToYieldInstruction();
        Assert.AreEqual(2, numValueChanged);
        Assert.AreEqual(1, numCompleted);
    }
}
#endif