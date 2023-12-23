using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class TabContainer : Container, ITabContainer, IGUIChangedCallback
    {
        public struct Tab
        {
            public readonly string name;
            public readonly List<VisualEntity> entities;

            public Tab(string name, List<VisualEntity> entities)
            {
                this.name = name;
                this.entities = entities;
            }
        }

        private float headerHeight;
        private int tabIndex;
        private List<Tab> tabs;

        /// <summary>
        /// Tab group constructor.
        /// </summary>
        /// <param name="name">Tab container name.</param>
        /// <param name="tabs">Tab container tabs.</param>
        public TabContainer(string name, List<Tab> tabs)
            : base(name)
        {
            this.tabs = tabs;
            headerHeight = 22;
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling tab container.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            float x = position.x;
            float width = position.width;
            float contentHeight = position.height - headerHeight;

            position.width /= tabs.Count;
            position.height = headerHeight;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (GUI.Button(position, tabs[i].name, ApexStyles.BoxCenteredButton))
                {
                    tabIndex = i;
                    GUIChanged?.Invoke();
                }

                position.x = position.xMax;
            }

            if (contentHeight > 0)
            {
                position.x = x;
                position.y = position.yMax - 1;
                position.width = width;
                position.height = contentHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
                position.y += EditorGUIUtility.standardVerticalSpacing * ApexGUIUtility.BoxBounds;

                tabIndex = Mathf.Clamp(tabIndex, 0, tabs.Count);
                Tab tab = tabs[tabIndex];

                using (new BoxScope(ref position))
                {
                    DrawEntities(position, in tab.entities);
                }
            }
        }

        /// <summary>
        /// Total height required to drawing tab container.
        /// </summary>
        public override float GetHeight()
        {
            tabIndex = Mathf.Clamp(tabIndex, 0, tabs.Count);
            Tab tab = tabs[tabIndex];

            return headerHeight + (EditorGUIUtility.standardVerticalSpacing * 2) * ApexGUIUtility.BoxBounds + GetEntitiesHeight(in tab.entities);
        }

        #endregion

        #region [IGUIChangedCallback Implementation]

        /// <summary>
        /// Called when GUI has been changed.
        /// </summary>
        public event Action GUIChanged;

        #endregion

        #region [IEntityContainer Implementation]

        /// <summary>
        /// Loop through all entities of the entity container.
        /// </summary>
        public override IEnumerable<VisualEntity> Entities
        {
            get
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    Tab tab = tabs[i];
                    for (int j = 0; j < tab.entities.Count; j++)
                    {
                        yield return tab.entities[j];
                    }
                }
            }
        }

        #endregion

        #region [IEntityVisibility Implementation]

        /// <summary>
        /// Container visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                for (int j = 0; j < tab.entities.Count; j++)
                {
                    if (tab.entities[j].IsVisible())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region [ITabContainer Implementation]

        /// <summary>
        /// Add new entity to the tab container.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="entity">Visual entity reference.</param>
        public void AddEntity(string tabName, VisualEntity entity)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                if (tab.name == tabName)
                {
                    tab.entities.Add(entity);
                    return;
                }
            }

            tabs.Add(new Tab(tabName, new List<VisualEntity>() {entity}));
        }

        /// <summary>
        /// Remove entity from the tab container by reference.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="entity">Visual entity reference.</param>
        public void RemoveEntity(string tabName, VisualEntity entity)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                if (tab.name == tabName)
                {
                    tab.entities.Remove(entity);
                    return;
                }
            }
        }

        /// <summary>
        /// Remove entity from the tab container by index.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="index">Visual entity index in tab.</param>
        public void RemoveEntity(string tabName, int index)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                if (tab.name == tabName)
                {
                    tab.entities.RemoveAt(index);
                    return;
                }
            }
        }

        #endregion

        #region [Getter / Setter]

        public float GetHeaderHeight() { return headerHeight; }

        public void SetHeaderHeight(float value) { headerHeight = value; }

        public int GetTabIndex() { return tabIndex; }

        public void SetTabIndex(int value) { tabIndex = value; }

        public List<Tab> GetTabs() { return tabs; }

        public void SetTabs(List<Tab> value) { tabs = value; }

        #endregion
    }
}