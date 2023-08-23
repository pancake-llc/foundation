#if UNITY_EDITOR && TEST_FRAMEWORK_INSTALLED
using NUnit.Framework;
using PrimeTween;
using UnityEngine;
using AssertionException = UnityEngine.Assertions.AssertionException;

[ExecuteInEditMode]
public class EditModeTest : MonoBehaviour {
    [SerializeField] TweenSettings _settings = new TweenSettings(1, AnimationCurve.Linear(0, 0, 1, 1));
    Tween tween = test();
    Sequence sequence = Sequence.Create();
    
    static Tween test() {
        Assert.IsTrue(Constants.isEditMode, "This test is designed only for Edit mode.");
        Assert.Throws<AssertionException>(() => PrimeTweenConfig.SetTweensCapacity(10));
        Assert.Throws<AssertionException>(() => PrimeTweenConfig.warnZeroDuration = false);
        Sequence.Create();
        Tween.StopAll();
        return Tween.Custom(0, 1, 1, delegate {});
    }

    void Awake() => test();
    void OnValidate() => test();
    void Reset() => test();
    void OnEnable() => test();
    void OnDisable() => test();
    void OnDestroy() => test();
}

/*[UnityEditor.InitializeOnLoad]
public partial class EditModeTest {
    static EditModeTest() => test();
    EditModeTest() => test();

    [RuntimeInitializeOnLoadMethod]
    static void runtimeInitOnLoad() => test();
}*/
#endif