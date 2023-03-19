using System;
using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [Serializable]
    [Searchable]
    public class ScriptableBase : ScriptableObject
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;
    }
}