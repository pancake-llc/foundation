using System;
using UnityEngine;

[Serializable]
internal class ToolbarSides : BaseToolbarElement
{
    public override string NameInList => "[Left-right splitter]";

    public override void Init() { }

    protected override void OnDrawInList(Rect position) { }

    protected override void OnDrawInToolbar() { }
}