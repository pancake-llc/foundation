using System;
using System.Collections.Generic;
using System.IO;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Math = System.Math;

namespace PancakeEditor.Sound
{
    public static class EditorAudioEx
    {
        public enum RandomRangeSliderType
        {
            Default,
            Logarithmic,
            Volume,
            VolumeNoField
        }

        public struct TabViewData
        {
            public float ratio;
            public GUIContent label;
            public Action onTabChanged;
            public Action<Rect, SerializedProperty> onButtonClick;

            public TabViewData(float ratio, GUIContent label, Action onTabChanged, Action<Rect, SerializedProperty> onButtonClick)
            {
                this.ratio = ratio;
                this.label = label;
                this.onTabChanged = onTabChanged;
                this.onButtonClick = onButtonClick;
            }
        }

        private const float LOW_VOLUME_SNAPPING_THRESHOLD = 0.05f;
        private const float HIGH_VOLUME_SNAPPING_THRESHOLD = 0.2f;
        private const string DB_VALUE_STRING_FORMAT = "0.##";
        private static Vector2 DecibelLabelSize => new(55f, 25f);
        private static Vector2 MinMaxDecibelLabelSize => new(115f, 25f);
        public static readonly float[] VolumeSplitPoints = {-80f, -60f, -36f, -24f, -12f, -6f, 0f, 6f, 20f};
        public static Action onCloseWindow;
        public static Action onSelectAsset;
        public static Action onLostFocus;

