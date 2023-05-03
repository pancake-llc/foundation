using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class IndentScope : GUI.Scope
    {
        private readonly int lastLevel;

        public IndentScope(int level)
        {
            lastLevel = ApexGUI.IndentLevel;
            ApexGUI.IndentLevel = level;
        }

        public IndentScope()
            : this(1)
        {
        }

        protected override void CloseScope() { ApexGUI.IndentLevel = lastLevel; }
    }
}