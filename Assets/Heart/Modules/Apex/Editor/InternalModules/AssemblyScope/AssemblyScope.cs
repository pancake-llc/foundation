using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [Serializable]
    public struct AssemblyScope : IEquatable<AssemblyScope>
    {
        [SerializeField] [NotEmpty] private string name;

        [SerializeField] private string condition;

        public AssemblyScope(string name, string condition)
        {
            this.name = name;
            this.condition = condition;
        }

        #region[HashCode Override]

        public override int GetHashCode() { return name.GetHashCode(); }

        #endregion

        #region [IEquatable<T> Implementation]

        public bool Equals(AssemblyScope other) { return name == other.name; }

        #endregion

        #region [Getter / Setter]

        public string GetName() { return name; }

        public void SetName(string value) { name = value; }

        public string GetCondition() { return condition; }

        public void SetCondition(string value) { condition = value; }

        #endregion
    }
}