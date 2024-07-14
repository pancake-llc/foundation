#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class AnyOdinAttributeProcessor<T> : OdinAttributeProcessor<Any<T>>
	{
		private readonly HashSet<Type> attributeBlacklist = new HashSet<Type>()
		{
			typeof(ShowIfAttribute),
			typeof(HideIfAttribute),
			typeof(EnableIfAttribute),
			typeof(DisableIfAttribute)
		};

		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			if(!string.Equals(member.Name, nameof(Any<object>.reference)) && !string.Equals(member.Name, nameof(Any<object>.value)))
			{
				return;
			}

			var anyType = member.DeclaringType;
			var valueType = AnyPropertyDrawer.GetValueTypeFromAnyType(anyType);

			// Inject Attributes from members in private Init class defined inside the Initializer.
			Type maybeInitializerType = parentProperty.ParentType;
			if(typeof(IInitializer).IsAssignableFrom(maybeInitializerType))
			{
				if(parentProperty.Parent.ValueEntry.WeakSmartValue is IInitializerEditorOnly initializerEditorOnly
				&& initializerEditorOnly.NullArgumentGuard.HasFlag(NullArgumentGuard.EditModeWarning))
				{
					const string message = "This Init argument is required";
					const InfoMessageType messageType = InfoMessageType.Warning;

					#if DEV_MODE && DEBUG_ENABLED
					UnityEngine.Debug.Log($"Injecting attribute to {maybeInitializerType.Name}.{parentProperty.Name}.{member.Name}: RequiredAttribute(\"{message}\")");
					#endif

					attributes.Add(new RequiredAttribute(message, messageType));
				}

				if(InitializerEditorUtility.GetMetaDataClassType(maybeInitializerType) is Type metadataClass
				&& InitializerEditorUtility.TryGetInitParameterAttributesFromMetadata(parentProperty.Name, valueType, metadataClass, out var attributesToInject))
				{
					#if DEV_MODE && DEBUG_ENABLED
					UnityEngine.Debug.Log($"Injecting attributes to {maybeInitializerType.Name}.{parentProperty.Name}.{member.Name}: {string.Join(", ", attributesToInject)}");
					#endif

					Inject(attributes, attributesToInject);
					return;
				}
			}

			Inject(attributes, parentProperty.Info.Attributes);
		}

		private void Inject(List<Attribute> list, IEnumerable<Attribute> attributesToInject)
		{
			foreach(var attribute in attributesToInject)
			{
				if(!attributeBlacklist.Contains(attribute.GetType()))
				{
					list.Add(attribute);
				}
			}
		}
	}
}
#endif