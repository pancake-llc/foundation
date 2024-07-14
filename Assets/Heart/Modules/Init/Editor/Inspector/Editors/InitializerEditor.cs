//#define DEBUG_ENABLED

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly
{
	using static Internal.InitializerEditorUtility;

	[CanEditMultipleObjects]
	public class InitializerEditor : Editor
	{
        protected internal const string InitArgumentMetadataClassName = InitializerUtility.InitArgumentMetadataClassName;

        private bool setupDone;
        private SerializedProperty client;
        private SerializedProperty nullArgumentGuard;
        private GUIContent clientLabel;
        private bool clientIsInitializable;
        private bool hasServiceArguments;
        private InitializerGUI ownedInitializerGUI;
        private InitializerGUI externallyOwnedInitializerGUI;
        [NonSerialized]
        private InitParameterGUI[] parameterGUIs = new InitParameterGUI[0];
        private Type clientType;
        private bool drawNullArgumentGuard;
        private ServiceChangedListener[] serviceChangedListeners = Array.Empty<ServiceChangedListener>();

        protected virtual Type[] GetGenericArguments() => target.GetType().BaseType.GetGenericArguments();
        protected virtual Type GetClientType(Type[] genericArguments) => genericArguments[0];
        protected virtual Type[] GetInitArgumentTypes(Type[] genericArguments) => genericArguments.Skip(1).ToArray();

        private static bool ServicesShown => InitializerGUI.ServicesShown;

        protected virtual bool HasUserDefinedInitArgumentFields { get; }

        private bool IsNullAllowed
        {
            get
            {
                if(!drawNullArgumentGuard)
				{
                    return true;
				}

                var nullGuard = (NullArgumentGuard)nullArgumentGuard.intValue;
                return !nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning)
                   || (!nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs) && PrefabUtility.IsPartOfPrefabAsset(target));
            }
        }

        internal void Setup([AllowNull] InitializerGUI externallyOwnedInitializerGUI)
        {
            #if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.Setup() with Event.current:{Event.current.type}");
			#endif

            setupDone = true;

            this.externallyOwnedInitializerGUI = externallyOwnedInitializerGUI;
            client = serializedObject.FindProperty("target");
            nullArgumentGuard = serializedObject.FindProperty(nameof(nullArgumentGuard));
            drawNullArgumentGuard = nullArgumentGuard != null;

            var genericArguments = GetGenericArguments();
            clientType = GetClientType(genericArguments);
            clientIsInitializable = IsInitializable(clientType);
            clientLabel = GetClientLabel();
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

            if(count > 0 && clients[0] == null)
			{
                clients = Array.Empty<Object>();
			}

            var initArguments = GetInitArgumentTypes(genericArguments);
            if(externallyOwnedInitializerGUI is null)
            {
                DisposeOwnedInitializerGUI();
                ownedInitializerGUI = new InitializerGUI(targets, clients, initArguments, this);
				ownedInitializerGUI.Changed += OnOwnedInitializerGUIChanged;
            }

            SetupParameterGUIs(initArguments);
            hasServiceArguments = Array.Exists(parameterGUIs, d => d.isService);

            ServiceChangedListener.UpdateAll(ref serviceChangedListeners, genericArguments, OnInitArgumentServiceChanged);

            LayoutUtility.Repaint(this);

            GUIContent GetClientLabel()
			{
                var result = GetLabel(clientType);
                if(typeof(StateMachineBehaviour).IsAssignableFrom(clientType))
				{
                    result.text = "None (Animator → " + result.text + ")";
                }
                else if(typeof(ScriptableObject).IsAssignableFrom(clientType))
				{
                    result.text = "None (" + result.text + ")";
                }
                else
                {
                    result.text = "New Instance (" + result.text + ")";
                }

                return result;
            }
        }

		// This can get called during deserialization, which could result in errors
		private void OnInitArgumentServiceChanged() => EditorApplication.delayCall += ()=>
		{
            if(!this)
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
            parameterGUIs = HasUserDefinedInitArgumentFields ? Array.Empty<InitParameterGUI>() : CreateParameterGUIs(clientType, argumentTypes);
            
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

        private InitParameterGUI[] CreateParameterGUIs(Type clientType, Type[] argumentTypes)
            => InitializerEditorUtility.CreateParameterGUIs(serializedObject, clientType, argumentTypes);

        public override void OnInspectorGUI()
		{
            HandleBeginGUI();

            bool hierarchyModeWas = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            serializedObject.Update();

			if(client == null)
			{
				Setup(null);
			}

			if(ownedInitializerGUI == null)
			{
                EditorGUIUtility.hierarchyMode = hierarchyModeWas;
				return;
			}

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

            GUILayout.Space(-EditorGUIUtility.singleLineHeight - 2f);
			ownedInitializerGUI.OnInspectorGUI();

            if(client.objectReferenceValue is Component)
            {
			    DrawClientField(rect, client, clientLabel, clientIsInitializable, hasServiceArguments);
            }

            serializedObject.ApplyModifiedProperties();

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
                var serializedProperty = serializedObject.GetIterator();
                serializedProperty.NextVisible(true);
                while(serializedProperty.NextVisible(false))
				{
                    EditorGUILayout.PropertyField(serializedProperty);
				}

                return;
			}

			for(int i = 0; i < count; i++)
			{
				InitParameterGUI drawer = parameterGUIs[i];
				drawer.DrawArgumentField(IsNullAllowed, ServicesShown);
			}
        }

        private void OnDisable()
		{
            AnyPropertyDrawer.DisposeAllStaticState();
            AnyPropertyDrawer.UserSelectedTypeChanged -= OnInitArgUserSelectedTypeChanged;
            ServiceChangedListener.DisposeAll(ref serviceChangedListeners);
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
			Repaint();
			LayoutUtility.ExitGUI(this);
		}

        private void OnOwnedInitializerGUIChanged(InitializerGUI initializerGUI)
		{
            #if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"{GetType().Name}.OnInitializerGUIChanged with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			setupDone = false;
			Repaint();
		}
    }
}