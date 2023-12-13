using System;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public abstract class GreeneryEditorModule
    {
        public abstract void Initialize(GreeneryToolEditor toolEditor);
        public abstract void Release();
        public abstract void OnGUI();
        public abstract float GetHeight();
        public abstract void SaveSettings();
    }
}