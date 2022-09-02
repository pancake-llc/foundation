using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    using static InitializerEditorUtility;

    [CustomEditor(typeof(WrapperInitializer<,,,,,,,>), true), CanEditMultipleObjects]
    public sealed class WrapperInitializerT6Editor : InitializerEditor
    {
        private SerializedProperty firstArgument;
        private SerializedProperty secondArgument;
        private SerializedProperty thirdArgument;
        private SerializedProperty fourthArgument;
        private SerializedProperty fifthArgument;
        private SerializedProperty sixthArgument;

        private Type firstArgumentType;
        private Type secondArgumentType;
        private Type thirdArgumentType;
        private Type fourthArgumentType;
        private Type fifthArgumentType;
        private Type sixthArgumentType;

        private PropertyDrawer firstPropertyDrawer;
        private PropertyDrawer secondPropertyDrawer;
        private PropertyDrawer thirdPropertyDrawer;
        private PropertyDrawer fourthPropertyDrawer;
        private PropertyDrawer fifthPropertyDrawer;
        private PropertyDrawer sixthPropertyDrawer;

        private GUIContent firstArgumentLabel;
        private GUIContent secondArgumentLabel;
        private GUIContent thirdArgumentLabel;
        private GUIContent fourthArgumentLabel;
        private GUIContent fifthArgumentLabel;
        private GUIContent sixthArgumentLabel;

        private bool firstArgumentIsService;
        private bool secondArgumentIsService;
        private bool thirdArgumentIsService;
        private bool fourthArgumentIsService;
        private bool fifthArgumentIsService;
        private bool sixthArgumentIsService;

        protected override int InitArgumentCount => 6;

        protected override void Setup(Type[] genericArguments)
        {
            firstArgument = serializedObject.FindProperty(nameof(firstArgument));
            secondArgument = serializedObject.FindProperty(nameof(secondArgument));
            thirdArgument = serializedObject.FindProperty(nameof(thirdArgument));
            fourthArgument = serializedObject.FindProperty(nameof(fourthArgument));
            fifthArgument = serializedObject.FindProperty(nameof(fifthArgument));
            sixthArgument = serializedObject.FindProperty(nameof(sixthArgument));

            firstArgumentType = genericArguments[2];
            secondArgumentType = genericArguments[3];
            thirdArgumentType = genericArguments[4];
            fourthArgumentType = genericArguments[5];
            fifthArgumentType = genericArguments[6];
            sixthArgumentType = genericArguments[7];

            firstArgumentIsService = IsService(firstArgumentType);
            secondArgumentIsService = IsService(secondArgumentType);
            thirdArgumentIsService = IsService(thirdArgumentType);
            fourthArgumentIsService = IsService(fourthArgumentType);
            fifthArgumentIsService = IsService(fifthArgumentType);
            sixthArgumentIsService = IsService(sixthArgumentType);

            var initializerType = target.GetType();
            var metadata = initializerType.GetNestedType(InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic);
            var members = metadata is null ? Array.Empty<MemberInfo>() : metadata.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // constructor + 6 other members
            if(members.Length == 7)
			{
                Array.Sort(members, (f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

                TryGetAttributeBasedPropertyDrawer(metadata, firstArgument, firstArgumentType, out firstPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, secondArgument, secondArgumentType, out secondPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, thirdArgument, thirdArgumentType, out thirdPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, fourthArgument, fourthArgumentType, out fourthPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, fifthArgument, fifthArgumentType, out fifthPropertyDrawer);
                TryGetAttributeBasedPropertyDrawer(metadata, sixthArgument, sixthArgumentType, out sixthPropertyDrawer);
                
                firstArgumentLabel = GetLabel(members[0]);
                secondArgumentLabel = GetLabel(members[1]);
                thirdArgumentLabel = GetLabel(members[2]);
                fourthArgumentLabel = GetLabel(members[3]);
                fifthArgumentLabel = GetLabel(members[4]);
                sixthArgumentLabel = GetLabel(members[5]);
            }
            else
            {
                var wrappedType = genericArguments[1];
                firstArgumentLabel = GetArgumentLabel(wrappedType, firstArgumentType);
                secondArgumentLabel = GetArgumentLabel(wrappedType, secondArgumentType);
                thirdArgumentLabel = GetArgumentLabel(wrappedType, thirdArgumentType);
                fourthArgumentLabel = GetArgumentLabel(wrappedType, fourthArgumentType);
                fifthArgumentLabel = GetArgumentLabel(wrappedType, fifthArgumentType);
                sixthArgumentLabel = GetArgumentLabel(wrappedType, sixthArgumentType);
            }
        }

        protected override string GetInitArgumentsHeader(Type[] genericArguments) => "Constructor Arguments";

        protected override void DrawArgumentFields(bool nullAllowed)
        {
            DrawArgumentField(firstArgument, firstArgumentType, firstArgumentLabel, firstPropertyDrawer, firstArgumentIsService, nullAllowed);
            DrawArgumentField(secondArgument, secondArgumentType, secondArgumentLabel, secondPropertyDrawer, secondArgumentIsService, nullAllowed);
            DrawArgumentField(thirdArgument, thirdArgumentType, thirdArgumentLabel, thirdPropertyDrawer, thirdArgumentIsService, nullAllowed);
            DrawArgumentField(fourthArgument, fourthArgumentType, fourthArgumentLabel, fourthPropertyDrawer, fourthArgumentIsService, nullAllowed);
            DrawArgumentField(fifthArgument, fifthArgumentType, fifthArgumentLabel, fifthPropertyDrawer, fifthArgumentIsService, nullAllowed);
            DrawArgumentField(sixthArgument, sixthArgumentType, sixthArgumentLabel, sixthPropertyDrawer, sixthArgumentIsService, nullAllowed);
        }
    }
}