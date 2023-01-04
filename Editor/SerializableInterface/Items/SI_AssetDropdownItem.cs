using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.Editor
{
    internal sealed class SI_AssetDropdownItem : AdvancedDropdownItem, SI_IDropdownItem
    {
        private readonly string _path;

        /// <inheritdoc />
        public SI_AssetDropdownItem(string path)
            : base(Path.GetFileNameWithoutExtension(path))
        {
            _path = path;
            icon = SI_IconUtility.GetIconForObject(path);
        }

        /// <inheritdoc />
        InterfaceRefMode SI_IDropdownItem.Mode => InterfaceRefMode.Unity;

        /// <inheritdoc />
        object SI_IDropdownItem.GetValue() { return AssetDatabase.LoadAssetAtPath<Object>(_path); }
    }
}