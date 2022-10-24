using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public sealed class TabContainer : EntityContainer
    {
        private Dictionary<string, List<VisualEntity>> sections;

        // Stored required properties.
        private float totalHeight;
        private string currentSection;

        /// <summary>
        /// Tab group constructor.
        /// </summary>
        /// <param name="title">Foldout title content.</param>
        /// <param name="children">Foldout children elements.</param>
        public TabContainer(string name, Dictionary<string, List<VisualEntity>> sections)
            : base(name)
        {
            this.sections = sections;
            this.totalHeight = 21;
        }

        /// <summary>
        /// Add new element to section.
        /// </summary>
        /// <param name="section">Section key.</param>
        /// <param name="visualElement">Visual element reference.</param>
        public void AddElement(string section, VisualEntity visualElement)
        {
            if (!sections.ContainsKey(section))
            {
                sections[section] = new List<VisualEntity>();
            }

            sections[section].Add(visualElement);
        }

        /// <summary>
        /// Remove element from section.
        /// </summary>
        /// <param name="section">Section key.</param>
        /// <param name="visualElement">Visual element reference.</param>
        public void RemoveElement(string section, VisualEntity visualElement)
        {
            if (sections.ContainsKey(section))
            {
                sections[section].Remove(visualElement);
            }
        }

        /// <summary>
        /// Remove element from section by index.
        /// </summary>
        /// <param name="section">Section key.</param>
        /// <param name="visualElement">Visual element reference.</param>
        public void RemoveElement(string section, int index)
        {
            if (sections.ContainsKey(section))
            {
                sections[section].RemoveAt(index);
            }
        }

        #region [Visual Element Implementation]

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            if (string.IsNullOrEmpty(currentSection))
            {
                currentSection = sections.First().Key;
            }

            Event current = Event.current;

            using (new ContainerScope(ref position))
            {
                position.height = Mathf.Max(0, position.height - totalHeight);
                Rect headerPosition = EditorGUI.IndentedRect(position);
                headerPosition.height = totalHeight;
                GUI.Box(headerPosition, GUIContent.none, Uniform.Header);

                Rect contentPosition = new Rect(headerPosition.x, headerPosition.yMax - 1, headerPosition.width, position.height);
                GUI.Box(contentPosition, GUIContent.none, Uniform.ContentBackground);

                Rect[] buttonsPosition = HorizontalContainer.SplitRectangle(headerPosition, sections.Count);

                int index = 0;
                foreach (KeyValuePair<string, List<VisualEntity>> section in sections)
                {
                    Rect buttonPosition = buttonsPosition[index];
                    if (index < sections.Count - 1)
                    {
                        buttonPosition.width += 1;
                    }

                    if (current.type == EventType.Repaint)
                    {
                        const float HEADER_HEIGHT = 21;
                        float height = Uniform.TabButton.CalcHeight(new GUIContent(section.Key), buttonPosition.width);
                        totalHeight = Mathf.Max(height, HEADER_HEIGHT);

                        Uniform.TabButton.Draw(buttonPosition,
                            section.Key,
                            true,
                            currentSection == section.Key,
                            true,
                            true);
                    }

                    if (current.type == EventType.DragUpdated && buttonPosition.Contains(current.mousePosition))
                    {
                        currentSection = section.Key;
                    }

                    if (GUI.Button(buttonPosition, GUIContent.none, GUIStyle.none))
                    {
                        currentSection = section.Key;
                    }

                    index++;
                }


                if (position.height > 0)
                {
                    Rect childrenPosition = new Rect(position.x, headerPosition.yMax + 3, position.width - 2, position.height);
                    List<VisualEntity> children = sections[currentSection];
                    EditorGUI.indentLevel++;
                    DrawEntities(childrenPosition, in children);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Tab group elements height.
        /// </summary>
        public override float GetHeight()
        {
            if (string.IsNullOrEmpty(currentSection))
            {
                currentSection = sections.First().Key;
            }

            float height = totalHeight + 7;
            List<VisualEntity> children = sections[currentSection];
            for (int i = 0; i < children.Count; i++)
            {
                height += children[i].GetHeight();
                if (i < children.Count - 1)
                {
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        #endregion

        #region [IEnumerable Implementation]

        /// <summary>
        /// Children iterator.
        /// </summary>
        public override IEnumerator<VisualEntity> GetEnumerator()
        {
            foreach (KeyValuePair<string, List<VisualEntity>> section in sections)
            {
                for (int i = 0; i < section.Value.Count; i++)
                {
                    yield return section.Value[i];
                }
            }
        }

        #endregion

        #region [Getter / Setter]

        public Dictionary<string, List<VisualEntity>> GetSections() { return sections; }

        #endregion
    }
}