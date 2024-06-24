using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;


namespace PancakeEditor.Sound
{
    public class SpatialSettingsEditorWindow : EditorWindow
    {
        public const string REVERB_ZONE_MIX_LABEL = "Reverb Zone Mix";
        public Vector2 WindowSize => new(400f, 550f);

        private MethodInfo _draw3DguiMethod;
        private UnityEditor.Editor _audioSourceEditor;
        private readonly Keyframe[] _dummyFrameArray = {new(0f, 0f)};
        private GameObject _tempObj;
        private SerializedProperty _spatialProp;

        private EditorAudioEx.MultiLabel StereoPanLabels => new() {main = "Stereo Pan", left = "Left", right = "Right"};
        private EditorAudioEx.MultiLabel SpatialBlendLabels => new() {main = "Spatial Blend", left = "2D", right = "3D"};

        public static void ShowWindow(SerializedProperty spatialProp)
        {
            var window = GetWindow<SpatialSettingsEditorWindow>();
            window.minSize = window.WindowSize;
            window.maxSize = window.WindowSize;
            window.titleContent = new GUIContent("Spatial Settings");
            window.Init(spatialProp);
#if UNITY_2021_1_OR_NEWER
            EditorApplication.update += Show;
#else
			window.ShowModalUtility();
#endif
            void Show()
            {
                // When the modal window popup, the other GUI should be freezed. But this behavior ridiculously changed after Untiy2021. 
                EditorApplication.update -= Show;
                window.ShowModalUtility();
            }
        }

        private void Init(SerializedProperty spatialProp)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioSourceInspector = unityEditorAssembly?.GetType($"UnityEditor.AudioSourceInspector");
            _draw3DguiMethod = audioSourceInspector?.GetMethod("Audio3DGUI", BindingFlags.NonPublic | BindingFlags.Instance);

            _tempObj = new GameObject("AudioSourceInspector") {hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave};
            var audioSource = _tempObj.AddComponent<AudioSource>();
            _audioSourceEditor = UnityEditor.Editor.CreateEditor(audioSource);

            if (spatialProp.objectReferenceValue != null && spatialProp.objectReferenceValue is SpatialSetting spatialSetting)
            {
                _spatialProp = spatialProp;
                foreach (ESpatialPropertyType propType in Enum.GetValues(typeof(ESpatialPropertyType)))
                {
                    SetAudioSourceSpatialProperty(propType, spatialSetting);
                }
            }
        }

        private void SetAudioSourceSpatialProperty(ESpatialPropertyType propType, SpatialSetting spatialSetting)
        {
            var audioSource = _audioSourceEditor.serializedObject;
            switch (propType)
            {
                case ESpatialPropertyType.StereoPan:
                    audioSource.FindProperty("Pan2D").floatValue = spatialSetting.stereoPan;
                    break;
                case ESpatialPropertyType.DopplerLevel:
                    audioSource.FindProperty("DopplerLevel").floatValue = spatialSetting.dopplerLevel;
                    break;
                case ESpatialPropertyType.MinDistance:
                    audioSource.FindProperty("MinDistance").floatValue = spatialSetting.minDistance;
                    break;
                case ESpatialPropertyType.MaxDistance:
                    audioSource.FindProperty("MaxDistance").floatValue = spatialSetting.maxDistance;
                    break;
                case ESpatialPropertyType.SpatialBlend:
                    if (!spatialSetting.spatialBlend.IsDefaultCurve(AudioConstant.SPATIAL_BLEND_2D))
                    {
                        audioSource.FindProperty("panLevelCustomCurve").animationCurveValue = spatialSetting.spatialBlend;
                    }

                    break;
                case ESpatialPropertyType.ReverbZoneMix:
                    if (!spatialSetting.reverbZoneMix.IsDefaultCurve(AudioConstant.DEFAULT_REVER_ZONE_MIX))
                    {
                        audioSource.FindProperty("reverbZoneMixCustomCurve").animationCurveValue = spatialSetting.reverbZoneMix;
                    }

                    break;
                case ESpatialPropertyType.Spread:
                    if (!spatialSetting.spread.IsDefaultCurve(AudioConstant.DEFAULT_SPREAD))
                    {
                        audioSource.FindProperty("spreadCustomCurve").animationCurveValue = spatialSetting.spread;
                    }

                    break;
                case ESpatialPropertyType.CustomRolloff:
                    if (spatialSetting.rolloffMode != AudioConstant.DEFAULT_ROLLOFF_MODE)
                    {
                        audioSource.FindProperty("rolloffCustomCurve").animationCurveValue = spatialSetting.customRolloff;
                    }

                    break;
                case ESpatialPropertyType.RolloffMode:
                    audioSource.FindProperty("rolloffMode").enumValueIndex = (int) spatialSetting.rolloffMode;
                    break;
            }
        }

