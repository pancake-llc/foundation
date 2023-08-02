using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Animation = Spine.Animation;

namespace PancakeEditor
{
    public static class UtilitiesSpineDrawer
    {
        public static void OnInspectorGUI(Action repaint)
        {
            onRepaint = repaint;
#if PANCAKE_SPINE
            Uniform.DrawInstalled("4.1");
            
            var selectionSkeletonAnimation = (Selection.activeObject as GameObject)?.GetComponent<SkeletonAnimation>();
            var selectionSkeletonGraphic = (Selection.activeObject as GameObject)?.GetComponent<SkeletonGraphic>();

            if (selectionSkeletonAnimation == null && selectionSkeletonGraphic == null)
            {
                EditorGUILayout.LabelField("Please select GameObject has Skeleton Animation or Skeleton Graphic to continue!");
                return;
            }
            
            autoRun = EditorGUILayout.Toggle("Auto Run", autoRun, GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected skeleton : ", GUILayout.Width(140));
            GUI.enabled = false;
            skeletonAnimation = null;
            skeletonGraphic = null;
            
            if (selectionSkeletonAnimation != null)
                skeletonAnimation = (EditorGUILayout.ObjectField(Selection.activeObject, typeof(SkeletonAnimation), true) as GameObject)?.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null)
            {
                if (selectionSkeletonGraphic != null) skeletonGraphic = (EditorGUILayout.ObjectField(Selection.activeObject, typeof(SkeletonGraphic), true) as GameObject)?.GetComponent<SkeletonGraphic>();
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
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical();
            if (autoRun)
            {
                EditorGUILayout.LabelField("Speed");
                timeScale = EditorGUILayout.Slider(timeScale, 0, 2f);
                currentAnimState.TimeScale = timeScale;
            }

            if (currentAnim != null) playTime = EditorGUILayout.Slider(playTime, 0, currentAnim.Duration);
            else EditorGUILayout.Slider(playTime, 0, 0);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();
            Draw();

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

        private static Spine.Animation currentAnim;
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
        public static bool autoRun;
        private static float animationLastTime;
        private static float playTime;
        private static float timeScale;
        private static Action onRepaint;

        private static void Draw()
        {
            EditorApplication.update -= HandleEditorUpdate;
            EditorApplication.update += HandleEditorUpdate;
            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;

            if (Application.isPlaying) return;

            if (currentSkeleton != null)
            {
                //DrawRightSide();
                //DrawLeftSide();
            }
        }

        private static void SelectionChanged() { }

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
            currentSkins.Clear();
            currentSkeleton = skeletonGraphic.Skeleton;
            UpdateListView();
            isNeedReload = true;
            currentAnim = currentSkeleton.Data.Animations.Items[0];
            skeletonGraphic.initialSkinName = skeletonGraphic.skeletonDataAsset.GetSkeletonData(true).Skins.Items[0].Name;
            currentSkeleton.SetSkin(skeletonGraphic.initialSkinName);
            currentAnimState = skeletonGraphic.AnimationState;
        }
    }
}