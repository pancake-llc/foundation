#if TEST_FRAMEWORK_INSTALLED
using System.Collections;
using NUnit.Framework;
using PrimeTween;
using UnityEngine;

public class FramePacingTest : MonoBehaviour {
    int frame;
    int numRunning;
    
    void Update() {
        if (frame == 0) {
            StartCoroutine(delayCor());
            StartCoroutine(zeroFramesAnimationCor());
            StartCoroutine(DelaysInSequenceDurationTest());
            for (int i = 2; i <= 10; i++) {
                StartCoroutine(testMult(i));
            }
            Assert.AreEqual(numRunning, 12);
            Assert.AreEqual(11, Tween.SetPausedAll(true, this));
            Assert.AreEqual(11, Tween.SetPausedAll(false, this));
        }
        if (numRunning == 0) {
            Assert.AreEqual(0, Tween.StopAll(this));
            Destroy(gameObject);
        }
        frame++;
    }

    IEnumerator DelaysInSequenceDurationTest() {
        numRunning++;
        int frameStart = Time.frameCount;
        float duration = 1f / Application.targetFrameRate;
        yield return Tween.Delay(duration)
            .Chain(Tween.Delay(duration))
            .ChainCallback(assert)
            .ToYieldInstruction();
        assert();
        void assert() => Assert.AreEqual(1, Time.frameCount - frameStart);
        numRunning--;
    }
    
    IEnumerator delayCor() {
        numRunning++;
        int frameStart = Time.frameCount;
        float deltaTime = 1f / Application.targetFrameRate;
        const int expectedFramesDelta = 1;
        yield return Tween.Delay(this, deltaTime)
            .OnComplete(() => Assert.AreEqual(expectedFramesDelta, Time.frameCount - frameStart))
            .ToYieldInstruction();
        Assert.AreEqual(expectedFramesDelta, Time.frameCount - frameStart);
        numRunning--;
    }

    IEnumerator zeroFramesAnimationCor() {
        numRunning++;
        int frameStart = Time.frameCount;
        float duration = 1f / Application.targetFrameRate;
        // print($"zeroFramesAnimationCor {duration}");
        yield return Tween.Custom(this, 0f, 0f, duration, delegate {
                // Assert.AreEqual(Time.frameCount, frameStart + 1);
            })
            .OnComplete(() => Assert.AreEqual(1, Time.frameCount - frameStart))
            .ToYieldInstruction();
        Assert.AreEqual(1, Time.frameCount - frameStart);
        numRunning--;
    }

    IEnumerator testMult(int numFrames) {
        numRunning++;
        int frameStart = Time.frameCount;
        float duration = 1f / Application.targetFrameRate * numFrames;
        // print($"multipleFrameAnimationCor {numFrames}, {duration}");
        yield return Tween.Custom(this, 0f, 0f, duration, delegate {
                // print(Time.deltaTime);
            })
            .OnComplete(() => Assert.AreEqual(numFrames, Time.frameCount - frameStart))
            .ToYieldInstruction();
        Assert.AreEqual(numFrames, Time.frameCount - frameStart);
        // print($"multipleFrameAnimationCor done {numFrames}");
        numRunning--;
    }
}
#endif