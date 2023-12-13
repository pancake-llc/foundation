using Pancake.Greenery;
using UnityEditor;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryProceduralItem), true)]
    public class GreeneryProceduralItemEditor : UnityEditor.Editor
    {
        private MaterialEditor materialEditor;

        private void OnEnable()
        {
            GreeneryProceduralItem item = target as GreeneryProceduralItem;
            if (item.renderingMaterial != null)
            {
                if (materialEditor == null)
                {
                    materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
                }
                else
                {
                    DestroyImmediate(materialEditor);
                    materialEditor = (MaterialEditor) CreateEditor(item.renderingMaterial);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (materialEditor != null)
            {
                materialEditor.DrawHeader();
                materialEditor.OnInspectorGUI();
            }
        }


        private void OnDisable() { DestroyImmediate(materialEditor); }
    }
}