#pragma warning disable 0414

using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Pancake.Debugging
{
	[AddComponentMenu("")] // hide in Add Component menu to avoid clutter
	#if UNITY_2018_2_OR_NEWER
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
	public class DebugLogExtensionsDemoComponent : MonoBehaviour
	{
		[SerializeField]
		private bool isVisible = true;

		[SerializeField, Range(0f, 360f)]
		public float rotationSpeed = 100f;

		[SerializeField, Range(0f, 360f)]
		private float rotation;

		[SerializeField, HideInInspector]
		private MeshRenderer meshRenderer;

		#if UNITY_EDITOR
		private double lastUpdateTime = -100d;

		public bool IsVisible
		{
			get
			{
				return meshRenderer.enabled;
			}
			
			set
			{
				isVisible = value;
				meshRenderer.enabled = value;
			}
		}

		public float Rotation
		{
			get
			{
				return transform.localEulerAngles.y;
			}


			set
			{
				rotation = value;
				transform.localEulerAngles = new Vector3(0f, value, 0f);
			}
		}

		[UsedImplicitly]
		private void Reset()
		{
			meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.hideFlags = HideFlags.HideInInspector;
			transform.hideFlags = HideFlags.HideInInspector;
			GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
			UnityEditor.EditorApplication.delayCall += OnValuesChanged;
		}

		public void OnValuesChanged()
		{
			meshRenderer.enabled = isVisible;
			Rotation = rotation;
		}

		public void UpdateRotation()
		{
			double timeNow = UnityEditor.EditorApplication.timeSinceStartup;
			double deltaTime = timeNow - lastUpdateTime;
			if(deltaTime > 100d)
			{
				lastUpdateTime = timeNow;
				return;
			}
			else if(deltaTime < 0.001f)
			{
				return;
			}

			lastUpdateTime = timeNow;

			UnityEditor.Undo.RecordObject(transform, "Update Rotation");

			float setRotation = (float)(Rotation + deltaTime * rotationSpeed);
			if(setRotation >= 360f)
			{
				setRotation -= 360f;
			}
			else if(setRotation < 0f)
			{
				setRotation += 360f;
			}
			Rotation = setRotation;

			transform.hasChanged = true;
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			Debug.CancelLogChanges(() => rotationSpeed);
			Debug.CancelLogChanges(() => IsVisible);
			Debug.CancelLogChanges(() => Rotation);

			Debug.CancelDisplayOnScreen(() => rotationSpeed);
			Debug.CancelDisplayOnScreen(() => IsVisible);
			Debug.CancelDisplayOnScreen(() => Rotation);
		}

		public void LogIsVisible()
		{
			Debug.Log(()=>IsVisible);
		}

		public void LogRotation()
		{
			Debug.Log(() => Rotation);
		}

		public void LogRotationSpeed()
		{
			Debug.Log(() => rotationSpeed);
		}

		public void LogPublicMembers()
		{
			Debug.LogState(this);
		}

		public void LogPrivateMembers()
		{
			Debug.LogState(this, BindingFlags.Instance | BindingFlags.NonPublic);
		}
		#endif
	}
}