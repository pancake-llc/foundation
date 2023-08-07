using System;
using UnityEngine;
using UnityEditor;
#if PANCAKE_SPINE
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pancake;
using Pancake.ExLibEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using Animation = Spine.Animation;
using Event = UnityEngine.Event;
#endif

namespace PancakeEditor
{
    public static class UtilitiesSpineDrawer
    {
        public static void OnInspectorGUI(Action repaint, Rect position)
        {
#if PANCAKE_SPINE
            void DrawButtonUninstall()
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.Space();
                GUI.backgroundColor = Uniform.Red;
                if (GUILayout.Button("Uninstall Spine", GUILayout.MaxHeight(25f)))
                {
                    bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Spine", "Are you sure you want to uninstall spine package ?", "Yes", "No");
                    if (confirmDelete)
                    {
                        RegistryManager.Remove("com.esotericsoftware.spine.spine-csharp");
                        RegistryManager.Remove("com.esotericsoftware.spine.spine-unity");
                        RegistryManager.Resolve();
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            
            onRepaint = repaint;

            Uniform.DrawInstalled("4.1");

            var selectionSkeletonAnimation = (Selection.activeObject as GameObject)?.GetComponent<SkeletonAnimation>();
            var selectionSkeletonGraphic = (Selection.activeObject as GameObject)?.GetComponent<SkeletonGraphic>();

            if (selectionSkeletonAnimation == null && selectionSkeletonGraphic == null)
            {
                EditorGUILayout.HelpBox("Please select GameObject has Skeleton Animation or Skeleton Graphic to continue!", MessageType.Warning);
                DrawButtonUninstall();
                return;
            }

            autoRun = EditorGUILayout.Toggle("Auto Run", autoRun, GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected skeleton", GUILayout.Width(140));
            GUI.enabled = false;
            skeletonAnimation = null;
            skeletonGraphic = null;

            if (selectionSkeletonAnimation != null)
            {
                skeletonAnimation = selectionSkeletonAnimation;
                EditorGUILayout.ObjectField(Selection.activeObject, typeof(SkeletonAnimation), true);
            }

            if (selectionSkeletonAnimation == null && selectionSkeletonGraphic != null)
            {
                skeletonGraphic = selectionSkeletonGraphic;
                EditorGUILayout.ObjectField(Selection.activeObject, typeof(SkeletonGraphic), true);
            }

            GUI.enabled = true;

            if (skeletonAnimation != null)
            {
                var s = (SkeletonDataAsset) EditorGUILayout.ObjectField("", skeletonAnimation.skeletonDataAsset, typeof(SkeletonDataAsset), true);
                if (s != skeletonAnimation.skeletonDataAsset) skeletonAnimation.skeletonDataAsset = s;
            }

            if (skeletonGraphic != null)
            {
                var s = (SkeletonDataAsset) EditorGUILayout.ObjectField("", skeletonGraphic.skeletonDataAsset, typeof(SkeletonDataAsset), true);
                if (s != skeletonGraphic.skeletonDataAsset) skeletonGraphic.skeletonDataAsset = s;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Frame : {(int) (playTime * 30)}", GUILayout.Width(140), GUILayout.ExpandWidth(false));
            if (currentAnim != null) playTime = EditorGUILayout.Slider(playTime, 0, currentAnim.Duration);
            else EditorGUILayout.Slider(playTime, 0, 0);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            if (autoRun)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Speed", GUILayout.Width(140));
                timeScale = EditorGUILayout.Slider(timeScale, 0, 2f);
                EditorGUILayout.EndHorizontal();
                if (currentAnimState != null) currentAnimState.TimeScale = timeScale;
            }

            EditorGUILayout.EndVertical();

            Draw(position);

            DrawButtonUninstall();
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Spine 4.1", GUILayout.MaxHeight((40f))))
            {
                RegistryManager.Add("com.esotericsoftware.spine.spine-csharp", "https://github.com/EsotericSoftware/spine-runtimes.git?path=spine-csharp/src#4.1");
                RegistryManager.Add("com.esotericsoftware.spine.spine-unity", "https://github.com/EsotericSoftware/spine-runtimes.git?path=spine-unity/Assets/Spine#4.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }

#if PANCAKE_SPINE
        private static Animation currentAnim;
        private static Spine.AnimationState currentAnimState;
        private static Skeleton currentSkeleton;
        private static SkeletonDataAsset currentDataAsset;
        private static SkeletonAnimation skeletonAnimation;
        private static SkeletonGraphic skeletonGraphic;
        private static Vector2 skinScrollPos;
        private static Vector2 animScrollPos;
        private static List<Skin> currentSkins = new();
        private static bool isNeedReload;
        private static Animation[] animations;
        private static Skin[] skins;
        private static bool autoRun;
        private static float animationLastTime;
        private static float playTime;
        private static float timeScale = 1f;
        private static Action onRepaint;
        private static string searchAnimation = string.Empty;
        private static string searchAnimationPrevious = string.Empty;
        private static string searchSkin = string.Empty;
        private static string searchSkinPrevious = string.Empty;
        private static bool open;
        private static bool mixSkin;


        public static void Clear()
        {
            EditorApplication.update -= HandleEditorUpdate;
            Selection.selectionChanged -= SelectionChanged;
        }

        private static void Draw(Rect position)
        {
            EditorApplication.update -= HandleEditorUpdate;
            EditorApplication.update += HandleEditorUpdate;
            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;

            if (!SessionState.GetBool("spine_flag", false))
            {
                SessionState.SetBool("spine_flag", true);
                SelectionChanged();
            }

            if (Application.isPlaying) return;

            if (currentSkeleton != null)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
                EditorGUILayout.BeginHorizontal();
                DrawLeftSide(position);
                DrawRightSide();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (isNeedReload)
                {
                    isNeedReload = false;
                    if (skeletonAnimation != null)
                    {
                        skeletonAnimation.ClearState();
                        if (currentAnim != null) skeletonAnimation.AnimationName = currentAnim.Name;

                        skeletonAnimation.initialSkinName = skeletonAnimation.skeleton.Data.Skins.Items[0].Name;
                        skeletonAnimation.LateUpdate();
                        skeletonAnimation.Initialize(true);
                        currentAnimState = skeletonAnimation.state;
                        currentAnimState.TimeScale = timeScale;
                        skeletonAnimation.gameObject.SetActive(false);
                        skeletonAnimation.gameObject.SetActive(true);
                        ChangeSkin();
                    }
                    else if (skeletonGraphic != null)
                    {
                        skeletonGraphic.Clear();
                        if (currentAnim != null) skeletonGraphic.startingAnimation = currentAnim.Name;
                        skeletonGraphic.AnimationState.SetAnimation(0, currentAnim.Name, false);
                        skeletonGraphic.LateUpdate();
                        skeletonGraphic.Initialize(true);
                        currentAnimState.TimeScale = timeScale;
                        skeletonGraphic.gameObject.SetActive(false);
                        skeletonGraphic.gameObject.SetActive(true);
                        currentAnimState = skeletonGraphic.AnimationState;
                        ChangeSkin();
                    }

                    onRepaint?.Invoke();
                }
            }
        }

        private static void DrawLeftSide(Rect position)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));
            searchAnimation = EditorGUILayout.TextField(searchAnimation, EditorStyles.toolbarSearchField);
            animScrollPos = EditorGUILayout.BeginScrollView(animScrollPos);
            int count = 0;
            if (searchAnimation != searchAnimationPrevious)
            {
                searchAnimationPrevious = searchAnimation;
                animations = currentSkeleton.Data.Animations.Items.Where(_ => _.Name.ToLower().Contains(searchAnimation.ToLower())).ToArray();
            }

            foreach (var a in animations)
            {
                if (a == currentAnim)
                    GUILayout.BeginHorizontal(new GUIStyle {normal = new GUIStyleState {background = EditorCreator.CreateTexture(new Color(0.36f, 0.36f, 0.36f))}});
                else GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(new GUIContent(SpineEditorUtilities.Icons.animation), GUILayout.Width(18));
                EditorGUILayout.LabelField(a.Name,
                    new GUIStyle {normal = new GUIStyleState {textColor = Color.white}, alignment = TextAnchor.MiddleLeft, margin = new RectOffset(5, 0, 0, 0)},
                    GUILayout.Height(24), GUILayout.Width(150));
                EditorGUILayout.LabelField(a.Duration.ToString(CultureInfo.InvariantCulture),
                    new GUIStyle
                    {
                        normal = new GUIStyleState {textColor = new Color(0.48f, 0.48f, 0.48f)},
                        alignment = TextAnchor.MiddleRight,
                        margin = new RectOffset(5, 5, 0, 0)
                    },
                    GUILayout.Height(24), GUILayout.ExpandWidth(true), GUILayout.Width(100));

                GUILayout.EndHorizontal();
                var lastRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                {
                    currentAnim = a;
                    isNeedReload = true;
                    if (Event.current.control)
                    {
                        EditorGUIUtility.systemCopyBuffer = a.Name;
                    }
                }

                onRepaint?.Invoke();
                count++;
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static void DrawRightSide()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            searchSkin = EditorGUILayout.TextField(searchSkin, EditorStyles.toolbarSearchField);
            EditorGUILayout.LabelField("Mix", GUILayout.Width(24));
            mixSkin = EditorGUILayout.Toggle("", mixSkin, GUILayout.Width(16));
            EditorGUILayout.EndHorizontal();
            skinScrollPos = EditorGUILayout.BeginScrollView(skinScrollPos);
            int count = 0;
            if (searchSkin != searchSkinPrevious)
            {
                searchSkinPrevious = searchSkin;
                skins = currentSkeleton.Data.Skins.Items.Where(_ => _.Name.ToLower().Contains(searchSkin.ToLower())).ToArray();
            }

            foreach (var s in skins)
            {
                if (currentSkins.Contains(s))
                {
                    EditorGUILayout.BeginHorizontal(new GUIStyle {normal = new GUIStyleState {background = EditorCreator.CreateTexture(new Color(0.36f, 0.36f, 0.36f))}});
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                }

                EditorGUILayout.LabelField(new GUIContent(SpineEditorUtilities.Icons.skin), GUILayout.ExpandWidth(false), GUILayout.Width(32));
                EditorGUILayout.LabelField($"{s.Name}",
                    new GUIStyle {normal = new GUIStyleState {textColor = Color.white}, alignment = TextAnchor.MiddleLeft, margin = new RectOffset(5, 0, 0, 0)},
                    GUILayout.Height(24));

                EditorGUILayout.EndHorizontal();
                var lastRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                {
                    if (mixSkin)
                    {
                        if (!currentSkins.Contains(s))
                        {
                            currentSkins.Add(s);
                        }
                        else
                        {
                            currentSkins.Remove(s);
                        }
                    }
                    else
                    {
                        currentSkins.Clear();
                        currentSkins.Add(s);
                    }

                    ChangeSkin();
                    if (Event.current.control)
                    {
                        EditorGUIUtility.systemCopyBuffer = s.Name;
                    }

                    onRepaint?.Invoke();
                }

                count++;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static void SelectionChanged()
        {
            UpdateSkeleton();
            UpdateGraphic();
            onRepaint?.Invoke();
        }

        private static void ChangeSkin()
        {
            if (currentSkins.Count <= 0) return;

            var skinTemp = new Skin("temp");
            foreach (var skin in currentSkins)
            {
                skinTemp.AddSkin(skin);
            }

            if (skeletonAnimation != null)
            {
                skeletonAnimation.initialSkinName = "temp";
                skeletonAnimation.skeleton.SetSkin(skinTemp);
                skeletonAnimation.skeleton.SetSlotsToSetupPose();
                skeletonAnimation.LateUpdate();
                skeletonAnimation.AnimationState.Apply(skeletonAnimation.skeleton);
            }
            else if (skeletonGraphic != null)
            {
                skeletonGraphic.Skeleton.SetSkin(skinTemp);
                skeletonGraphic.Skeleton.SetSlotsToSetupPose();
                skeletonGraphic.LateUpdate();
            }
        }

        private static void HandleEditorUpdate()
        {
            if (currentAnim != null && currentAnimState?.GetCurrent(0) != null)
            {
                if (skeletonAnimation != null && currentDataAsset != skeletonAnimation.skeletonDataAsset)
                {
                    currentDataAsset = skeletonAnimation.skeletonDataAsset;
                    skeletonAnimation.initialSkinName = "default";
                    skeletonAnimation.Initialize(true);
                    UpdateSkeleton();
                }
                else if (skeletonGraphic != null && currentDataAsset != skeletonGraphic.skeletonDataAsset)
                {
                    currentDataAsset = skeletonGraphic.skeletonDataAsset;
                    skeletonGraphic.initialSkinName = "default";
                    skeletonGraphic.Initialize(true);
                    UpdateGraphic();
                }

                if (autoRun)
                {
                    float deltaTime = (float) EditorApplication.timeSinceStartup - animationLastTime;
                    currentAnimState.Update(deltaTime);
                    animationLastTime = (float) EditorApplication.timeSinceStartup;
                    currentAnimState.TimeScale = timeScale;
                    var currentTrack = currentAnimState.GetCurrent(0);
                    if (currentTrack != null && currentTrack.TrackTime >= currentAnim.Duration) currentTrack.TrackTime = 0;

                    if (skeletonAnimation != null)
                    {
                        skeletonAnimation.LateUpdate();
                        skeletonAnimation.Skeleton.UpdateWorldTransform();
                    }

                    if (skeletonGraphic != null)
                    {
                        skeletonGraphic.LateUpdate();
                        skeletonGraphic.Skeleton.UpdateWorldTransform();
                        skeletonGraphic.UpdateMesh();
                    }

                    if (currentTrack != null) playTime = currentTrack.TrackTime;
                    onRepaint?.Invoke();
                }
                else
                {
                    animationLastTime = (float) EditorApplication.timeSinceStartup;
                    currentAnimState.TimeScale = timeScale;
                    var currentTrack = currentAnimState.GetCurrent(0);
                    if (currentTrack != null) currentTrack.TrackTime = playTime;
                    currentAnimState.Update(0);

                    if (skeletonAnimation != null)
                    {
                        skeletonAnimation.LateUpdate();
                        skeletonAnimation.Skeleton.UpdateWorldTransform();
                    }

                    if (skeletonGraphic != null)
                    {
                        skeletonGraphic.LateUpdate();
                        skeletonGraphic.Skeleton.UpdateWorldTransform();
                        skeletonGraphic.UpdateMesh();
                    }
                }
            }
        }

        private static void UpdateSkeleton()
        {
            skeletonAnimation = (Selection.activeObject as GameObject)?.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null) return;

            currentSkins.Clear();
            currentSkeleton = skeletonAnimation.Skeleton;
            UpdateListView();
            isNeedReload = true;
            currentAnim = currentSkeleton.Data.Animations.Items[0];
            skeletonAnimation.initialSkinName = skeletonAnimation.skeletonDataAsset.GetSkeletonData(true).Skins.Items[0].Name;
            currentSkeleton.SetSkin(skeletonAnimation.initialSkinName);
            currentAnimState = skeletonAnimation.AnimationState;
        }

        private static void UpdateListView()
        {
            animations = currentSkeleton.Data.Animations.Items;
            skins = currentSkeleton.Data.Skins.Items;
        }

        private static void UpdateGraphic()
        {
            skeletonGraphic = (Selection.activeObject as GameObject)?.GetComponent<SkeletonGraphic>();
            if (skeletonGraphic == null) return;

            currentSkins.Clear();
            currentSkeleton = skeletonGraphic.Skeleton;
            UpdateListView();
            isNeedReload = true;
            currentAnim = currentSkeleton.Data.Animations.Items[0];
            skeletonGraphic.initialSkinName = skeletonGraphic.skeletonDataAsset.GetSkeletonData(true).Skins.Items[0].Name;
            currentSkeleton.SetSkin(skeletonGraphic.initialSkinName);
            currentAnimState = skeletonGraphic.AnimationState;
        }

#endif
    }
}