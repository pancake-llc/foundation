using System.Reflection;
using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [InitializeOnLoad]
    static class ExperimentalChecker
    {
        /// <summary>
        /// Static constructor of ExperimentalChecker initializer.
        /// </summary>
        static ExperimentalChecker()
        {
            ObjectFactory.componentWasAdded += OnAddComponent;
        }

        /// <summary>
        /// Called every time a component is added to an object. 
        /// </summary>
        /// <param name="component">Added component reference.</param>
        private static void OnAddComponent(Component component)
        {
            if (component != null && component is MonoBehaviour && component.GetType().GetCustomAttribute<ExperimentalAttribute>() != null)
            {
                const string message = "You are adding a component marked with the [Experimental] attribute.\n\nKeep in mind that this component is under development or testing, may be changed or updated in the next updates.";
                EditorUtility.DisplayDialog("Warning", message, "Continue");
            }
        }
    }
}