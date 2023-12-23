using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public abstract class GreeneryScattererModule
    {
        public abstract void Release();
        public abstract GUIContent GetIcon();
        public abstract void OnGUI();
        public abstract float GetHeight();

        protected abstract void SaveSettings();


        [SerializeField] protected GreeneryScatteringModule scatteringModule;

        public virtual void ToolHandles(Rect guiRect)
        {
            //Steal control
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
        }

        public virtual void Initialize(GreeneryScatteringModule scatteringModule) { this.scatteringModule = scatteringModule; }

        public virtual void UpdateScatteringSettings(GreeneryScatteringModule scatteringModule) { this.scatteringModule = scatteringModule; }
    }
}