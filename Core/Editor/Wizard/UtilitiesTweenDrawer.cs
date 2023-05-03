using System.Collections;
using System.Collections.Generic;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Tween;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesTweenDrawer
    {
        public static void OnInspectorGUI()
        {
            var tweenUpdates = new List<Tween>(0);
            var tweenCacheds = Tween.GetTweenCachedStack();
            var tweenUnrecycled = Tween.GetTweenUnrecycledList();
            if (Application.isPlaying)
            {
                // not visit TweenManager when Application is not playing
                // otherwise, the jobHandleUpdateList of TweenManager will
                // cause a dispose error when Application is playing
                tweenUpdates = TweenManager.GettweenUpdateList();
            }
            else
            {
                EditorGUILayout.HelpBox(" All Tweens information will be displayed at runtime. ", MessageType.Info);
            }

            EditorGUILayout.Space();
            Draw("all_tween_update", tweenUpdates, "All Update Tween : {0}", true);
            EditorGUILayout.Space();
            Draw("all_tween_cached", tweenCacheds, "All Cached Tween : {0}", false);
            EditorGUILayout.Space();
            Draw("all_tween_unrecycled", tweenUnrecycled, "All Unrecycled Tween : {0}", false);
            EditorGUILayout.Space();
            DrawActionInfoCollection();
        }

        private static void Draw(string key, ICollection tweens, string label, bool isActionShowInfo)
        {
            Uniform.DrawGroupFoldout(key,
                string.Format(label, tweens.Count),
                () =>
                {
                    int index = -1;
                    foreach (Tween tween in tweens)
                    {
                        DrawTweenInfo(tween, ++index, tween.GetCurStateName(), isActionShowInfo);
                    }
                },
                isShowContent: tweens.Count != 0);
        }

        /// <summary>
        /// Draws the tween info.
        /// </summary>
        /// <param name="tween"></param>
        /// <param name="tweenIndex"></param>
        /// <param name="label"></param>
        /// <param name="isActionShowInfo"></param>
        private static void DrawTweenInfo(Tween tween, int tweenIndex, string label, bool isActionShowInfo)
        {
            var actions = tween.GetActionList();
            float tweenCurTime = tween.GetCurTime();
            var loopCount = TweenExtensions.GetTweenLoopCount(tween);
            label = $"Tween[{tweenIndex}][{label}]: actions = {actions.Count}";

            if (loopCount != Vector2.zero) label += $" loops = {loopCount.x} / {loopCount.y}";

            label += $" progress = {tweenCurTime:00} / {tween.Duration:0.0}";
            Uniform.DrawGroupFoldout($"all_tween_update_{tweenIndex}",
                label,
                () =>
                {
                    float tweenProgress = Math.Min(tweenCurTime / tween.Duration, 1f);
                    EditorGUI.ProgressBar(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false)), tweenProgress, $"{tweenProgress:0%}");
                    DrawActionInfo(tween, tweenCurTime, actions);
                },
                isShowContent: isActionShowInfo && actions.Count != 0);
        }

        /// <summary>
        /// Draw the action info.
        /// </summary>
        /// <param name="tween"></param>
        /// <param name="tweenCurTime"></param>
        /// <param name="actions"></param>
        private static void DrawActionInfo(Tween tween, float tweenCurTime, List<TweenAction> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                float actionCurTime = Math.Clamp(tweenCurTime - action.TimelineStart, 0.0f, action.Duration);
                float progress = Math.Min(actionCurTime / action.Duration, 1.0f);
                EditorGUILayout.LabelField($"Action[{i}]: " + $"timeline = [{action.TimelineStart:0.0}, {action.TimelineEnd:0.0}] " +
                                           $"progress = {actionCurTime:0.00} / {action.Duration:0.0}");
                var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false));
                float timeWidth = rect.width / tween.Duration;
                rect.x += action.TimelineStart * timeWidth;

                if (action.Duration != 0)
                {
                    rect.width = action.Duration * timeWidth;
                }
                else
                {
                    rect.x -= 8.0f;
                    rect.width = 8.0f;
                    progress = tweenCurTime < action.TimelineStart ? 0f : 1f;
                }

                EditorGUI.ProgressBar(rect, progress, "");
            }
        }

        /// <summary>
        /// Draws the collection of action info.
        /// </summary>
        private static void DrawActionInfoCollection()
        {
            var actions = TweenAction.GetActionCachedStack();
            var label = $" All Cached Actions: {actions.Count} ";
            Uniform.DrawGroupFoldout("all_action_cached",
                label,
                () =>
                {
                    var index = -1;
                    foreach (TweenAction action in actions)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField($"Action[{++index}]: " + $"timeline = [{action.TimelineStart:0.0}, {action.TimelineEnd:0.0}] " +
                                                   $"duration = {action.Duration:0.0}");
                        EditorGUILayout.EndVertical();
                    }
                },
                isShowContent: actions.Count != 0);
        }
    }
}