        public static string AssetOutputPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LibraryDataContainer.Data.Settings.assetOutputPath))
                {
                    LibraryDataContainer.Data.Settings.assetOutputPath = "Assets/_Root/Storages";
                    LibraryDataContainer.Data.SaveSetting();
                }

                return LibraryDataContainer.Data.Settings.assetOutputPath;
            }
            set => LibraryDataContainer.Data.Settings.assetOutputPath = value;
        }

        public static int GetProjectSettingRealAudioVoices()
        {
            var path = $"{Application.dataPath.Replace("/Assets", string.Empty)}/ProjectSettings/AudioManager.asset";
            if (!File.Exists(path)) return default;

            using (var stream = new StreamReader(path))
            {
                while (!stream.EndOfStream)
                {
                    string lineText = stream.ReadLine();
                    if (lineText == null) continue;
                    if (!lineText.Contains("m_RealVoiceCount")) continue;

                    var value = string.Empty;
                    for (var i = 0; i < lineText.Length; i++)
                    {
                        if (char.IsNumber(lineText[i])) value += lineText[i];
                    }

                    if (!string.IsNullOrWhiteSpace(value)) return int.Parse(value);
                }
            }

            return default;
        }

        public static void ResetAudioClipSerializedProperties(SerializedProperty property)
        {
            property.FindPropertyRelative(SoundClip.ForEditor.AudioClip).objectReferenceValue = null;
            property.FindPropertyRelative(SoundClip.ForEditor.Weight).intValue = 0;
            ResetAudioClipPlaybackSetting(property);
        }

        public static void ResetAudioClipPlaybackSetting(SerializedProperty property)
        {
            property.FindPropertyRelative(SoundClip.ForEditor.Volume).floatValue = AudioConstant.FULL_VOLUME;
            property.FindPropertyRelative(SoundClip.ForEditor.StartPosition).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.ForEditor.EndPosition).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.ForEditor.FadeIn).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.ForEditor.FadeOut).floatValue = 0f;
        }

        public static void ResetEntitySerializedProperties(SerializedProperty property)
        {
            property.FindPropertyRelative(AudioEntity.ForEditor.Name).stringValue = string.Empty;
            property.FindPropertyRelative(AudioEntity.ForEditor.Clips).arraySize = 0;
            property.FindPropertyRelative(AudioEntity.ForEditor.AudioPlayMode).enumValueIndex = 0;
            property.FindPropertyRelative(AudioEntity.ForEditor.MasterVolume).floatValue = AudioConstant.FULL_VOLUME;
            property.FindPropertyRelative(AudioEntity.ForEditor.Loop).boolValue = false;
            property.FindPropertyRelative(AudioEntity.ForEditor.SeamlessLoop).boolValue = false;
            property.FindPropertyRelative(AudioEntity.ForEditor.Pitch).floatValue = AudioConstant.DEFAULT_PITCH;
            property.FindPropertyRelative(AudioEntity.ForEditor.PitchRandomRange).floatValue = 0f;
            property.FindPropertyRelative(AudioEntity.ForEditor.RandomFlags).intValue = 0;
            property.FindPropertyRelative(AudioEntity.ForEditor.Priority).intValue = AudioConstant.DEFAULT_PRIORITY;

            var spatialProp = property.FindPropertyRelative(AudioEntity.ForEditor.SpatialSetting);
            spatialProp.objectReferenceValue = null;
            spatialProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static float VolumeScale => 1f / (VolumeSplitPoints.Length - 1);

        private static float SliderFullScale =>
            AudioConstant.FULL_VOLUME / ((AudioConstant.FULL_DECIBEL_VOLUME - AudioConstant.MIN_DECIBEL_VOLUME) / AudioConstant.DecibelVoulumeFullScale);

        public static bool IsTempReservedName(string name)
        {
            string tempName = AudioConstant.TEMP_ASSET_NAME;
            bool sameLength = name.Length == tempName.Length;
            bool couldBeTempWithNumber = name.Length > tempName.Length && Char.IsNumber(name[name.Length - 1]);
            return (sameLength || couldBeTempWithNumber) && name[0] == 'T' && name.StartsWith(tempName, StringComparison.Ordinal);
        }

        public static bool TryGetEntityName(IAudioAsset asset, int id, out string name)
        {
            name = null;
            foreach (var entity in asset.GetAllAudioEntities())
            {
                if (entity.Id == id)
                {
                    name = entity.Name;
                    return true;
                }
            }

            return false;
        }

        public static void DrawAssetOutputPath(Rect rect, Action onUpdateSuccess)
        {
            var style = new GUIStyle(EditorStyles.objectField) {alignment = TextAnchor.MiddleCenter};
            if (GUI.Button(rect, new GUIContent(AssetOutputPath), style))
            {
                var openPath = $"{Application.dataPath.Replace("/Assets", string.Empty)}/{AssetOutputPath}";
                if (!Directory.Exists(openPath)) openPath = Application.dataPath;

                string newPath = EditorUtility.OpenFolderPanel("Select Audio auto-generated asset file's output folder", openPath, "");
                if (!string.IsNullOrEmpty(newPath) && newPath.Contains(Application.dataPath))
                {
                    newPath = newPath.Remove(0, Application.dataPath.Replace("/Assets", string.Empty).Length + 1);
                    AssetOutputPath = newPath;
                    WriteAssetOutputPathToSetting(newPath);
                    onUpdateSuccess?.Invoke();
                }
            }

            var browserIconRect = rect;
            browserIconRect.width = EditorGUIUtility.singleLineHeight;
            browserIconRect.height = EditorGUIUtility.singleLineHeight;
            browserIconRect.x = rect.xMax - EditorGUIUtility.singleLineHeight;
#if UNITY_2020_1_OR_NEWER
            GUI.DrawTexture(browserIconRect, EditorGUIUtility.IconContent("FolderOpened Icon").image);
#endif
            EditorGUI.DrawRect(browserIconRect, new Color(0.21f, 0.21f, 0.21f, 0.4f));
        }

        public static void DrawVuMeter(Rect vuRect)
        {
            vuRect.height *= 0.25f;
            vuRect.y += vuRect.height * 0.5f;
            EditorGUI.DrawTextureTransparent(vuRect, EditorGUIUtility.IconContent("d_VUMeterTextureHorizontal").image);
            EditorGUI.DrawRect(vuRect, new Color(0.2f, 0.2f, 0.2f, 0.6f));
        }

        public static float DrawVolumeSlider(Rect position, GUIContent label, float currentValue, bool canDrawVu = true)
        {
            return DrawVolumeSlider(position,
                label,
                currentValue,
                false,
                null,
                canDrawVu);
        }

        public static float DrawVolumeSlider(Rect position, GUIContent label, float currentValue, bool isSnap, Action onSwitchSnapMode, bool canDrawVu = true)
        {
            Rect suffixRect = EditorGUI.PrefixLabel(position, label);
            float padding = 3f;
            Rect sliderRect = new Rect(suffixRect) {width = suffixRect.width - EditorGUIUtility.fieldWidth - padding};
            Rect fieldRect = new Rect(suffixRect) {x = sliderRect.xMax + padding, width = EditorGUIUtility.fieldWidth};

#if !UNITY_WEBGL
            if (canDrawVu && AudioEditorSetting.ShowVuColorOnVolumeSlider) DrawVuMeter(sliderRect);

            DrawFullVolumeSnapPoint(sliderRect, onSwitchSnapMode);

            float newNormalizedValue = DrawVolumeSlider(sliderRect, currentValue, out bool hasSliderChanged, out float newSliderValue);
            float newFloatFieldValue = EditorGUI.FloatField(fieldRect, hasSliderChanged ? newNormalizedValue : currentValue);
            currentValue = Mathf.Clamp(newFloatFieldValue, 0f, AudioConstant.MAX_VOLUME);
            if (isSnap && CanSnap(currentValue))
            {
                currentValue = AudioConstant.FULL_VOLUME;
            }

            DrawDecibelValuePeeking(currentValue, padding, sliderRect, newSliderValue);
#else
            currentValue = GUI.HorizontalSlider(sliderRect, currentValue, 0f, FullVolume);
			currentValue = Mathf.Clamp(EditorGUI.FloatField(fieldRect, currentValue),0f,FullVolume);
#endif

            return currentValue;

#if !UNITY_WEBGL
            void DrawFullVolumeSnapPoint(Rect sliderPosition, Action onSwitch)
            {
                if (onSwitch == null) return;

                float scale = 1f / (VolumeSplitPoints.Length - 1);
                float sliderValue = VolumeToSlider(AudioConstant.FULL_VOLUME);

                Rect rect = new Rect(sliderPosition);
                rect.width = 30f;
                rect.x = sliderPosition.x + sliderPosition.width * sliderValue - (rect.width * 0.5f) + 1f; // add 1 pixel for more precise position
                rect.y -= sliderPosition.height;
                var icon = new GUIContent(EditorGUIUtility.IconContent("SignalAsset Icon"));
                icon.tooltip = "Toggle full volume snapping";
                EditorGUI.BeginDisabledGroup(!isSnap);
                {
                    GUI.Label(rect, icon);
                }
                EditorGUI.EndDisabledGroup();
                if (GUI.Button(rect, "", EditorStyles.label))
                {
                    onSwitch?.Invoke();
                }
            }

            bool CanSnap(float value)
            {
                float difference = value - AudioConstant.FULL_VOLUME;
                bool isInLowVolumeSnappingRange = difference < 0f && difference * -1f <= LOW_VOLUME_SNAPPING_THRESHOLD;
                bool isInHighVolumeSnappingRange = difference > 0f && difference <= HIGH_VOLUME_SNAPPING_THRESHOLD;
                return isInLowVolumeSnappingRange || isInHighVolumeSnappingRange;
            }
#endif
        }

        private static string GetDecibelText(float value)
        {
            float dBvalue = value.ToDecibel();
            string plusSymbol = dBvalue > 0 ? "+" : string.Empty;
            return plusSymbol + dBvalue.ToString(DB_VALUE_STRING_FORMAT) + "dB";
        }

        public static void DrawDecibelValuePeeking(float currentValue, float padding, Rect sliderRect, float sliderValue)
        {
            string volText = GetDecibelText(currentValue);
            DrawDecibelValuePeeking(volText,
                padding,
                sliderRect,
                sliderValue,
                DecibelLabelSize);
        }

        public static void DrawDecibelValuePeeking(float minValue, float maxValue, float padding, Rect sliderRect, float sliderValue)
        {
            string minDB = GetDecibelText(minValue);
            string maxDB = GetDecibelText(maxValue);
            string volText = minDB + " ~ " + maxDB;
            DrawDecibelValuePeeking(volText,
                padding,
                sliderRect,
                sliderValue,
                MinMaxDecibelLabelSize);
        }

        public static void DrawDecibelValuePeeking(string text, float padding, Rect sliderRect, float sliderValue, Vector2 size)
        {
            if (Event.current.type == EventType.Repaint && sliderRect.Contains(Event.current.mousePosition))
            {
                float sliderHandlerPos = sliderValue / SliderFullScale * sliderRect.width - (size.x * 0.5f);
                Rect valueTooltipRect = new Rect(sliderRect.x + sliderHandlerPos, sliderRect.y - size.y - padding, size.x, size.y);
                GUI.skin.window.Draw(valueTooltipRect,
                    false,
                    false,
                    false,
                    false);
                // ** Don't use EditorGUI.Label(), it will change the keyboard focus, might be a Unity's bug **
                GUI.Label(valueTooltipRect, text, Uniform.CenterRichLabel);
            }
        }

        public static float DrawVolumeSlider(Rect position, float currentValue, out bool hasChanged, out float newSliderInFullScale)
        {
            float sliderValue = VolumeToSlider(currentValue);

            EditorGUI.BeginChangeCheck();
            sliderValue = GUI.HorizontalSlider(position, sliderValue, 0f, 1f);
            newSliderInFullScale = sliderValue * SliderFullScale;
            hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged)
            {
                return SliderToVolume(sliderValue);
            }

            return currentValue;
        }

        public static void GetMixerMinMaxVolume(out float minVol, out float maxVol)
        {
            bool isWebGL = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
            minVol = AudioConstant.MIN_VOLUME;
            maxVol = isWebGL ? AudioConstant.FULL_VOLUME : AudioConstant.MAX_VOLUME;
        }

        public static void DrawMinMaxSlider(
            Rect position,
            GUIContent label,
            ref float min,
            ref float max,
            float minLimit,
            float maxLimit,
            float fieldWidth,
            Action<Rect> onGetSliderRect = null)
        {
            Rect sliderRect = DrawMinMaxLabelAndField(position,
                label,
                ref min,
                ref max,
                fieldWidth,
                onGetSliderRect);
            EditorGUI.MinMaxSlider(sliderRect,
                ref min,
                ref max,
                minLimit,
                maxLimit);
        }

        public static void DrawLogarithmicMinMaxSlider(
            Rect position,
            GUIContent label,
            ref float min,
            ref float max,
            float minLimit,
            float maxLimit,
            float fieldWidth,
            Action<Rect> onGetSliderRect = null)
        {
            Rect sliderRect = DrawMinMaxLabelAndField(position,
                label,
                ref min,
                ref max,
                fieldWidth,
                onGetSliderRect);

            min = Mathf.Log10(min);
            max = Mathf.Log10(max);
            minLimit = Mathf.Log10(minLimit);
            maxLimit = Mathf.Log10(maxLimit);

            EditorGUI.MinMaxSlider(sliderRect,
                ref min,
                ref max,
                minLimit,
                maxLimit);

            min = Mathf.Pow(10, min);
            max = Mathf.Pow(10, max);
        }

        public static void DrawRandomRangeSlider(
            Rect rect,
            GUIContent label,
            ref float value,
            ref float valueRange,
            float minLimit,
            float maxLimit,
            RandomRangeSliderType sliderType,
            Action<Rect> onGetSliderRect = null)
        {
            float minRand = value - valueRange * 0.5f;
            float maxRand = value + valueRange * 0.5f;
            minRand = (float) Math.Round(Mathf.Clamp(minRand, minLimit, maxLimit), AudioConstant.ROUNDED_DIGITS, MidpointRounding.AwayFromZero);
            maxRand = (float) Math.Round(Mathf.Clamp(maxRand, minLimit, maxLimit), AudioConstant.ROUNDED_DIGITS, MidpointRounding.AwayFromZero);
            switch (sliderType)
            {
                case RandomRangeSliderType.Default:
                    DrawMinMaxSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        AudioConstant.MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
                case RandomRangeSliderType.Logarithmic:
                    DrawLogarithmicMinMaxSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        AudioConstant.MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
                case RandomRangeSliderType.Volume:
                    DrawRandomRangeVolumeSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        AudioConstant.MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
                case RandomRangeSliderType.VolumeNoField:
                    DrawRandomRangeVolumeSliderNoField(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit);
                    break;
            }

            valueRange = maxRand - minRand;
            value = minRand + valueRange * 0.5f;
        }

        public static float DrawRandomRangeVolumeSlider(
            Rect position,
            GUIContent label,
            ref float min,
            ref float max,
            float minLimit,
            float maxLimit,
            float fieldWidth,
            Action<Rect> onGetSliderRect = null)
        {
            Rect sliderRect = DrawMinMaxLabelAndField(position,
                label,
                ref min,
                ref max,
                fieldWidth,
                onGetSliderRect);
            DrawRandomRangeVolumeSliderNoField(sliderRect,
                label,
                ref min,
                ref max,
                minLimit,
                maxLimit);
            return max;
        }

        public static float DrawRandomRangeVolumeSliderNoField(Rect position, GUIContent label, ref float min, ref float max, float minLimit, float maxLimit)
        {
            float minSlider = VolumeToSlider(min);
            float maxSlider = VolumeToSlider(max);

            EditorGUI.MinMaxSlider(position,
                ref minSlider,
                ref maxSlider,
                0f,
                1f);

            min = SliderToVolume(minSlider);
            max = SliderToVolume(maxSlider);

            float midPoint = (minSlider + maxSlider) / 2f;
            DrawDecibelValuePeeking(min,
                max,
                3f,
                position,
                midPoint);
            return max;
        }

        private static float VolumeToSlider(float vol)
        {
            float decibelVol = vol.ToDecibel();
            for (var i = 0; i < VolumeSplitPoints.Length; i++)
            {
                if (i + 1 >= VolumeSplitPoints.Length)
                {
                    return 1f;
                }
                else if (decibelVol >= VolumeSplitPoints[i] && decibelVol < VolumeSplitPoints[i + 1])
                {
                    float currentStageSliderValue = VolumeScale * i;
                    float range = Mathf.Abs(VolumeSplitPoints[i + 1] - VolumeSplitPoints[i]);
                    float stageProgress = Mathf.Abs(decibelVol - VolumeSplitPoints[i]) / range;
                    return currentStageSliderValue + stageProgress * VolumeScale;
                }
            }

            return 0f;
        }

        private static float SliderToVolume(float sliderValue)
        {
            if (Mathf.Approximately(sliderValue, 1f)) return AudioConstant.MAX_VOLUME;

            var newStageIndex = (int) (sliderValue / VolumeScale);
            float progress = (sliderValue % VolumeScale) / VolumeScale;
            float range = Mathf.Abs(VolumeSplitPoints[newStageIndex + 1] - VolumeSplitPoints[newStageIndex]);
            float decibelResult = VolumeSplitPoints[newStageIndex] + range * progress;
            return decibelResult.ToNormalizeVolume();
        }

        public static Rect GetNextLineRect(IEditorDrawLineCounter drawer, Rect position)
        {
            var newRect = new Rect(position.x,
                position.y + drawer.SingleLineSpace * drawer.DrawLineCount + drawer.Offset,
                position.width,
                EditorGUIUtility.singleLineHeight);
            return newRect;
        }

        public static Rect GetRectAndIterateLine(IEditorDrawLineCounter drawer, Rect position)
        {
            var newRect = new Rect(position.x,
                position.y + drawer.SingleLineSpace * drawer.DrawLineCount + drawer.Offset,
                position.width,
                EditorGUIUtility.singleLineHeight);
            drawer.DrawLineCount++;

            return newRect;
        }

        public static string GetFieldName(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName.Length > 0)
            {
                if (char.IsUpper(propertyName[0])) propertyName = propertyName.Replace(propertyName[0], propertyName[0].ToLower());
                return $"{propertyName}";
            }

            return propertyName;
        }

        public static string GetBackingFieldName(string propertyName) { return $"<{propertyName}>k__BackingField"; }

        public struct LabelWidthScope : IDisposable
        {
            private readonly float _originalLabelWidth;

            public LabelWidthScope(float labelWidth)
            {
                _originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;
            }

            public void Dispose() { EditorGUIUtility.labelWidth = _originalLabelWidth; }
        }

        public static void DrawMultiFloatField(Rect position, GUIContent title, GUIContent[] labels, float[] values, float totalFieldWidth = 100f, float gap = 10f)
        {
            if (labels == null || values == null || labels.Length != values.Length)
            {
                Debug.LogError("[Editor] Draw Multi-Float field failed. labels and values cannot be null, and their length should be equal");
                return;
            }

            var suffixRect = EditorGUI.PrefixLabel(position, title);

            using (new LabelWidthScope())
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var rect = new Rect(suffixRect);
                    rect.x += i == 0 ? 0 : (totalFieldWidth + gap) * i;
                    rect.width = totalFieldWidth;
                    EditorGUIUtility.labelWidth = totalFieldWidth * 0.4f;
                    values[i] = EditorGUI.FloatField(rect, labels[i], values[i]);
                    EditorGUIUtility.labelWidth = 0f;
                }
            }
        }

        public struct MultiLabel
        {
            public string main;
            public string left;
            public string right;
        }

        public static Rect DissolveHorizontal(this Rect origin, float dissolveRatio)
        {
            return new Rect(origin.xMin + dissolveRatio * origin.width, origin.y, (1 - dissolveRatio) * origin.width, origin.height);
        }

        public static Vector2 DeScope(this Vector2 inScopePos, Rect scope, Vector2 offset = default)
        {
            return new Vector2(inScopePos.x + scope.x + offset.x, inScopePos.y + scope.y + offset.y);
        }

        public static Rect Scoping(this Rect originRect, Rect scope, Vector2 offset = default)
        {
            var rect = new Rect(originRect.position.Scoping(scope, offset), originRect.size);
            rect.xMax = rect.xMax > scope.xMax ? scope.xMax : rect.xMax;
            rect.yMax = rect.yMax > scope.yMax ? scope.yMax : rect.yMax;
            return rect;
        }

        public static Vector2 Scoping(this Vector2 originPos, Rect scope, Vector2 offset = default)
        {
            return new Vector2(originPos.x - scope.x + offset.x, originPos.y - scope.y + offset.y);
        }

        public static bool TryGetPropertyObject<T>(this SerializedProperty sourceProperty, string propertyPath, out T newProperty) where T : class
        {
            newProperty = null;
            if (sourceProperty == null)
            {
                return false;
            }

            newProperty = sourceProperty.FindPropertyRelative(propertyPath)?.objectReferenceValue as T;
            return newProperty != null;
        }

        public static void DrawToggleGroup(Rect totalPosition, GUIContent label, SerializedProperty[] toggles, Rect[] rects = null, bool isAllowSwitchOff = true)
        {
            if (toggles == null)
            {
                return;
            }

            var suffixRect = EditorGUI.PrefixLabel(totalPosition, label);
            if (rects == null)
            {
                float toggleWidth = suffixRect.width / toggles.Length;
                rects = new Rect[toggles.Length];
                for (int i = 0; i < rects.Length; i++)
                {
                    rects[i] = new Rect(suffixRect) {width = toggleWidth, x = suffixRect.x + i * toggleWidth};
                }
            }

            SerializedProperty currentActiveToggle = null;
            foreach (var toggle in toggles)
            {
                if (toggle.boolValue)
                {
                    currentActiveToggle = toggle;
                }
            }

            for (var i = 0; i < toggles.Length; i++)
            {
                var toggle = toggles[i];
                if (EditorGUI.ToggleLeft(rects[i], toggle.displayName, toggle.boolValue))
                {
                    if (toggle != currentActiveToggle)
                    {
                        if (currentActiveToggle != null)
                        {
                            currentActiveToggle.boolValue = false;
                        }

                        currentActiveToggle = toggle;
                    }

                    toggle.boolValue = true;
                }
                else if (!isAllowSwitchOff && currentActiveToggle == null)
                {
                    toggles[0].boolValue = true;
                }
                else
                {
                    toggle.boolValue = false;
                }
            }
        }

        public static int DrawButtonTabsMixedView(Rect position, SerializedProperty property, int selectedTabIndex, float labelTabHeight, TabViewData[] datas)
        {
            DrawFrameBox(position);

            float accumulatedWidth = 0f;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                Rect rect = new Rect(position) {height = labelTabHeight, width = position.width * data.ratio, x = position.x + accumulatedWidth,};
                accumulatedWidth += rect.width;

                GUIStyle style = Uniform.GetTabStyle(i, datas.Length);
                if (data.onTabChanged != null)
                {
                    bool oldState = selectedTabIndex == i;
                    bool newState = GUI.Toggle(rect, oldState, data.label, style);
                    bool isChanged = newState != oldState;
                    if (isChanged && newState) selectedTabIndex = i;

                    if (isChanged) data.onTabChanged.Invoke();
                }
                else if (data.onButtonClick != null)
                {
                    if (GUI.Button(rect, data.label, style)) data.onButtonClick.Invoke(rect, property);
                }
            }

            return selectedTabIndex;
        }

        public static void DrawFrameBox(Rect position)
        {
            if (Event.current.type == EventType.Repaint)
            {
                GUIStyle frameBox = "FrameBox";
                frameBox.Draw(position,
                    false,
                    false,
                    false,
                    false);
            }
        }

        public static float Draw2SidesLabelSliderLayout(MultiLabel labels, float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            float resultValue = EditorGUILayout.Slider(labels.main,
                value,
                leftValue,
                rightValue,
                options);
            var lastRect = GUILayoutUtility.GetLastRect();
            Draw2SidesLabels(lastRect, labels);
            return resultValue;
        }

        public static float Draw2SidesLabelSlider(Rect position, MultiLabel labels, float value, float leftValue, float rightValue)
        {
            float resultValue = EditorGUI.Slider(position,
                labels.main,
                value,
                leftValue,
                rightValue);
            Draw2SidesLabels(position, labels);
            return resultValue;
        }

        public static void Draw2SidesLabels(Rect position, MultiLabel labels)
        {
            var rightWordLength = 30f;
            var leftRect = new Rect(position.x + EditorGUIUtility.labelWidth,
                position.y + AudioConstant.TWO_SIDES_LABEL_OFFSET_Y,
                EditorGUIUtility.fieldWidth,
                position.height);
            var rightRect = new Rect(position.xMax - EditorGUIUtility.fieldWidth - rightWordLength,
                position.y + AudioConstant.TWO_SIDES_LABEL_OFFSET_Y,
                EditorGUIUtility.fieldWidth,
                position.height);

            var lowerLeftMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            lowerLeftMiniLabel.alignment = TextAnchor.LowerLeft;

            EditorGUI.LabelField(leftRect, labels.left, lowerLeftMiniLabel);
            EditorGUI.LabelField(rightRect, labels.right, lowerLeftMiniLabel);
        }

        public static Rect DrawMinMaxLabelAndField(Rect position, GUIContent label, ref float min, ref float max, float fieldWidth, Action<Rect> onGetSliderRect)
        {
            var suffixRect = EditorGUI.PrefixLabel(position, label);
            GetMinMaxRects(suffixRect,
                fieldWidth,
                out var minFieldRect,
                out var sliderRect,
                out var maxFieldRect);
            onGetSliderRect?.Invoke(sliderRect);

            min = EditorGUI.FloatField(minFieldRect, min);
            max = EditorGUI.FloatField(maxFieldRect, max);
            return sliderRect;
        }

        private static void GetMinMaxRects(Rect suffixRect, float fieldWidth, out Rect minFieldRect, out Rect sliderRect, out Rect maxFieldRect)
        {
            var gap = 5f;
            minFieldRect = new Rect(suffixRect) {width = fieldWidth};
            sliderRect = new Rect(suffixRect) {x = minFieldRect.xMax + gap, width = suffixRect.width - (fieldWidth + gap) * 2f};
            maxFieldRect = new Rect(suffixRect) {x = sliderRect.xMax + gap, width = fieldWidth};
        }

        public static float DrawLogarithmicSlider_Horizontal(Rect position, float currentValue, float leftValue, float rightValue, bool isDrawField = true)
        {
            if (leftValue <= 0f)
            {
                //Debug.LogWarning($"The left value of the LogarithmicSlider should be greater than 0. It has been set to the default value of {min}");
                leftValue = Mathf.Max(AudioConstant.LOGARITHMIC_MIN_VALUE, leftValue);
            }

            var sliderRect = new Rect(position);
            if (isDrawField)
            {
                var gap = 5f;
                sliderRect.width -= EditorGUIUtility.fieldWidth + gap;
                var fieldRect = new Rect(sliderRect) {x = sliderRect.xMax + gap, width = EditorGUIUtility.fieldWidth};

                currentValue = Mathf.Clamp(EditorGUI.FloatField(fieldRect, currentValue), leftValue, rightValue);
            }

            currentValue = currentValue == 0 ? leftValue : currentValue;
            float logValue = Mathf.Log10(currentValue);
            float logLeftValue = Mathf.Log10(leftValue);
            float logRightValue = Mathf.Log10(rightValue);

            // only use logResult if the slider is changed.
            EditorGUI.BeginChangeCheck();
            float logResult = GUI.HorizontalSlider(sliderRect, logValue, logLeftValue, logRightValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (Mathf.Approximately(logResult, logLeftValue)) return leftValue;
                if (Mathf.Approximately(logResult, logRightValue)) return rightValue;
                return Mathf.Pow(10, logResult);
            }

            return currentValue;
        }

        public static Rect PolarCoordinates(this Rect rect, float pixel)
        {
            rect.width += pixel * 2;
            rect.height += pixel * 2;
            rect.x -= pixel;
            rect.y -= pixel;
            return rect;
        }

        public static Rect SetWidth(this Rect rect, Func<float, float> onModify) { return new Rect(rect.x, rect.y, onModify(rect.width), rect.height); }

        public static Rect SetHeight(this Rect rect, float height) { return new Rect(rect.x, rect.y, rect.width, height); }

        public static Rect SetHeight(this Rect rect, Func<float, float> onModify) { return new Rect(rect.x, rect.y, rect.width, onModify(rect.height)); }

        public static GUIContent GetPlaybackButtonIcon(bool isPlaying)
        {
            string icon = isPlaying ? "PreMatQuad" : "PlayButton";
            return EditorGUIUtility.IconContent(icon);
        }

        public static void ForeachConcreteDrawedProperty(Action<EDrawedProperty> onGetDrawedProperty)
        {
            for (int i = 0; i < 32; i++) // int32
            {
                var drawFlag = (EDrawedProperty) (1 << i);
                if (drawFlag > EDrawedProperty.All) break;

                if (!EDrawedProperty.All.HasFlagUnsafe(drawFlag)) continue;

                onGetDrawedProperty?.Invoke(drawFlag);
            }
        }

        public static bool TryGetCoreData(out AudioData coreData)
        {
            var result = ProjectDatabase.FindAll<AudioData>();
            if (result.Count > 0)
            {
                coreData = result[0];
                return true;
            }

            coreData = null;
            return false;
        }

        public static void WriteAssetOutputPathToSetting(string path)
        {
            AudioEditorSetting.AssetOutputPath = path;
            SaveToDisk(AudioEditorSetting.Instance);
        }

        public static void AddNewAssetToCoreData(ScriptableObject asset)
        {
            if (TryGetCoreData(out var coreData))
            {
                coreData.AddAsset(asset as AudioAsset);
                SaveToDisk(coreData);
            }
        }

        public static void RemoveEmptyDatas()
        {
            if (TryGetCoreData(out AudioData coreData))
            {
                coreData.RemoveEmpty();
                SaveToDisk(coreData);
            }
        }

        public static void ReorderAssets(List<string> allAssetGUIDs)
        {
            if (TryGetCoreData(out var coreData))
            {
                coreData.ReorderAssets(allAssetGUIDs);
                SaveToDisk(coreData);
            }
        }

        private static void SaveToDisk(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}