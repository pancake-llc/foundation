using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake
{
    public abstract class ScriptableConstant<T> : ScriptableObject
    {
        [Tooltip("The value of the const. This will can not be change during play mode")] [SerializeField, DisableInPlayMode]
        protected T value;

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