using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

namespace Pancake.Debugging
{
	public class DebugLogExtensionsDemoWindow : EditorWindow
	{
		public static DebugLogExtensionsDemoWindow Instance
		{
			get;
			private set;
		}

		private DebugLogExtensionsDemoComponent target;

		private bool trackingIsVisible;
		private bool trackingRotation;
		private bool trackingRotationSpeed;

		private bool displayingIsVisible;
		private bool displayingRotation;
		private bool displayingRotationSpeed;

		[UsedImplicitly]
		private void OnEnable()
		{
			Instance = this;

			name = "Demo Window";
			titleContent = new GUIContent("Demo Window");
			minSize = new Vector2(470f, 650f);

			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			target = FindObjectOfType<DebugLogExtensionsDemoComponent>();
			if(target == null)
			{
				Debug.LogWarning("Closing Debug.Log Extensions Demo Window because demo scene was not open.");
				Close();
			}
		}

		private void OnActiveSceneChanged(Scene wasActive, Scene isActive)
		{
			if(target == null)
			{
				target = FindObjectOfType<DebugLogExtensionsDemoComponent>();
				if(target == null)
				{
					Debug.LogWarning("Closing Debug.Log Extensions Demo Window because demo scene was closed.");
					Close();
				}
			}
		}

		[UsedImplicitly]
		private void OnGUI()
		{
			if(target == null)
			{
				Debug.LogWarning("Closing Debug.Log Extensions Demo Window because demo component was not found in scene.");
				Close();
				return;
			}

			GUILayout.BeginHorizontal();

			GUILayout.Space(10f);

			GUILayout.BeginVertical();

			GUILayout.Space(10f);

			GUILayout.Label("Log Field Name And Value", EditorStyles.boldLabel);

			if(GUILayout.Button("Log IsVisible", GUILayout.Height(20f)))
			{
				target.LogIsVisible();
			}
			CopiableLabel("Debug.Log(()=>IsVisible);");

			GUILayout.Space(5f);

			if(GUILayout.Button("Log Rotation", GUILayout.Height(20f)))
			{
				target.LogRotation();
			}
			CopiableLabel("Debug.Log(()=>Rotation);");

			GUILayout.Space(5f);

			if(GUILayout.Button("Log Rotation Speed", GUILayout.Height(20f)))
			{
				target.LogRotationSpeed();
			}
			CopiableLabel("Debug.Log(()=>rotationSpeed);");

			GUILayout.Space(10f);

			GUILayout.Label("Log Full State", EditorStyles.boldLabel);

			if(GUILayout.Button("Log Public Members", GUILayout.Height(25f)))
			{
				target.LogPublicMembers();
			}
			CopiableLabel("Debug.LogState(this);");

			GUILayout.Space(5f);

			if(GUILayout.Button("Log Private Members", GUILayout.Height(25f)))
			{
				target.LogPrivateMembers();
			}
			CopiableLabel("Debug.LogState(this, BindingFlags.Instance | BindingFlags.NonPublic);");

			GUILayout.Space(10f);

			GUILayout.Label("Track Value Changes", EditorStyles.boldLabel);

			bool set = EditorGUILayout.Toggle("Track IsVisible", trackingIsVisible);
			if(set != trackingIsVisible)
			{
				trackingIsVisible = set;
				if(set)
				{
					Debug.LogChanges(() => target.IsVisible);
				}
				else
				{
					Debug.CancelLogChanges(() => target.IsVisible);
				}
			}
			CopiableLabel("Debug.LogChanges(()=>target.IsVisible);");

			GUILayout.Space(5f);

			set = EditorGUILayout.Toggle("Track Rotation", trackingRotation);
			if(set != trackingRotation)
			{
				trackingRotation = set;
				if(set)
				{
					Debug.LogChanges(() => target.Rotation);
				}
				else
				{
					Debug.CancelLogChanges(() => target.Rotation);
				}
			}
			CopiableLabel("Debug.LogChanges(()=>target.Rotation);");

			GUILayout.Space(5f);

			set = EditorGUILayout.Toggle("Track Rotation Speed", trackingRotationSpeed);
			if(set != trackingRotationSpeed)
			{
				trackingRotationSpeed = set;
				if(set)
				{
					Debug.LogChanges(() => target.rotationSpeed);
				}
				else
				{
					Debug.CancelLogChanges(() => target.rotationSpeed);
				}
			}
			CopiableLabel("Debug.LogChanges(()=>target.rotationSpeed);");

			GUILayout.Space(10f);

			GUILayout.Label("Display Values On Screen", EditorStyles.boldLabel);

			set = EditorGUILayout.Toggle("Display IsVisible", displayingIsVisible);
			if(set != displayingIsVisible)
			{
				displayingIsVisible = set;
				if(set)
				{
					Debug.DisplayOnScreen(() => target.IsVisible);
				}
				else
				{
					Debug.CancelDisplayOnScreen(() => target.IsVisible);
				}
			}
			CopiableLabel("Debug.DisplayOnScreen(()=>target.IsVisible);");

			GUILayout.Space(5f);

			set = EditorGUILayout.Toggle("Display Rotation", displayingRotation);
			if(set != displayingRotation)
			{
				displayingRotation = set;
				if(set)
				{
					Debug.DisplayOnScreen(() => target.Rotation);
				}
				else
				{
					Debug.CancelDisplayOnScreen(() => target.Rotation);
				}
			}
			CopiableLabel("Debug.DisplayOnScreen(()=>target.Rotation);");

			GUILayout.Space(5f);

			set = EditorGUILayout.Toggle("Display Rotation Speed", displayingRotationSpeed);
			if(set != displayingRotationSpeed)
			{
				displayingRotationSpeed = set;
				if(set)
				{
					Debug.DisplayOnScreen(() => target.rotationSpeed);
				}
				else
				{
					Debug.CancelDisplayOnScreen(() => target.rotationSpeed);
				}
			}
			CopiableLabel("Debug.DisplayOnScreen(()=>target.rotationSpeed);");

			GUILayout.Space(10f);

			GUILayout.EndVertical();

			GUILayout.Space(10f);
			
			GUILayout.EndHorizontal();
		}

		private static void CopiableLabel(string text)
		{
			if(GUILayout.Button(text, EditorStyles.helpBox))
			{
				GUIUtility.systemCopyBuffer = text;
				Debug.Log("Copied code snippet to clipboard.");
			}
		}

		[UsedImplicitly]
		private void OnDisable()
		{
			if(Instance == this)
			{
				Instance = null;
			}

			SceneManager.activeSceneChanged -= OnActiveSceneChanged;

			if(target != null)
			{
				if(trackingIsVisible)
				{
					Debug.CancelLogChanges(() => target.IsVisible);
				}
				if(trackingRotation)
				{
					Debug.CancelLogChanges(() => target.Rotation);
				}
				if(trackingRotationSpeed)
				{
					Debug.CancelLogChanges(() => target.rotationSpeed);
				}

				if(displayingIsVisible)
				{
					Debug.CancelDisplayOnScreen(() => target.IsVisible);
				}
				if(displayingRotation)
				{
					Debug.CancelDisplayOnScreen(() => target.Rotation);
				}
				if(displayingRotationSpeed)
				{
					Debug.CancelDisplayOnScreen(() => target.rotationSpeed);
				}
			}
		}
	}
}