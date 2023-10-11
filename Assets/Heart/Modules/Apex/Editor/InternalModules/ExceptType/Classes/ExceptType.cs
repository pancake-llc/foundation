using System;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [Serializable]
    public struct ExceptType : IEquatable<ExceptType>
    {
        [SerializeField] private string name;

        [SerializeField] private bool subClasses;

        /// <summary>
        /// Except type constructor.
        /// </summary>
        /// <param name="name">Name of type which need to except.</param>
        public ExceptType(string name)
        {
            this.name = name;
            subClasses = false;
        }

        /// <summary>
        /// Except type constructor.
        /// </summary>
        /// <param name="name">Name of type which need to except.</param>
        /// <param name="subClasses">Do need to exclude all derived types?</param>
        public ExceptType(string name, bool subClasses)
            : this(name)
        {
            this.subClasses = subClasses;
        }

        #region[HashCode Override]

        public override int GetHashCode() { return name.GetHashCode(); }

        #endregion

        #region [IEquatable<T> Implementation]

        public bool Equals(ExceptType other) { return name == other.name; }

        #endregion

        #region [Getter / Setter]

        public string GetName() { return name; }

        public void SetName(string value) { name = value; }

        public bool SubClasses() { return subClasses; }

        public void SubClasses(bool value) { subClasses = value; }

        #endregion
    }
}