#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Class responsible for exposing the state of all active <see cref="ServiceAttribute">services</see>
	/// so that their state can be examined using the Inspector window.
	/// </summary>
	[AddComponentMenu(Hidden)]
	internal sealed class ServicesDebugger : MonoBehaviour
	{
		private const string Hidden = "";

		private List<object> services = new();

		private void Awake() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		private void OnDestroy() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

		private void OnPlayModeStateChanged(PlayModeStateChange playModeState)
		{
			if(playModeState == PlayModeStateChange.ExitingPlayMode)
			{
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				Destroy(this);
			}
		}

		#pragma warning disable CS1998
		internal async Task SetServices(IEnumerable<object> services)
		#pragma warning restore CS1998
		{
			foreach(var service in services)
			{
				if(service is Task task)
				{
					#pragma warning disable CS4014
					AddServiceAsync(task);
					#pragma warning restore CS4014
				}
				else
				{
					AddService(service);
				}
			}
		}

		private async Task AddServiceAsync(Task task)
		{
			await task.ContinueWith(AddResult);

			void AddResult(Task task)
			{
				AddService(task.GetResult());
			}
		}

		private void AddService(object service)
		{
			if(service is Object unityObject)
			{
				services.Add(new LabeledUnityObjectService(unityObject));
			}
			else
			{
				services.Add(new LabeledPlainOldClassObjectService(service));
			}
		}

		[Serializable]
		private sealed class LabeledPlainOldClassObjectService
		{
			[SerializeField]
			private string label;

			[SerializeReference]
			private object state;

			public LabeledPlainOldClassObjectService(object service)
			{
				label = service.GetType().Name;
				state = service;				
			}
		}

		[Serializable]
		private sealed class LabeledUnityObjectService
		{
			[SerializeField]
			private string label;

			[SerializeField]
			private Object state;

			public LabeledUnityObjectService(Object service)
			{
				label = service.GetType().Name;
				state = service;				
			}
		}
	}
}

#endif