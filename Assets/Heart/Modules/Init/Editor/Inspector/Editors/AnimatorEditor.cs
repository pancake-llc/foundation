using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	[CustomEditorDecorator(typeof(Animator)), CanEditMultipleObjects]
    public sealed class AnimatorEditor : MultiInitializableEditor<Animator, StateMachineBehaviour>
	{
		protected override void GetInitializablesFromTarget([DisallowNull] Animator target, List<StateMachineBehaviour> addToList)
		{
			foreach(var behaviour in target.GetBehaviours<StateMachineBehaviour>())
			{
				if(InitializerEditorUtility.IsInitializable(behaviour.GetType()))
				{
					addToList.Add(behaviour);
				}
			}
		}
	}
}