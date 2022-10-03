using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    using static InitializerEditorUtility;

    [CustomEditor(typeof(WrapperInitializer<,,>), true), CanEditMultipleObjects]
    public sealed class WrapperInitializerT1Editor : InitializerEditor
    {
        private SerializedProperty argument;
        private Type argumentType;
        private PropertyDrawer propertyDrawer;
        private GUIContent argumentLabel;
        private bool argumentIsService;

        protected override int InitArgumentCount => 1;

        protected override void Setup(Type[] genericArguments)
        {
            argument = serializedObject.FindProperty(nameof(argument));
            argumentType = genericArguments[2];
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
                var wrappedType = genericArguments[1];
                argumentLabel = GetArgumentLabel(wrappedType, argumentType);
            }
        }

        protected override string GetInitArgumentsHeader(Type[] genericArguments) => "Constructor Argument";

		protected override void DrawArgumentFields(bool nullAllowed, bool servicesShown)
            => DrawArgumentField(argument, argumentType, argumentLabel, propertyDrawer, argumentIsService, nullAllowed, servicesShown);
	}
}