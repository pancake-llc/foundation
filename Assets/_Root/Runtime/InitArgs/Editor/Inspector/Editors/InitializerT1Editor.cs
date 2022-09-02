using System;
using System.Reflection;
using Pancake.Init;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Init
{
	using static InitializerEditorUtility;

	[CustomEditor(typeof(Initializer<,>), true), CanEditMultipleObjects]
    public sealed class InitializerT1Editor : InitializerEditor
    {
        private SerializedProperty argument;
        private Type argumentType;
        private PropertyDrawer propertyDrawer;
        private GUIContent argumentLabel;
        private bool argumentIsService;

        protected override void Setup(Type[] genericArguments)
        {
            argument = serializedObject.FindProperty(nameof(argument));
            
            var clientType = genericArguments[0];
            argumentType = genericArguments[1];

            argumentIsService = IsService(argumentType);

            var initializerType = target.GetType();
            var metadata = initializerType.GetNestedType(InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic);
            var members = metadata is null ? Array.Empty<MemberInfo>() : metadata.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // constructor + 1 other member
            if(members.Length == 2)
			{
                Array.Sort(members, (f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

                TryGetAttributeBasedPropertyDrawer(metadata, argument, argumentType, out propertyDrawer);

                argumentLabel = GetLabel(members[0]);
            }
            else
            {
                argumentLabel = GetArgumentLabel(clientType, argumentType);
            }
        }

        protected override void DrawArgumentFields(bool nullAllowed)
		{
            DrawArgumentField(argument, argumentType, argumentLabel, propertyDrawer, argumentIsService, nullAllowed);
        }
    }
}