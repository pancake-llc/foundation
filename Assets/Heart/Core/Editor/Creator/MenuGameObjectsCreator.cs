using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PancakeEditor
{
    public static class MenuGameObjectsCreator
    {
        private class StringTree<T>
        {
            public Dictionary<string, StringTree<T>> SubTrees { get; } = new();
            public T Value { get; private set; }

            public void Insert(string path, T value, int idx = 0) { InternalInsert(path.Split('/'), idx, value); }

            private void InternalInsert(string[] path, int idx, T value)
            {
                if (idx >= path.Length)
                {
                    Value = value;
                    return;
                }

                if (!SubTrees.ContainsKey(path[idx])) SubTrees.Add(path[idx], new StringTree<T>());
                SubTrees[path[idx]].InternalInsert(path, idx + 1, value);
            }

            public IEnumerable<KeyValuePair<string, StringTree<T>>> GetSortedSubTrees() { return SubTrees.OrderBy(pair => pair.Key); }
        }

        private class SearchTypeDropdown : AdvancedDropdown
        {
            private readonly StringTree<Type> _list;
            private readonly Action<Type> _func;

            private readonly List<Type> _lookup = new();

            public Vector2 MinimumSize { get => minimumSize; set => minimumSize = value; }

            public SearchTypeDropdown(AdvancedDropdownState state, StringTree<Type> list, Action<Type> func)
                : base(state)
            {
                _list = list;
                _func = func;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                _func?.Invoke(_lookup[item.id]);
            }

            private void Render(StringTree<Type> tree, string key, AdvancedDropdownItem parentGroup)
            {
                if (tree.Value != null)
                {
                    _lookup.Add(tree.Value);
                    parentGroup.AddChild(new AdvancedDropdownItem(key) {id = _lookup.Count - 1});
                }
                else
                {
                    var self = new AdvancedDropdownItem(key);
                    foreach (var subtree in tree.GetSortedSubTrees())
                    {
                        Render(subtree.Value, subtree.Key, self);
                    }

                    parentGroup.AddChild(self);
                }
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Scriptable Search");

                foreach (var subtree in _list.GetSortedSubTrees())
                {
                    Render(subtree.Value, subtree.Key, root);
                }

                return root;
            }
        }

        [MenuItem(itemName: "Assets/Create/Pancake/Scriptable Search #1", isValidateFunction: false, priority: -1)]
        private static void ScriptableSearchMenu()
        {
            var typeTree = new StringTree<Type>();
            foreach (var type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>().Where(t => t.GetCustomAttribute<SearchableAttribute>(true) != null))
            {
                string name = type.GetCustomAttribute<CreateAssetMenuAttribute>().menuName;
                typeTree.Insert(name, type, 1);
            }

            var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var projectBrowser = EditorWindow.GetWindow(projectBrowserType);

            var xy = new Vector2(projectBrowser.position.x + 10, projectBrowser.position.y + 60);

            var dropdown =
                new SearchTypeDropdown(new AdvancedDropdownState(),
                    typeTree,
                    s => EditorApplication.ExecuteMenuItem("Assets/Create/" + s.GetCustomAttribute<CreateAssetMenuAttribute>().menuName))
                {
                    MinimumSize = new Vector2(projectBrowser.position.width - 20, projectBrowser.position.height - 80)
                };

            var rect = new Rect(xy.x, xy.y, projectBrowser.position.width - 20, projectBrowser.position.height);

            dropdown.Show(new Rect()); // don't use this to position the
            var window = typeof(SearchTypeDropdown).GetField("m_WindowInstance", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(dropdown) as EditorWindow;
            if (window != null) window.position = rect;
        }

        [MenuItem("Assets/Export Sub-Sprites")]
        public static void DoExportSubSprites()
        {
            string folder = EditorUtility.OpenFolderPanel("Export subsprites into what folder?", "", "");
            foreach (var obj in Selection.objects)
            {
                var sprite = obj as Sprite;
                if (sprite == null) continue;
                var extracted = ExtractAndName(sprite);
                SaveSubSprite(extracted, folder);
            }
        }

        [MenuItem("Assets/Export Sub-Sprites", true)]
        private static bool CanExportSubSprites() { return Selection.activeObject is Sprite; }

        // Since a sprite may exist anywhere on a texture2D, this will crop out the sprite's claimed region and return a new, cropped, texture2D.
        private static Texture2D ExtractAndName(Sprite sprite)
        {
            var output = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            var r = sprite.textureRect;
            var pixels = sprite.texture.GetPixels((int) r.x, (int) r.y, (int) r.width, (int) r.height);
            output.SetPixels(pixels);
            output.Apply();
            output.name = sprite.texture.name + " " + sprite.name;
            return output;
        }

        private static void SaveSubSprite(Texture2D tex, string saveToDirectory)
        {
            if (!System.IO.Directory.Exists(saveToDirectory)) System.IO.Directory.CreateDirectory(saveToDirectory);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(saveToDirectory, tex.name + ".png"), tex.EncodeToPNG());
        }
    }
}