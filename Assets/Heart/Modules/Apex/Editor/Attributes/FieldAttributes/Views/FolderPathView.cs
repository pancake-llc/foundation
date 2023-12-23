using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(FolderPathAttribute))]
    public sealed class FolderPathView : ExplorerPathView
    {
        private FolderPathAttribute attribute;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label) { attribute = viewAttribute as FolderPathAttribute; }

        /// <summary>
        /// Select new path.
        /// </summary>
        protected override string GetSeletedPath() { return EditorUtility.OpenFolderPanel(attribute.Title, attribute.Folder, attribute.DefaultName); }

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