        private void OnDisable()
        {
            if (_audioSourceEditor != null)
            {
                if (_spatialProp != null && _audioSourceEditor.target is AudioSource audioSource)
                {
                    var serializedSpatial = new SerializedObject(_spatialProp.objectReferenceValue);
                    serializedSpatial.FindProperty(nameof(SpatialSetting.stereoPan)).floatValue = audioSource.panStereo;
                    serializedSpatial.FindProperty(nameof(SpatialSetting.dopplerLevel)).floatValue = audioSource.dopplerLevel;
                    serializedSpatial.FindProperty(nameof(SpatialSetting.minDistance)).floatValue = audioSource.minDistance;
                    serializedSpatial.FindProperty(nameof(SpatialSetting.maxDistance)).floatValue = audioSource.maxDistance;
                    serializedSpatial.FindProperty(nameof(SpatialSetting.spatialBlend)).animationCurveValue =
                        audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
                    serializedSpatial.FindProperty(nameof(SpatialSetting.reverbZoneMix)).animationCurveValue =
                        audioSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix);
                    serializedSpatial.FindProperty(nameof(SpatialSetting.spread)).animationCurveValue = audioSource.GetCustomCurve(AudioSourceCurveType.Spread);
                    if (audioSource.rolloffMode != AudioConstant.DEFAULT_ROLLOFF_MODE)
                    {
                        serializedSpatial.FindProperty(nameof(SpatialSetting.customRolloff)).animationCurveValue =
                            audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
                    }

                    serializedSpatial.FindProperty(nameof(SpatialSetting.rolloffMode)).enumValueIndex = (int) audioSource.rolloffMode;
                    serializedSpatial.ApplyModifiedProperties();
                    _spatialProp.serializedObject.ApplyModifiedProperties();
                }

                DestroyImmediate(_tempObj, true);
                DestroyImmediate(_audioSourceEditor);
                _audioSourceEditor = null;
            }
        }

        private void OnGUI()
        {
            if (_audioSourceEditor == null)  return;

            DrawStereoPan();
            EditorGUILayout.Space();

            var spatialBlendProp = _audioSourceEditor.serializedObject.FindProperty("panLevelCustomCurve");
            SerializeAnimateCurveValue(spatialBlendProp, SpatialBlendLabels.main, currValue => EditorAudioEx.Draw2SidesLabelSliderLayout(SpatialBlendLabels, currValue, 0f, 1f));
            EditorGUILayout.Space();

            var reverbZoneProp = _audioSourceEditor.serializedObject.FindProperty("reverbZoneMixCustomCurve");
            SerializeAnimateCurveValue(reverbZoneProp,
                REVERB_ZONE_MIX_LABEL,
                currValue => EditorGUILayout.Slider(REVERB_ZONE_MIX_LABEL, currValue, 0f, 1.1f)); // reverb zone can accept value up to 1.1
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("3D Sound Settings".ToBold(), Uniform.RichLabel);
            _draw3DguiMethod?.Invoke(_audioSourceEditor, null);
            _audioSourceEditor.serializedObject.ApplyModifiedProperties();
        }

        private void DrawStereoPan()
        {
            var stereoPanProp = _audioSourceEditor.serializedObject.FindProperty("Pan2D");
            stereoPanProp.floatValue = EditorAudioEx.Draw2SidesLabelSliderLayout(StereoPanLabels, stereoPanProp.floatValue, -1f, 1f);
        }

        private void SerializeAnimateCurveValue(SerializedProperty serializedProperty, string label, Func<float, float> onDrawSlider)
        {
            if (serializedProperty.animationCurveValue.length > 1)
            {
                var leftMiniGrey = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {alignment = TextAnchor.MiddleLeft};
                EditorGUILayout.LabelField(label, "Controlled by curve", leftMiniGrey);
            }
            else if (serializedProperty.animationCurveValue.length == 1)
            {
                EditorGUI.BeginChangeCheck();
                float newValue = onDrawSlider.Invoke(serializedProperty.animationCurveValue[0].value);
                if (EditorGUI.EndChangeCheck())
                {
                    var curve = serializedProperty.animationCurveValue;
                    _dummyFrameArray[0] = new Keyframe(0f, newValue);
                    curve.keys = _dummyFrameArray;
                    serializedProperty.animationCurveValue = curve;
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}