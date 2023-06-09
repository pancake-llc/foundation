using Pancake.Apex;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = Pancake.Apex.FilePathAttribute;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(FilePathAttribute))]
    public sealed class FilePathView : ExplorerPathView
    {
        private FilePathAttribute attribute;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label) { attribute = viewAttribute as FilePathAttribute; }

        /// <summary>
        /// Select new path.
        /// </summary>
        protected override string GetSeletedPath() { return EditorUtility.OpenFilePanel(attribute.Title, attribute.Directory, attribute.Extension); }

        /// <summary>
        /// Called before applying selected path to the property.
        /// </summary>
        /// <param name="path">Selected path.</param>
        protected override string ValidatePath(string path)
        {
            if (attribute.RelativePath && path.Contains("Assets"))
            {
                return path.Remove(0, path.IndexOf("Assets"));
            }

            return base.ValidatePath(path);
        }
    }
}