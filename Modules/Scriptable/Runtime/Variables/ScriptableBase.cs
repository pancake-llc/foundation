using System;
using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [Serializable]
    [Searchable]
    public abstract class ScriptableBase : ScriptableObject
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;

        public virtual void Reset() { }
    }
}