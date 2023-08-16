using System.Collections;
using System.Diagnostics;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

internal class YieldInstructionsClash : MonoBehaviour {
    int frame;

    void Update() {
        log($"{Time.frameCount} Update()");
        switch (frame) {
            case 0:
                StartCoroutine(cor());
                break;
            case 1:
                Tween.Delay(0.00001f).ToYieldInstruction();
                break;
        }
        frame++;
    }

    IEnumerator cor() {
        log($"{Time.frameCount} cor start");
        int frameStart = Time.frameCount;
        yield return Tween.Delay(0.00001f).ToYieldInstruction();
        Destroy(gameObject);
        var diff = Time.frameCount - frameStart;
        Assert.AreEqual(2, diff);
        log($"{Time.frameCount} cor DONE");
    }

    [Conditional("_")]
    static void log(string msg) {
        Debug.Log(msg);
    }
}