#if PANCAKE_AI
using Pancake.AI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.AI
{
    [CustomEditor(typeof(AIBrain))]
    public class AIBrainEditor : Editor
    {
        private void OnEnable() { RequiresConstantRepaint(); }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                var brain = (AIBrain) target;

                var chosenAction = GetChosenAction(brain);
                if (chosenAction != null)
                {
                    EditorGUILayout.LabelField($"Current Chosen Action: {chosenAction.name}", EditorStyles.boldLabel);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Actions/Considerations", EditorStyles.boldLabel);


                foreach (var action in brain.actions)
                {
                    float priority = action.CalculatePriority(brain.context);
                    EditorGUILayout.LabelField($"Action: {action.name}, Priority: {priority:F2}");

                    // Draw the single consideration for the action
                    DrawConsideration(action.consideration, brain.context, 1);
                }
            }
        }

        private void DrawConsideration(Consideration consideration, AIContext context, int indentLevel)
        {
            EditorGUI.indentLevel = indentLevel;

            if (consideration is CompositeConsideration compositeConsideration)
            {
                EditorGUILayout.LabelField($"Composite Consideration: {compositeConsideration.name}, Operation: {compositeConsideration.operation}");

                foreach (var subConsideration in compositeConsideration.considerations)
                {
                    DrawConsideration(subConsideration, context, indentLevel + 1);
                }
            }
            else
            {
                float value = consideration.Evaluate(context);
                EditorGUILayout.LabelField($"Consideration: {consideration.name}, Value: {value:F2}");
            }

            EditorGUI.indentLevel = indentLevel - 1; // Reset indentation after drawing
        }

        private AIAction GetChosenAction(AIBrain brain)
        {
            var highestPriority = float.MinValue;
            AIAction chosenAction = null;

            foreach (var action in brain.actions)
            {
                float priority = action.CalculatePriority(brain.context);
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    chosenAction = action;
                }
            }

            return chosenAction;
        }
    }
}
#endif