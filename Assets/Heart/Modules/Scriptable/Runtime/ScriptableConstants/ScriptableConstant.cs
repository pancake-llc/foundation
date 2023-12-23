using System;
using System.Text;
using UnityEngine;

namespace Pancake.Scriptable
{
    [Serializable]
    [EditorIcon("scriptable_const")]
    public class ScriptableConstant<T> : ScriptableVariableBase
    {
        [Tooltip("The value of the const. This will can not be change during play mode")] [SerializeField]
        protected T value;

        public override Type GetGenericType => typeof(T);

        public T Value => value;

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(name);
            sb.Append(" : ");
            sb.Append(value);
            return sb.ToString();
        }

        public static implicit operator T(ScriptableConstant<T> variable) => variable.Value;
    }
}