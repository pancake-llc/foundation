#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
	/// <summary>
	/// Class responsible for exposing the state of all active <see cref="ServiceAttribute">services</see>
	/// so that their state can be examined using the Inspector window.
	/// </summary>
	[AddComponentMenu(Hidden)]
	internal sealed class ServicesDebugger : MonoBehaviour<object[]>
	{
		private const string Hidden = "";

		private object[] services = new object[0];

		protected override void Init(object[] services) => this.services = services;

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

        internal void SetServices(object[] value)
        {
			int count = value.Length;
			services = value;
            for(int i = count - 1; i >= 0; i--)
            {
				var obj = value[i];
				if(obj is Object unityObject)
                {
					services[i] = new LabeledUnityObjectService(unityObject);
				}
				else
                {
					services[i] = new LabeledPlainOldClassObjectService(obj);
				}
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