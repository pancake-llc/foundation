using System.Collections.Generic;
using UnityEngine;

namespace Pancake.BakingSheet.Unity
{
    public sealed class SheetContainerScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SheetScriptableObject> sheets;

        public IEnumerable<SheetScriptableObject> Sheets => sheets;

        private void Reset() { sheets = new List<SheetScriptableObject>(); }

        internal void Clear() { sheets.Clear(); }

        internal void Add(SheetScriptableObject sheet) { sheets.Add(sheet); }
    }
}