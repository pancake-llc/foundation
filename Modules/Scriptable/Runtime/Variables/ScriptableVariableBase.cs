using System;
using UnityEngine;

namespace Pancake.Scriptable
{
    [Serializable]
    public class ScriptableVariableBase : ScriptableBase
    {
        [SerializeField, HideInInspector] private string id;

        public string Id { get => id; set => id = value; }
    }
}