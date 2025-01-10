using System;
using UnityEngine;
using UnityEditor;

[Serializable]
internal abstract class BaseToolbarElement : IComparable<BaseToolbarElement>
{
    public abstract string NameInList { get; }
    public virtual int SortingGroup { get; }

    [SerializeField] protected bool IsEnabled = true;
    [SerializeField] protected float WidthInToolbar;

    protected const float FIELD_SIZE_SPACE = 10.0f;
    protected const float FIELD_SIZE_SINGLE_CHAR = 7.0f;
    protected const float FIELD_SIZE_WIDTH = 50.0f;

    public BaseToolbarElement()
        : this(100.0f)
    {
        //Init();
    }

    public BaseToolbarElement(float widthInToolbar) { WidthInToolbar = widthInToolbar; }

    public void DrawInList(Rect position)
    {
        position.y += 2;
        position.height -= 4;

        position.x += FIELD_SIZE_SPACE;
        position.width = 15.0f;
        IsEnabled = EditorGUI.Toggle(position, IsEnabled);

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = 200.0f;
        EditorGUI.LabelField(position, NameInList);

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = FIELD_SIZE_SINGLE_CHAR * 4;
        EditorGUI.LabelField(position, "Size");

        position.x += position.width + FIELD_SIZE_SPACE;
        position.width = FIELD_SIZE_WIDTH;
        WidthInToolbar = EditorGUI.IntField(position, (int) WidthInToolbar);

        position.x += position.width + FIELD_SIZE_SPACE;

        EditorGUI.BeginDisabledGroup(!IsEnabled);
        OnDrawInList(position);
        EditorGUI.EndDisabledGroup();
    }

    public void DrawInToolbar()
    {
        if (IsEnabled)
            OnDrawInToolbar();
    }

    public virtual void Init() { }

    protected abstract void OnDrawInList(Rect position);
    protected abstract void OnDrawInToolbar();

    public int CompareTo(BaseToolbarElement other) { return SortingGroup.CompareTo(other.SortingGroup); }
}