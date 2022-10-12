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
        }

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
                const float HEADER_HEIGHT = 21;
                position.height = Mathf.Max(0, position.height - HEADER_HEIGHT);
                Rect headerPosition = EditorGUI.IndentedRect(position);
                headerPosition.height = HEADER_HEIGHT;
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

            float height = 28;
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

        public void AddElement(string section, VisualEntity visualElement)
        {
            if (!sections.ContainsKey(section))
            {
                sections[section] = new List<VisualEntity>();
            }

            sections[section].Add(visualElement);
        }

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

        #region [Getter / Setter]

        public Dictionary<string, List<VisualEntity>> GetSections() { return sections; }

        #endregion
    }
}