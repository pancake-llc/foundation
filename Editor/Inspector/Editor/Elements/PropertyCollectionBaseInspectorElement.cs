using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;


namespace Pancake.Editor
{
    public abstract class PropertyCollectionBaseInspectorElement : InspectorElement
    {
        private List<DeclareGroupBaseAttribute> _declarations = new List<DeclareGroupBaseAttribute>();

        private Dictionary<string, PropertyCollectionBaseInspectorElement> _groups;

        [PublicAPI]
        public void DeclareGroups([CanBeNull] Type type)
        {
            if (type == null)
            {
                return;
            }

            foreach (var attribute in ReflectionUtilities.GetAttributesCached(type))
            {
                if (attribute is DeclareGroupBaseAttribute declareAttribute)
                {
                    _declarations.Add(declareAttribute);
                }
            }
        }

        [PublicAPI]
        public void AddProperty(Property property) { AddProperty(property, default, out _); }

        [PublicAPI]
        public void AddProperty(Property property, PropertyInspectorElement.Props props, out string group)
        {
            var propertyElement = new PropertyInspectorElement(property, props);

            if (property.TryGetAttribute(out GroupAttribute groupAttribute))
            {
                IEnumerable<string> path = groupAttribute.Path.Split('/');

                var remaining = path.GetEnumerator();
                if (remaining.MoveNext())
                {
                    group = remaining.Current;
                    AddGroupedChild(propertyElement,
                        property,
                        remaining.Current,
                        remaining.Current,
                        remaining);
                }
                else
                {
                    group = null;
                    AddPropertyChild(propertyElement, property);
                }
            }
            else
            {
                group = null;
                AddPropertyChild(propertyElement, property);
            }
        }

        private void AddGroupedChild(InspectorElement child, Property property, string currentPath, string currentName, IEnumerator<string> remainingPath)
        {
            if (_groups == null)
            {
                _groups = new Dictionary<string, PropertyCollectionBaseInspectorElement>();
            }

            var groupElement = CreateSubGroup(property, currentPath, currentName);

            if (remainingPath.MoveNext())
            {
                var nextPath = currentPath + "/" + remainingPath.Current;
                var nextName = remainingPath.Current;

                groupElement.AddGroupedChild(child,
                    property,
                    nextPath,
                    nextName,
                    remainingPath);
            }
            else
            {
                groupElement.AddPropertyChild(child, property);
            }
        }

        private PropertyCollectionBaseInspectorElement CreateSubGroup(Property property, string groupPath, string groupName)
        {
            if (!_groups.TryGetValue(groupName, out var groupElement))
            {
                var declaration = _declarations.FirstOrDefault(it => it.Path == groupPath);

                if (declaration != null)
                {
                    groupElement = DrawersUtilities.TryCreateGroupElementFor(declaration);
                }

                if (groupElement == null)
                {
                    groupElement = new DefaultGroupInspectorElement();
                }

                groupElement._declarations = _declarations;

                _groups.Add(groupName, groupElement);

                AddPropertyChild(groupElement, property);
            }

            return groupElement;
        }

        protected virtual void AddPropertyChild(InspectorElement inspectorElement, Property property) { AddChild(inspectorElement); }

        private class DefaultGroupInspectorElement : PropertyCollectionBaseInspectorElement
        {
        }
    }
}