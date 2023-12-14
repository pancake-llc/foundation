using Pancake.Greenery;
using UnityEditor;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryInstance))]
    public class GreeneryInstanceEditor : UnityEditor.Editor
    {
        private MaterialEditor _materialEditor;

        private void OnEnable()
        {
            var item = target as GreeneryInstance;
            if (item != null && item.instanceMaterial != null)
            {
                if (_materialEditor == null) _materialEditor = (MaterialEditor) CreateEditor(item.instanceMaterial);
                else
                {
                    DestroyImmediate(_materialEditor);
                    _materialEditor = (MaterialEditor) CreateEditor(item.instanceMaterial);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_materialEditor != null)
            {
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
            }
        }


        private void OnDisable() { DestroyImmediate(_materialEditor); }
    }
}