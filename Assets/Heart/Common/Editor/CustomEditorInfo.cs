using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;

namespace PancakeEditor
{
    /// <summary>
    /// Contains information about a single custom editor targeting a particular <see cref="UnityEngine.Object"/> class.
    /// <para>
    /// Mirrors the internal UnityEditor.CustomEditorAttributes.MonoEditorType type.
    /// </para>
    /// </summary>
    public readonly struct CustomEditorInfo
    {
        internal readonly static Type monoEditorTypeType;
        internal static readonly FieldInfo inspectorTypeField;
        internal static readonly FieldInfo supportedRenderPipelineTypesField;
        internal static readonly FieldInfo editorForChildClassesField;
        internal static readonly FieldInfo isFallbackField;

        internal readonly Type inspectorType;
        internal readonly Type[] supportedRenderPipelineTypes;
        internal readonly bool editorForChildClasses;
        internal readonly bool isFallback;

        /// <summary>
        /// Contains information about a single custom editor targeting a particular <see cref="UnityEngine.Object"/> class.
        /// <para>
        /// Mirrors the internal UnityEditor.CustomEditorAttributes.MonoEditorType type.
        /// </para>
        /// </summary>
        static CustomEditorInfo()
        {
            monoEditorTypeType = GetInternalEditorType("UnityEditor.CustomEditorAttributes")
                .GetNestedType("MonoEditorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            inspectorTypeField = monoEditorTypeType.GetField("inspectorType");
            supportedRenderPipelineTypesField = monoEditorTypeType.GetField("supportedRenderPipelineTypes");
            editorForChildClassesField = monoEditorTypeType.GetField("editorForChildClasses");
            isFallbackField = monoEditorTypeType.GetField("isFallback");
        }

        public CustomEditorInfo(Type inspectorType, bool editorForChildClasses = false, bool isFallback = false)
        {
            this.inspectorType = inspectorType;
            supportedRenderPipelineTypes = Type.EmptyTypes;
            this.editorForChildClasses = editorForChildClasses;
            this.isFallback = isFallback;
        }

        public CustomEditorInfo(Type inspectorType, Type[] supportedRenderPipelineTypes, bool editorForChildClasses, bool isFallback)
        {
            this.inspectorType = inspectorType;
            this.supportedRenderPipelineTypes = supportedRenderPipelineTypes;
            this.editorForChildClasses = editorForChildClasses;
            this.isFallback = isFallback;
        }

        public CustomEditorInfo(object obj)
        {
            inspectorType = inspectorTypeField.GetValue(obj) as Type;
            supportedRenderPipelineTypes = (Type[]) supportedRenderPipelineTypesField.GetValue(obj) ?? Type.EmptyTypes;
            editorForChildClasses = (bool) editorForChildClassesField.GetValue(obj);
            isFallback = (bool) isFallbackField.GetValue(obj);
        }

        public static CustomEditorInfo[] Create(IList internalList) // List<MonoEditorType>
        {
            int count = internalList.Count;
            var result = new CustomEditorInfo[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new CustomEditorInfo(internalList[i]);
            }

            return result;
        }

        public static Type ExtractInspectorType(object internalMonoEditorType) => inspectorTypeField.GetValue(internalMonoEditorType) as Type;

        public object ToInternalType()
        {
            object instance;
            try
            {
                instance = Activator.CreateInstance(monoEditorTypeType);
            }

            catch
            {
                instance = FormatterServices.GetUninitializedObject(monoEditorTypeType);
            }

            inspectorTypeField.SetValue(instance, inspectorType);
            supportedRenderPipelineTypesField.SetValue(instance, supportedRenderPipelineTypes);
            editorForChildClassesField.SetValue(instance, editorForChildClasses);
            isFallbackField.SetValue(instance, isFallback);

            return instance;
        }

        public IList ToInternalTypeList()
        {
            var listType = typeof(List<>).MakeGenericType(monoEditorTypeType);
            var list = Activator.CreateInstance(listType) as IList;
            list.Add(ToInternalType());
            return list;
        }

        private static Type GetInternalEditorType(string fullTypeName)
        {
            var type = typeof(Editor).Assembly.GetType(fullTypeName);

            return type;
        }
    }
}