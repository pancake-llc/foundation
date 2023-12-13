using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [CustomEditor(typeof(GreeneryLODInstance))]
    public class GreeneryLODInstanceEditor : UnityEditor.Editor
    {
        private MaterialEditor materialEditor;

        private void OnEnable()
        {
            GreeneryLODInstance item = target as GreeneryLODInstance;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GreeneryLODInstance item = target as GreeneryLODInstance;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            int previewIndex = EditorGUILayout.IntSlider("LOD Preview", item.previewIndex, 0, item.instanceLODs.Count - 1);


            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < item.instanceLODs.Count; i++)
            {
                var instanceLOD = item.instanceLODs[i];
                if (i == 0)
                {
                    if (GUILayout.Button(i.ToString(), GUILayout.Width((Screen.width - 50) * instanceLOD.LODFactor), GUILayout.Height(30)))
                    {
                        previewIndex = i;
                    }
                }
                else
                {
                    if (GUILayout.Button(i.ToString(),
                            GUILayout.Width((Screen.width - 50) * (instanceLOD.LODFactor - item.instanceLODs[i - 1].LODFactor)),
                            GUILayout.Height(30)))
                    {
                        previewIndex = i;
                    }
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(item, "Changed preview index");
                item.previewIndex = previewIndex;

                if (materialEditor != null)
                {
                    DestroyImmediate(materialEditor);
                }

                if (item.instanceLODs[previewIndex].instanceMaterial != null)
                {
                    materialEditor = (MaterialEditor) CreateEditor(item.instanceLODs[previewIndex].instanceMaterial);
                }
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();


            if (materialEditor != null)
            {
                materialEditor.DrawHeader();
                materialEditor.OnInspectorGUI();
            }
        }

        private void OnDisable() { DestroyImmediate(materialEditor); }
    }
}