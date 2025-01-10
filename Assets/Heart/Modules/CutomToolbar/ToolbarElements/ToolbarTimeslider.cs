using System;
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

    public ToolbarTimeslider(float minTime = 0.0f, float maxTime = 10.0f, float width = 200f)
        : base(width)
    {
        this.minTime = minTime;
        this.maxTime = maxTime;
    }

    protected override void OnDrawInList(Rect position)
    {
        
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUILayout.LabelField("Time Scale", GUILayout.Width(64));
        Time.timeScale = EditorGUILayout.Slider("",
            Time.timeScale,
            minTime,
            maxTime,
            GUILayout.Width(WidthInToolbar - 30.0f));
    }
}