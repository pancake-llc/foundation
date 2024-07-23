#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// A component that executes the <see cref="IInitializable.Init"/> method on its target during the OnAfterDeserialize event.
	/// <para>
	/// This can be used to initialize clients on inactive game objects, before the moment that they become active.
	/// </para>
	/// </summary>
	public sealed class InactiveInitializer : InactiveInitializerBaseInternal<MonoBehaviour>, IInitializable
	{
		[return: NotNull]
		private protected override bool InitTarget([DisallowNull] MonoBehaviour target, Context context)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!target) { throw new MissingReferenceException($"{GetType().Name} is missing '{nameof(target)}'."); }
			#endif

			var initializable = target as IInitializable;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(initializable is null) { throw new InvalidCastException($"{GetType().Name} '{nameof(target)}' does not implement {nameof(IInitializable)}."); }
			#endif

			return initializable.Init(context);
		}

		bool IInitializable.HasInitializer => false;
		bool IInitializable.Init(Context context) => InitTarget(target, context);

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
				_ = inactiveInitializer.OnAfterDeserializeOnMainThread();
			}
		}

		private protected override NullGuardResult EvaluateNullGuard() => !target || target is not IInitializable ? NullGuardResult.ClientNotSupported : NullGuardResult.Passed;
		private protected override void Reset() => Reset_Initializer<InactiveInitializer, MonoBehaviour>(this, gameObject);
		private protected override void OnValidate()
		{
			OnMainThread(()=>
			{
				if(!this)
				{
					return;
				}

				TryValidate_Initializer(this, gameObject);

				if(!target)
				{
					if(gameObject.IsAsset(true))
					{
						DestroyImmediate(this, true);
					}
					else
					{
						Debug.LogWarning($"{GetType().Name} is missing '{nameof(target)}'.");
						Destroy(this);
					}
				}
				else if(target is not IInitializable)
				{
					Debug.LogWarning($"{GetType().Name} {nameof(target)} {target.GetType().Name} can not be initialized, because it does not implement {nameof(IInitializable)}.", target);
					if(Application.isPlaying)
					{
						target = null;
					}
				}
				else if(target.gameObject != gameObject)
				{
					if(Application.isPlaying)
					{
						Debug.LogWarning($"{GetType().Name} '{nameof(target)}' {target.GetType().Name} is not attached to the same GameObject. This is not supported.", target);
						target = null;
					}
					else
					{
						DestroyImmediate(this, true);
					}
				}
			});
		}
		#endif
	}
}