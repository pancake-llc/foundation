#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init.Internal
{
	public sealed class InactiveInitializer : InactiveInitializerBaseInternal<MonoBehaviour>, IInitializable
	{
		[return: NotNull]
		private protected sealed override bool InitTarget([DisallowNull] ref MonoBehaviour target, Context context)
		{
			#if DEBUG
			if(!target) { throw new MissingReferenceException($"{GetType().Name} is missing '{nameof(target)}'."); }
			#endif

			var initializable = target as IInitializable;

			#if DEBUG
			if(initializable is null) { throw new InvalidCastException($"{GetType().Name} '{nameof(target)}' does not implement {nameof(IInitializable)}."); }
			#endif

			return initializable.Init(context);
		}

		bool IInitializable.HasInitializer => false;

		bool IInitializable.Init(Context context)
		{
			InitTarget(context);
			return true;
		}

		#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnEnterPlayMode()
		{
			// Because in the editor OnAfterDeserialize can already occur in edit mode, before the initial scene is loaded,
			// we need to manually call OnAfterDeserialize for instances in the initial scene.
			foreach(var inactiveInitializer in
				#if UNITY_2023_1_OR_NEWER
                FindObjectsByType<InactiveInitializer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                #else
                FindObjectsOfType<InactiveInitializer>(includeInactive:true))
                #endif
			{
				inactiveInitializer.OnAfterDeserialize();
			}
		}

		private protected override NullGuardResult EvaluateNullGuard() => target == null || target is not IInitializable ? NullGuardResult.ClientNotSupported : NullGuardResult.Passed;
		private protected sealed override void Reset() => Reset_Initializer<InactiveInitializer, MonoBehaviour>(this, gameObject);
		private protected override void OnValidate()
		{
			OnMainThread(()=>
			{
				if(this == null)
				{
					return;
				}

				TryValidate_Initializer(this, gameObject);

				if(target == null)
				{
					Debug.LogWarning($"{GetType().Name} is missing '{nameof(target)}'.");
					Destroy(this);
				}
				else if(target is not IInitializable)
				{
					Debug.LogWarning($"{GetType().Name} '{nameof(target)}' does not implement {nameof(IInitializable)}.");
					target = null;
				}
				else if(target.gameObject != gameObject)
				{
					Debug.LogWarning($"{GetType().Name} '{nameof(target)}' is not attached to the same GameObject.");
					target = null;
				}
			});
		}
		#endif
	}
}