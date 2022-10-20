using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Window.Searchable
{
    public class SearchItem
    {
        private GUIContent label;
        private bool isActive;
        private bool hidden;

        public SearchItem(GUIContent label)
        {
            this.label = label;
            isActive = true;
            hidden = false;
        }

        public SearchItem(GUIContent label, bool isActive)
            : this(label)
        {
            this.isActive = isActive;
            hidden = false;
        }

        public SearchItem(GUIContent label, bool isActive, bool hidden)
            : this(label, isActive)
        {
            this.hidden = hidden;
        }

        public virtual void OnGUI(Rect position)
        {
            EditorGUI.BeginDisabledGroup(!isActive);
            if (GUI.Button(position, label, Uniform.ButtonItem))
            {
                OnClickCallback?.Invoke();
            }

            EditorGUI.EndDisabledGroup();
        }

        public virtual float GetHeight() { return 20; }

        #region [Event Callback Function]

        public event Action OnClickCallback;

        #endregion

        #region [Getter / Setter]

        public GUIContent GetLabel() { return label; }

        public void SetLabel(GUIContent value) { label = value; }

        public bool IsActive() { return isActive; }

        public void IsActive(bool value) { isActive = value; }

        public bool Hidden() { return hidden; }

        public void Hidden(bool value) { hidden = value; }

        #endregion
    }
}