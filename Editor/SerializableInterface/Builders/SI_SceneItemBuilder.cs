using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Pancake.Editor
{
    internal sealed class SI_SceneItemBuilder
    {
        private readonly Type _interfaceType;
        private readonly Scene? _scene;

        public SI_SceneItemBuilder(Type interfaceType, Scene? scene)
        {
            Assert.IsNotNull(interfaceType);

            _interfaceType = interfaceType;
            _scene = scene;
        }

        public AdvancedDropdownItem Build()
        {
            if (_scene == null || !_scene.Value.IsValid())
            {
                return new AdvancedDropdownItem("Scene") {enabled = false};
            }

            AdvancedDropdownItem root = new SI_AdvancedDropdownItemWrapper("Scene");

            GameObject[] rootGameObjects = _scene.Value.GetRootGameObjects();

            foreach (GameObject rootGameObject in rootGameObjects)
            {
                CreateItemsRecursive(rootGameObject.transform, root);
            }

            return root;
        }

        private void CreateItemsRecursive(Transform transform, AdvancedDropdownItem parent)
        {
            AdvancedDropdownItem advancedDropdownItem = new AdvancedDropdownItem(transform.name) {icon = SI_IconUtility.GameObjectIcon};

            Component[] components = transform.GetComponents(_interfaceType);

            foreach (Component component in components)
            {
                advancedDropdownItem.AddChild(new SI_SceneDropdownItem(component));
            }

            foreach (Transform child in transform)
            {
                CreateItemsRecursive(child, advancedDropdownItem);
            }

            if (advancedDropdownItem.children.Any())
                parent.AddChild(advancedDropdownItem);
        }
    }
}