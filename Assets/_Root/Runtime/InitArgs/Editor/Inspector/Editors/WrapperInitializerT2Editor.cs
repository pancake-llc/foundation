using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    using static InitializerEditorUtility;

    [CustomEditor(typeof(WrapperInitializer<,,,>), true), CanEditMultipleObjects]
    public sealed class WrapperInitializerT2Editor : InitializerEditor
    {
        private SerializedProperty firstArgument;
        private SerializedProperty secondArgument;

        private Type firstArgumentType;
        private Type secondArgumentType;

        private PropertyDrawer firstPropertyDrawer;
        private PropertyDrawer secondPropertyDrawer;

        private GUIContent firstArgumentLabel;
        private GUIContent secondArgumentLabel;

        private bool firstArgumentIsService;
        private bool secondArgumentIsService;

        protected override int InitArgumentCount => 2;

        protected override void Setup(Type[] genericArguments)
        {
            firstArgument = serializedObject.FindProperty(nameof(firstArgument));
            secondArgument = serializedObject.FindProperty(nameof(secondArgument));

            firstArgumentType = genericArguments[2];
            secondArgumentType = genericArguments[3];

            firstArgumentIsService = IsService(firstArgumentType);
            secondArgumentIsService = IsService(secondArgumentType);

            var initializerType = target.GetType();
            var metadata = initializerType.GetNestedType(InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic);
            var members = metadata is null ? Array.Empty<MemberInfo>() : metadata.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // constructor + 2 other members
            if(members.Length == 3)
			{
                Array.Sort(members, (f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

                TryGetAttributeBasedPropertyDrawer(metadata, firstArgument, firstArgumentType, out firstPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, secondArgument, secondArgumentType, out secondPropertyDrawer);

                firstArgumentLabel = GetLabel(members[0]);
                secondArgumentLabel = GetLabel(members[1]);
            }
            else
            {
                var wrappedType = genericArguments[1];
                firstArgumentLabel = GetArgumentLabel(wrappedType, firstArgumentType);
                secondArgumentLabel = GetArgumentLabel(wrappedType, secondArgumentType);
            }
        }

        protected override string GetInitArgumentsHeader(Type[] genericArguments) => "Constructor Arguments";

        protected override void DrawArgumentFields(bool nullAllowed, bool servicesShown)
        {
            DrawArgumentField(firstArgument, firstArgumentType, firstArgumentLabel, firstPropertyDrawer, firstArgumentIsService, nullAllowed, servicesShown);
            DrawArgumentField(secondArgument, secondArgumentType, secondArgumentLabel, secondPropertyDrawer, secondArgumentIsService, nullAllowed, servicesShown);
        }
    }
}