using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class ExceptTypeScope : GUI.Scope
    {
        private ExceptType[] types;

        public ExceptTypeScope(in ExceptType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                ApexUtility.ExceptTypes.Add(types[i]);
            }
        }

        protected override void CloseScope()
        {
            for (int i = 0; i < types.Length; i++)
            {
                ApexUtility.ExceptTypes.Remove(types[i]);
            }
        }
    }
}