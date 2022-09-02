using System;
using UnityEngine;
using UnityEditor;
using Pancake.Init.Internal;
using static Pancake.Init.EditorOnly.InitializerEditorUtility;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	public abstract class InitializerEditor : UnityEditor.Editor
	{
        protected const string InitArgumentMetadataClassName = InitializerUtility.InitArgumentMetadataClassName;

        private SerializedProperty client;
        private SerializedProperty nullArgumentGuard;
        private GUIContent clientLabel;
        private bool clientIsInitializable;
        private InitializerDrawer drawer;
        private Type clientType;

        protected virtual Type[] GenericArguments => target.GetType().BaseType.GetGenericArguments();
        protected virtual int InitArgumentCount => GetInitArgumentCount(clientType);

        private bool IsNullAllowed
        {
            get
            {
                var nullGuard = (NullArgumentGuard)nullArgumentGuard.intValue;
                return !nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning)
                   || (!nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs) && PrefabUtility.IsPartOfPrefabAsset(target));
            }
        }

        internal void Setup(bool createDrawer)
        {
            client = serializedObject.FindProperty("target");
            nullArgumentGuard = serializedObject.FindProperty(nameof(nullArgumentGuard));

            var genericArguments = GenericArguments;            
            clientType = GetClientType(genericArguments);
            clientIsInitializable = IsInitializable(clientType);
            clientLabel = GetClientLabel();
            int initArgumentCount = InitArgumentCount;
            var initializers = targets;
            int count = targets.Length;
            var clients = new Object[count];
            for(int i = 0; i < count; i++)
			{
                clients[i] = (initializers[i] as IInitializer).Target;
            }

            Setup(genericArguments);

            if(createDrawer)
            {
                var initArguments = GetInitArguments(genericArguments, initArgumentCount);
                drawer = new InitializerDrawer(clientType, clients, initArguments, this);
            }

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

		protected virtual string GetInitArgumentsHeader(Type[] genericArguments) => genericArguments.Length <= 2 ? "Init Argument" : "Init Arguments";

        protected virtual Type GetClientType(Type[] genericArguments) => genericArguments[0];

        protected abstract void Setup(Type[] genericArguments);

        public override void OnInspectorGUI()
		{
            serializedObject.Update();

			if(client == null)
			{
				Setup(true);
			}

			if(drawer == null)
			{
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
			drawer.OnInspectorGUI();
			DrawClientField(rect, client, clientLabel, clientIsInitializable);

            serializedObject.ApplyModifiedProperties();
		}

		public void DrawArgumentFields()
        {
            if(client == null)
            {
                Setup(true);
            }

            DrawArgumentFields(IsNullAllowed);
        }

        protected abstract void DrawArgumentFields(bool nullAllowed);

        private void OnDestroy()
		{
            if(drawer is null)
			{
                return;
			}

            drawer.Dispose();
            drawer = null;
        }

        private static Type[] GetInitArguments(Type[] genericArguments, int initArgumentCount)
		{
            var results = new Type[initArgumentCount];

            int offset = genericArguments.Length - initArgumentCount;
            for(int i = 0; i < initArgumentCount; i++)
			{
                results[i] = genericArguments[offset + i];
			}

            return results;
		}
    }
}