//#define DEBUG_ENABLED
#define DEBUG_REPAINT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static Sisus.Init.EditorOnly.Internal.InitializerEditorUtility;

namespace Sisus.Init.EditorOnly
{
	[CustomEditor(typeof(Initializer), editorForChildClasses:true, isFallback = true), CanEditMultipleObjects]
	public class InitializerEditor : Editor
	{
		protected internal const string InitArgumentMetadataClassName = InitializerUtility.InitArgumentMetadataClassName;

		private static readonly Dictionary<int, string[]> fieldsToExcludeByArgumentCount = new(13)
		{
			{ 0, new[] { "m_Script" } },
			{ 1, new[] { "m_Script", "argument" } },
			{ 2, new[] { "m_Script", "firstArgument", "secondArgument" } },
			{ 3, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument" } },
			{ 4, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument" } },
			{ 5, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument" } },
			{ 6, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument" } },
			{ 7, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument" } },
			{ 8, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument" } },
			{ 9, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument" } },
			{ 10, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument" } },
			{ 11, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument", "eleventhArgument" } },
			{ 12, new[] { "m_Script", "firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument", "eleventhArgument", "twelfthArgument" } }
		};

		private bool setupDone;
		private SerializedProperty client;
		private SerializedProperty nullArgumentGuard;
		private bool clientIsInitializable;
		private InitializerGUI ownedInitializerGUI;
		private InitializerGUI externallyOwnedInitializerGUI;
		[NonSerialized]
		private InitParameterGUI[] parameterGUIs = new InitParameterGUI[0];
		private Type clientType;
		private bool drawNullArgumentGuard;
		private ServiceChangedListener targetServiceChangedListener;
		private ServiceChangedListener[] initArgumentServiceChangedListeners = Array.Empty<ServiceChangedListener>();
		private static bool ServicesShown => InitializerGUI.ServicesShown;
		protected virtual bool HasUserDefinedInitArgumentFields => false;

		private bool IsNullAllowed
		{
			get
			{
				if(!drawNullArgumentGuard)
				{
					return true;
				}

				var nullGuard = (NullArgumentGuard)nullArgumentGuard.intValue;
				return !nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning);
			}
		}

		internal void Setup([AllowNull] InitializerGUI externallyOwnedInitializerGUI)
		{
			var wasDrawing = LayoutUtility.NowDrawing;
			LayoutUtility.NowDrawing = this;

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.Setup() with Event.current:{Event.current.type}");
			#endif

			setupDone = true;

			this.externallyOwnedInitializerGUI = externallyOwnedInitializerGUI;
			client = serializedObject.FindProperty("target");
			nullArgumentGuard = serializedObject.FindProperty(nameof(nullArgumentGuard));
			drawNullArgumentGuard = nullArgumentGuard != null;

			if(!InitializerUtility.TryGetClientAndInitArgumentTypes(target.GetType(), out clientType, out var initArgumentTypes))
			{
				clientType = typeof(Object);
				initArgumentTypes = Array.Empty<Type>();
			}

			clientIsInitializable = IsInitializable(clientType);
			var initializers = targets;
			int count = targets.Length;
			var clients = new Object[count];
			for(int i = 0; i < count; i++)
			{
				var initializer = initializers[i] as IInitializer;
				clients[i] = initializer.Target;

				if(i == 0 && initializer is IInitializerEditorOnly initializerEditorOnly && !initializerEditorOnly.ShowNullArgumentGuard)
				{
					drawNullArgumentGuard = false;
				}
			}

			if(count > 0 && !clients[0])
			{
				clients = Array.Empty<Object>();
			}

			if(externallyOwnedInitializerGUI is null)
			{
				DisposeOwnedInitializerGUI();
				ownedInitializerGUI = new InitializerGUI(serializedObject, clients, initArgumentTypes, this);
				ownedInitializerGUI.Changed += OnOwnedInitializerGUIChanged;
			}

			SetupParameterGUIs(initArgumentTypes);

			LayoutUtility.NowDrawing = wasDrawing;

			if(clientType is not null)
			{
				targetServiceChangedListener ??= ServiceChangedListener.Create(clientType, OnTargetOrInitArgumentServiceChanged);
			}
			ServiceChangedListener.UpdateAll(ref initArgumentServiceChangedListeners, initArgumentTypes, OnTargetOrInitArgumentServiceChanged);

			LayoutUtility.Repaint(this);
		}

		// This can get called during deserialization, which could result in errors
		private void OnTargetOrInitArgumentServiceChanged() => EditorApplication.delayCall += ()=>
		{
			if(!this || !serializedObject.targetObject)
			{
				return;
			}

			OnDisable();

			Setup(externallyOwnedInitializerGUI);
		};

		/// <param name="argumentTypes">
		/// Types of the init arguments injected to the client.
		/// <para>
		/// In addition to the argument types, this can include the client type etc.
		/// </para>
		/// </param>
		private protected virtual void SetupParameterGUIs(Type[] argumentTypes)
		{
			DisposeParameterGUIs();
			parameterGUIs = HasUserDefinedInitArgumentFields ? Array.Empty<InitParameterGUI>() : CreateParameterGUIs(argumentTypes);

			AnyPropertyDrawer.UserSelectedTypeChanged -= OnInitArgUserSelectedTypeChanged;
			if(parameterGUIs.Length > 0)
			{
				AnyPropertyDrawer.UserSelectedTypeChanged += OnInitArgUserSelectedTypeChanged;
			}
		}

		private void OnInitArgUserSelectedTypeChanged(SerializedProperty changedProperty, Type userSelectedType)
		{
			for(int i = parameterGUIs.Length - 1; i >= 0; i--)
			{
				AnyPropertyDrawer.UpdateValueBasedState(parameterGUIs[i].anyProperty);
			}
		}

		private InitParameterGUI[] CreateParameterGUIs(Type[] argumentTypes)
			=> InitializerEditorUtility.CreateParameterGUIs(serializedObject, clientType, argumentTypes);

		public override void OnInspectorGUI()
		{
			HandleBeginGUI();

			bool hierarchyModeWas = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = true;

			if(client == null)
			{
				Setup(null);
			}

			if(ownedInitializerGUI == null)
			{
				EditorGUIUtility.hierarchyMode = hierarchyModeWas;
				return;
			}

			GUILayout.Space(-EditorGUIUtility.singleLineHeight);

			var rect = EditorGUILayout.GetControlRect();
			rect.y -= 2f;

			// Tooltip for icon must be drawn before drawer.OnInspectorGUI for it to
			// take precedence over Init header tooltip.
			var iconRect = rect;
			iconRect.x = EditorGUIUtility.labelWidth - 1f;
			iconRect.y += 5f;
			iconRect.width = 20f;
			iconRect.height = 20f;
			GUI.Label(iconRect, GetReferenceTooltip(client.serializedObject.targetObject, client.objectReferenceValue, clientIsInitializable));

			ownedInitializerGUI.OnInspectorGUI();

			EditorGUIUtility.hierarchyMode = hierarchyModeWas;
		}

		public virtual void DrawArgumentFields()
		{
			if(client == null)
			{
				Setup(null);
			}

			int count = parameterGUIs.Length;
			if(count == 0)
			{
				serializedObject.DrawPropertiesWithoutScriptField();
				return;
			}

			for(int i = 0; i < count; i++)
			{
				parameterGUIs[i].DrawArgumentField(IsNullAllowed, ServicesShown);
			}

			DrawPropertiesExcluding(serializedObject, fieldsToExcludeByArgumentCount[parameterGUIs.Length]);
		}

		private void OnDisable()
		{
			AnyPropertyDrawer.DisposeAllStaticState();
			AnyPropertyDrawer.UserSelectedTypeChanged -= OnInitArgUserSelectedTypeChanged;
			targetServiceChangedListener?.Dispose();
			targetServiceChangedListener = null;
			ServiceChangedListener.DisposeAll(ref initArgumentServiceChangedListeners);
			DisposeParameterGUIs();
			DisposeOwnedInitializerGUI();
		}

		private void DisposeOwnedInitializerGUI()
		{
			if(ownedInitializerGUI is null)
			{
				return;
			}

			ownedInitializerGUI.Changed -= OnOwnedInitializerGUIChanged;
			ownedInitializerGUI.Dispose();
			ownedInitializerGUI = null;
		}

		private void DisposeParameterGUIs()
		{
			for(int i = 0, count = parameterGUIs.Length; i < count; i++)
			{
				parameterGUIs[i].Dispose();
			}

			parameterGUIs = Array.Empty<InitParameterGUI>();
		}

		private void HandleBeginGUI()
		{
			LayoutUtility.BeginGUI(this);
			HandleSetup();
		}

		private void HandleSetup()
		{
			if(!setupDone || !(ownedInitializerGUI?.IsValid() ?? true))
			{
				SetupOrExitGUI();
			}
		}

		private void SetupOrExitGUI()
		{
			if(Event.current.type == EventType.Layout)
			{
				DisposeOwnedInitializerGUI();
				Setup(externallyOwnedInitializerGUI);
				return;
			}

			SetupDuringNextOnGUI();
		}

		private void SetupDuringNextOnGUI()
		{
			setupDone = false;

			#if DEV_MODE && DEBUG_REPAINT
			Debug.Log(GetType().Name + "Repaint");
			UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
			#endif

			Repaint();

			#if DEV_MODE && DEBUG_REPAINT
			UnityEngine.Profiling.Profiler.EndSample();
			#endif

			LayoutUtility.ExitGUI();
		}

		private void OnOwnedInitializerGUIChanged(InitializerGUI initializerGUI)
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;

			#if DEV_MODE && DEBUG_REPAINT
			Debug.Log(GetType().Name + "Repaint");
			UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
			#endif

			Repaint();

			#if DEV_MODE && DEBUG_REPAINT
			UnityEngine.Profiling.Profiler.EndSample();
			#endif
		}
	}
}