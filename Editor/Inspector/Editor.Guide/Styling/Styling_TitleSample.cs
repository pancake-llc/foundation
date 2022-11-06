namespace Pancake.Editor.Guide
{
    using System;
    using UnityEngine;

    public class Styling_TitleSample : ScriptableObject
    {
        [Title("My Title")] public string val;

        [Title("$" + nameof(_myTitleField))] public Rect rect;

        [Title("$" + nameof(MyTitleProperty))] public Vector3 vec;

        [Title("Button Title")]
        [Button]
        public void MyButton() { }

#pragma warning disable CS0414
        private string _myTitleField = "Serialized Title";
#pragma warning restore CS0414

        private string MyTitleProperty => DateTime.Now.ToLongTimeString();
    }
}