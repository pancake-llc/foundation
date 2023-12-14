using Pancake.Greenery;
using UnityEditor;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryProceduralItem), true)]
    public class GreeneryProceduralItemEditor : UnityEditor.Editor
    {
        private MaterialEditor _materialEditor;

        private void OnEnable()
        {
            var item = target as GreeneryProceduralItem;
            if (item != null && item.renderingMaterial != null)
            {
                if (_materialEditor == null) _materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
                else
                {
                    DestroyImmediate(_materialEditor);
                    _materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
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