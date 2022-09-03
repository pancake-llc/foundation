using UnityEngine;

namespace Pancake
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public string Name { get; private set; }
        public bool FoldEverything { get; private set; }
        public bool ReadOnly { get; private set; }
        public bool Styled { get; private set; }

        /// <summary>Adds the property to the specified foldout group.</summary>
        /// <param name="name">Name of the foldout group.</param>
        /// <param name="foldEverything">Toggle to put all properties to the specified group</param>
        /// <param name="readOnly">Toggle to put all properties to the specified group</param>
        public FoldoutAttribute(string name, bool foldEverything = true, bool readOnly = false, bool styled = false)
        {
            FoldEverything = foldEverything;
            Name = name;
            ReadOnly = readOnly;
            Styled = styled;
        }
    }
}