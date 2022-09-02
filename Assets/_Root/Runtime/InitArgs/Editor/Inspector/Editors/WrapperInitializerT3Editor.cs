using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    using static InitializerEditorUtility;

    [CustomEditor(typeof(WrapperInitializer<,,,,>), true), CanEditMultipleObjects]
    public sealed class WrapperInitializerT3Editor : InitializerEditor
    {
        private SerializedProperty firstArgument;
        private SerializedProperty secondArgument;
        private SerializedProperty thirdArgument;

        private Type firstArgumentType;
        private Type secondArgumentType;
        private Type thirdArgumentType;

        private PropertyDrawer firstPropertyDrawer;
        private PropertyDrawer secondPropertyDrawer;
        private PropertyDrawer thirdPropertyDrawer;

        private GUIContent firstArgumentLabel;
        private GUIContent secondArgumentLabel;
        private GUIContent thirdArgumentLabel;

        private bool firstArgumentIsService;
        private bool secondArgumentIsService;
        private bool thirdArgumentIsService;

        protected override int InitArgumentCount => 3;

        protected override void Setup(Type[] genericArguments)
        {
            firstArgument = serializedObject.FindProperty(nameof(firstArgument));
            secondArgument = serializedObject.FindProperty(nameof(secondArgument));
            thirdArgument = serializedObject.FindProperty(nameof(thirdArgument));

            firstArgumentType = genericArguments[2];
            secondArgumentType = genericArguments[3];
            thirdArgumentType = genericArguments[4];

            firstArgumentIsService = IsService(firstArgumentType);
            secondArgumentIsService = IsService(secondArgumentType);
            thirdArgumentIsService = IsService(thirdArgumentType);

            var initializerType = target.GetType();
            var metadata = initializerType.GetNestedType(InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic);
            var members = metadata is null ? Array.Empty<MemberInfo>() : metadata.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // constructor + 3 other members
            if(members.Length == 4)
			{
                Array.Sort(members, (f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

                TryGetAttributeBasedPropertyDrawer(metadata, firstArgument, firstArgumentType, out firstPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, secondArgument, secondArgumentType, out secondPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, thirdArgument, thirdArgumentType, out thirdPropertyDrawer);

                firstArgumentLabel = GetLabel(members[0]);
                secondArgumentLabel = GetLabel(members[1]);
                thirdArgumentLabel = GetLabel(members[2]);
            }
            else
            {
                var wrappedType = genericArguments[1];
                firstArgumentLabel = GetArgumentLabel(wrappedType, firstArgumentType);
                secondArgumentLabel = GetArgumentLabel(wrappedType, secondArgumentType);
                thirdArgumentLabel = GetArgumentLabel(wrappedType, thirdArgumentType);
            }
        }

        protected override string GetInitArgumentsHeader(Type[] genericArguments) => "Constructor Arguments";

        protected override void DrawArgumentFields(bool nullAllowed)
        {
            DrawArgumentField(firstArgument, firstArgumentType, firstArgumentLabel, firstPropertyDrawer, firstArgumentIsService, nullAllowed);
            DrawArgumentField(secondArgument, secondArgumentType, secondArgumentLabel, secondPropertyDrawer, secondArgumentIsService, nullAllowed);
            DrawArgumentField(thirdArgument, thirdArgumentType, thirdArgumentLabel, thirdPropertyDrawer, thirdArgumentIsService, nullAllowed);
        }
    }
}