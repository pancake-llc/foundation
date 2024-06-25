using System;
using System.IO;
using Pancake.Linq;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public static class EditorAudioEx
    {
        public enum RandomRangeSliderType
        {
            Default,
            Logarithmic,
            Volume,
        }

        private const float LOW_VOLUME_SNAPPING_THRESHOLD = 0.05f;
        private const float HIGH_VOLUME_SNAPPING_THRESHOLD = 0.2f;
        private const string DB_VALUE_STRING_FORMAT = "0.##";
        public const float TWO_SIDES_LABEL_OFFSET_Y = 7f;
        public const float INDENT_IN_PIXEL = 15f;
        public const float LOGARITHMIC_MIN_VALUE = 0.0001f;
        private static Vector2 VolumeLabelSize => new(55f, 25f);
        public static readonly float[] VolumeSplitPoints = {-80f, -60f, -36f, -24f, -12f, -6f, 0f, 6f, 20f};
        public static Action onCloseWindow;
        public static Action onSelectAsset;

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

        public static void AddToSoundManager(ScriptableObject asset)
        {
            var prefab = ProjectDatabase.FindAll<GameObject>().Filter(go => go.TryGetComponent<SoundManager>(out _)).First();
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editScope = new EditPrefabAssetScope(assetPath))
            {
                if (editScope.prefabRoot.TryGetComponent<SoundManager>(out var soundManager)) soundManager.AddAsset(asset);
                editScope.prefabRoot.transform.position = Vector3.one;
            }
        }

        public static void DeleteAssetRelativeData(string[] deletedAssetPaths)
        {
            if (deletedAssetPaths == null || deletedAssetPaths.Length == 0) return;

            DeleteJsonDataByAssetPath(deletedAssetPaths);

            var prefab = ProjectDatabase.FindAll<GameObject>().Filter(go => go.TryGetComponent<SoundManager>(out _)).First();
            string assetPath = AssetDatabase.GetAssetPath(prefab);

            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return;

            using (var editScope = new EditPrefabAssetScope(assetPath))
            {
                if (editScope.prefabRoot.TryGetComponent<SoundManager>(out var soundManager))
                {
                    foreach (string path in deletedAssetPaths)
                    {
                        if (!string.IsNullOrEmpty(path) && TryGetAssetByPath(path, out var asset))
                        {
                            var scriptableObject = asset as ScriptableObject;
                            soundManager.RemoveDeletedAsset(scriptableObject);
                        }
                        else soundManager.RemoveDeletedAsset(null);
                    }
                }
            }
        }

        private static void DeleteJsonDataByAssetPath(string[] deletedAssetPaths)
        {
            foreach (string path in deletedAssetPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    LibraryDataContainer.Data.Settings.guids.Remove(guid);
                }
            }

            LibraryDataContainer.Data.SaveSetting();
        }

        private static bool TryGetAssetByPath(string assetPath, out IAudioAsset asset)
        {
            asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as IAudioAsset;
            return asset != null;
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
            property.FindPropertyRelative(SoundClip.EditorPropertyName.AudioClip).objectReferenceValue = null;
            property.FindPropertyRelative(SoundClip.EditorPropertyName.Weight).intValue = 0;
            ResetAudioClipPlaybackSetting(property);
        }

        public static void ResetAudioClipPlaybackSetting(SerializedProperty property)
        {
            property.FindPropertyRelative(SoundClip.EditorPropertyName.Volume).floatValue = AudioConstant.FULL_VOLUME;
            property.FindPropertyRelative(SoundClip.EditorPropertyName.StartPosition).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.EditorPropertyName.EndPosition).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.EditorPropertyName.FadeIn).floatValue = 0f;
            property.FindPropertyRelative(SoundClip.EditorPropertyName.FadeOut).floatValue = 0f;
        }

        public static void ResetEntitySerializedProperties(SerializedProperty property)
        {
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.Name).stringValue = string.Empty;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.Clips).arraySize = 0;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.AudioPlayMode).enumValueIndex = 0;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.MasterVolume).floatValue = AudioConstant.FULL_VOLUME;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.Loop).boolValue = false;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.SeamlessLoop).boolValue = false;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.Pitch).floatValue = AudioConstant.DEFAULT_PITCH;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.PitchRandomRange).floatValue = 0f;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.RandomFlags).intValue = 0;
            property.FindPropertyRelative(AudioEntity.EditorPropertyName.Priority).intValue = AudioConstant.DEFAULT_PRIORITY;

            var spatialProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SpatialSetting);
            spatialProp.objectReferenceValue = null;
            spatialProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static float VolumeScale => 1f / (VolumeSplitPoints.Length - 1);

        private static float SliderFullScale =>
            AudioConstant.FULL_VOLUME / ((AudioConstant.FULL_DECIBEL_VOLUME - AudioConstant.MIN_DECIBEL_VOLUME) / AudioConstant.DecibelVoulumeFullScale);

        public static bool IsTempReservedName(string name)
        {
            string tempName = AudioConstant.TEMP_ASSET_NAME;
            return (name.Length == tempName.Length || name.Length > tempName.Length && char.IsNumber(name[tempName.Length])) &&
                   name.StartsWith(tempName, StringComparison.Ordinal);
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
                    LibraryDataContainer.Data.Settings.assetOutputPath = newPath;
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

        public static float DrawVolumeSlider(Rect position, GUIContent label, float currentValue)
        {
            return DrawVolumeSlider(position,
                label,
                currentValue,
                false,
                null);
        }

        public static float DrawVolumeSlider(Rect position, GUIContent label, float currentValue, bool isSnap, Action onSwitchSnapMode)
        {
            var suffixRect = EditorGUI.PrefixLabel(position, label);
            const float padding = 3f;
            var sliderRect = new Rect(suffixRect) {width = suffixRect.width - EditorGUIUtility.fieldWidth - padding};
            var fieldRect = new Rect(suffixRect) {x = sliderRect.xMax + padding, width = EditorGUIUtility.fieldWidth};

#if !UNITY_WEBGL
            if (AudioEditorSetting.ShowVuColorOnVolumeSlider) DrawVuMeter(sliderRect);

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
                if (onSwitch == null)
                {
                    return;
                }

                float sliderValue = VolumeToSlider(AudioConstant.FULL_VOLUME);

                var rect = new Rect(sliderPosition) {width = 30f};
                rect.x = sliderPosition.x + sliderPosition.width * sliderValue - (rect.width * 0.5f) + 1f; // add 1 pixel for more precise position
                rect.y -= sliderPosition.height;
                var icon = new GUIContent(EditorGUIUtility.IconContent("SignalAsset Icon")) {tooltip = "Toggle full volume snapping"};
                EditorGUI.BeginDisabledGroup(!isSnap);
                GUI.Label(rect, icon);
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

        public static void DrawDecibelValuePeeking(float currentValue, float padding, Rect sliderRect, float sliderValue)
        {
            if (Event.current.type == EventType.Repaint)
            {
                float sliderHandlerPos = sliderValue / SliderFullScale * sliderRect.width - (VolumeLabelSize.x * 0.5f);
                if (sliderRect.Contains(Event.current.mousePosition))
                {
                    var valueTooltipRect = new Rect(sliderRect.x + sliderHandlerPos, sliderRect.y - VolumeLabelSize.y - padding, VolumeLabelSize.x, VolumeLabelSize.y);
                    GUI.skin.window.Draw(valueTooltipRect,
                        false,
                        false,
                        false,
                        false);
                    float dBvalue = currentValue.ToDecibel();
                    string plusSymbol = dBvalue > 0 ? "+" : string.Empty;
                    string volText = plusSymbol + dBvalue.ToString(DB_VALUE_STRING_FORMAT) + "dB";
                    // ** Don't use EditorGUI.Label(), it will change the keyboard focus, might be a Unity's bug **
                    GUI.Label(valueTooltipRect, volText, Uniform.CenterRichLabel);
                }
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
            var sliderRect = DrawMinMaxLabelAndField(position,
                label,
                ref min,
                ref max,
                fieldWidth,
                onGetSliderRect);
            float minSlider = VolumeToSlider(min);
            float maxSlider = VolumeToSlider(max);

            EditorGUI.MinMaxSlider(sliderRect,
                ref minSlider,
                ref maxSlider,
                0f,
                1f);

            min = SliderToVolume(minSlider);
            max = SliderToVolume(maxSlider);
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
                if (char.IsUpper(propertyName[0])) propertyName = propertyName.Replace(propertyName[0], Pancake.Common.C.ToLower(propertyName[0]));
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

        public static void DrawToggleGroup(Rect totalPosition, GUIContent label, SerializedProperty[] toggles, bool isAllowSwitchOff = true, int toggleCountPerLine = 4)
        {
            if (toggles == null)
            {
                return;
            }

            var suffixRect = EditorGUI.PrefixLabel(totalPosition, label);
            float space = suffixRect.width / toggleCountPerLine;
            var toggleRect = new Rect(suffixRect);
            toggleRect.width = space;

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
                if (EditorGUI.ToggleLeft(toggleRect, toggle.displayName, toggle.boolValue))
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


                toggleRect.x += space;
                toggleRect.y += (i / toggleCountPerLine) * EditorGUIUtility.singleLineHeight;
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
            var leftRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + TWO_SIDES_LABEL_OFFSET_Y, EditorGUIUtility.fieldWidth, position.height);
            var rightRect = new Rect(position.xMax - EditorGUIUtility.fieldWidth - rightWordLength,
                position.y + TWO_SIDES_LABEL_OFFSET_Y,
                EditorGUIUtility.fieldWidth,
                position.height);

            var lowerLeftMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            lowerLeftMiniLabel.alignment = TextAnchor.LowerLeft;

            EditorGUI.LabelField(leftRect, labels.left, lowerLeftMiniLabel);
            EditorGUI.LabelField(rightRect, labels.right, lowerLeftMiniLabel);
        }

        public static int DrawTabsView(Rect position, int selectedTabIndex, float labelTabHeight, GUIContent[] labels, float[] ratios, Rect[] preAllocRects = null)
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

            // draw tab label
            var tabRect = new Rect(position) {height = labelTabHeight};

            var tabRects = preAllocRects ?? new Rect[ratios.Length];
            Uniform.SplitRectHorizontal(tabRect, 0f, tabRects, ratios);
            for (var i = 0; i < tabRects.Length; i++)
            {
                bool oldState = selectedTabIndex == i;
                bool newState = GUI.Toggle(tabRects[i], oldState, labels[i], Uniform.GetTabStyle(i, tabRects.Length));
                bool isChanged = newState != oldState;
                if (isChanged && newState)
                {
                    selectedTabIndex = i;
                }

                if (isChanged) EditorPlayAudioClip.StopAllClips();
            }

            return selectedTabIndex;
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
            var sliderRect = DrawMinMaxLabelAndField(position,
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
            var sliderRect = DrawMinMaxLabelAndField(position,
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
                leftValue = Mathf.Max(LOGARITHMIC_MIN_VALUE, leftValue);
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
    }
}