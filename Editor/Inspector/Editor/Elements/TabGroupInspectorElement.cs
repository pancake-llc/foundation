using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Editor
{
    public class TabGroupInspectorElement : HeaderGroupBaseInspectorElement
    {
        private const string DefaultTabName = "Main";

        private readonly List<string> _tabNames;
        private readonly Dictionary<string, InspectorElement> _tabElements;

        private string _activeTabName;

        public TabGroupInspectorElement()
        {
            _tabNames = new List<string>();
            _tabElements = new Dictionary<string, InspectorElement>();
            _activeTabName = null;
        }

        protected override void DrawHeader(Rect position)
        {
            if (_tabNames.Count == 0)
            {
                return;
            }

            var tabRect = new Rect(position) {width = position.width / _tabNames.Count,};

            if (_tabNames.Count == 1)
            {
                GUI.Toggle(tabRect, true, _tabNames[0], Uniform.TabOnlyOne);
            }
            else
            {
                for (int index = 0, tabCount = _tabNames.Count; index < tabCount; index++)
                {
                    var tabName = _tabNames[index];
                    var tabStyle = index == 0 ? Uniform.TabFirst : index == tabCount - 1 ? Uniform.TabLast : Uniform.TabMiddle;

                    var isTabActive = GUI.Toggle(tabRect, _activeTabName == tabName, tabName, tabStyle);
                    if (isTabActive && _activeTabName != tabName)
                    {
                        SetActiveTab(tabName);
                    }

                    tabRect.x += tabRect.width;
                }
            }
        }

        protected override void AddPropertyChild(InspectorElement inspectorElement, Property property)
        {
            var tabName = DefaultTabName;

            if (property.TryGetAttribute(out TabAttribute tab))
            {
                tabName = tab.TabName ?? tabName;
            }

            if (!_tabElements.TryGetValue(tabName, out var tabElement))
            {
                tabElement = new InspectorElement();

                _tabElements[tabName] = tabElement;
                _tabNames.Add(tabName);

                if (_activeTabName == null)
                {
                    SetActiveTab(tabName);
                }
            }

            tabElement.AddChild(inspectorElement);
        }

        private void SetActiveTab(string tabName)
        {
            _activeTabName = tabName;

            RemoveAllChildren();

            AddChild(_tabElements[_activeTabName]);
        }
    }
}