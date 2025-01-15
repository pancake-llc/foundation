using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
internal class ToolbarTimeslider : BaseToolbarElement
{
    [SerializeField] private float minTime;
    [SerializeField] private float maxTime;

    public override string NameInList => "[Slider] Timescale";
    public override int SortingGroup => 1;

    public override void Init() { }

    public ToolbarTimeslider(float minTime = 0.0f, float maxTime = 10.0f)
        : base(200)
    {
        this.minTime = minTime;
        this.maxTime = maxTime;
    }

    protected override void OnDrawInList(Rect position)
    {
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Min Time");

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = 50.0f;
        minTime = EditorGUI.FloatField(position, "", minTime);

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Max Time");

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = 50.0f;
        maxTime = EditorGUI.FloatField(position, "", maxTime);
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUILayout.LabelField("Time", GUILayout.Width(30));
        Time.timeScale = EditorGUILayout.Slider("",
            Time.timeScale,
            minTime,
            maxTime,
            GUILayout.Width(widthInToolbar - 30.0f));
    }
}