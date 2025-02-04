using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	using static InitializerEditorUtility;

	internal static class ScriptGenerator
	{
		private const string InitializerTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for the <see cref=\"{3}\"/> {4}.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {5}<{3}, {6}>\r\n" +
			"\t{{\r\n" +
			"\t\t#if UNITY_EDITOR\r\n" +
			"\t\t/// <summary>\r\n" +
			"\t\t/// This section can be used to customize how the Init arguments will be drawn in the Inspector.\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// The Init argument names shown in the Inspector will match the names of members defined inside this section.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// </summary>\r\n" +
			"\t\tprivate sealed class Init\r\n" +
			"\t\t{{\r\n" +
			"{7}" +
			"\t\t}}\r\n" +
			"\t\t#endif\r\n" +
			"\t}}\r\n" +
			"}}\r\n";

		private const string InitializerBaseTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for the <see cref=\"{3}\"/> {4}.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {5}<{3}, {6}>\r\n" +
			"\t{{\r\n" +
			"{7}" +
			"\t}}\r\n" +
			"}}\r\n";

		private const string WrapperInitializerTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for the <see cref=\"{3}\"/> {4}.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {5}<{6}>\r\n" +
			"\t{{\r\n" +
			"\t\t#if UNITY_EDITOR\r\n" +
			"\t\t/// <summary>\r\n" +
			"\t\t/// This section can be used to customize how the Init arguments will be drawn in the Inspector.\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// The Init argument names shown in the Inspector will match the names of members defined inside this section.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// </summary>\r\n" +
			"\t\tprivate sealed class Init\r\n" +
			"\t\t{{\r\n" +
			"{7}" +
			"\t\t}}\r\n" +
			"\t\t#endif\r\n" +
			"\r\n" +
			"\t\tprotected override {3} CreateWrappedObject({8}) => new({9});\r\n" +
			"\t}}\r\n" +
			"}}\r\n";

		

		private const string WrapperInitializerBaseTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for the <see cref=\"{3}\"/> {4}.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {5}<{6}>\r\n" +
			"\t{{\r\n" +
			"{7}" +
			"\r\n" +
			"\t\tprotected override {3} CreateWrappedObject({8}) => new({9});\r\n" +
			"\t\t}}\r\n" +
			"}}\r\n";

		private const string CustomInitializerTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for the <see cref=\"{3}\"/> component.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {4}<{3}, {5}>\r\n" +
			"\t{{\r\n" +
			"\t\t#if UNITY_EDITOR\r\n" +
			"\t\t/// <summary>\r\n" +
			"\t\t/// This section can be used to customize how the Init arguments will be drawn in the Inspector.\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// The Init argument names shown in the Inspector will match the names of members defined inside this section.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// </summary>\r\n" +
			"\t\tprivate sealed class Init\r\n" +
			"\t\t{{\r\n" +
			"{6}" +
			"\t\t}}\r\n" +
			"\t\t#endif\r\n" +
			"\r\n" +
			"\t\tprotected override void InitTarget({7})\r\n" +
			"\t\t{{\r\n" +
			"{8}\t\t}}\r\n" +
			"\t}}\r\n" +
			"}}\r\n";

		private const string VisualElementInitializerTemplate =
			"{0}\r\n" +
			"namespace {1}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// Initializer for a <see cref=\"{3}\"/> in a <see cref=\"UIDocument\"/>.\r\n" +
			"\t/// </summary>\r\n" +
			"\tinternal sealed class {2} : {4}<{3}, {5}>\r\n" +
			"\t{{\r\n" +
			"\t\t#if UNITY_EDITOR\r\n" +
			"\t\t/// <summary>\r\n" +
			"\t\t/// This section can be used to customize how the Init arguments will be drawn in the Inspector.\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// The Init argument names shown in the Inspector will match the names of members defined inside this section.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// <para>\r\n" +
			"\t\t/// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.\r\n" +
			"\t\t/// </para>\r\n" +
			"\t\t/// </summary>\r\n" +
			"\t\tprivate sealed class Init\r\n" +
			"\t\t{{\r\n" +
			"{6}" +
			"\t\t}}\r\n" +
			"\t\t#endif\r\n" +
			"\r\n" +
			"\t\tprotected override void InitTarget({7})\r\n" +
			"\t\t{{\r\n" +
			"{8}\t\t}}\r\n" +
			"\t}}\r\n" +
			"}}\r\n";

		private const string WrapperTemplate =
			"using UnityEngine;\r\n" +
			"using Sisus.Init;\r\n" +
			"\r\n" +
			"namespace {0}\r\n" +
			"{{\r\n" +
			"\t/// <summary>\r\n" +
			"\t/// <see cref=\"Wrapper{{}}\"/> for the <see cref=\"{1}\"/> object.\r\n" +
			"\t/// </summary>\r\n" +
			"\t[AddComponentMenu(\"Wrapper/{2}\")]\r\n" +
			"\tinternal sealed class {1}Component : Wrapper<{1}> {{ }}\r\n" +
			"}}\r\n";

		private const string DefaultNamespace = "MyNamespace";

		private static readonly Dictionary<char, Dictionary<Type, string>> toStringCache = new Dictionary<char, Dictionary<Type, string>>(1)
		{
			{ '\0', new(4096) {
				{ typeof(int), "int" }, { typeof(uint), "unit" },
				{ typeof(float), "float" }, { typeof(double), "double" },
				{ typeof(bool), "bool" }, { typeof(string), "string" },
				{ typeof(short), "short" }, { typeof(ushort), "ushort" },
				{ typeof(byte), "byte" },{ typeof(sbyte), "sbyte" },
				{ typeof(long), "long" }, { typeof(ulong), "ulong" },
				{ typeof(object), "object" }, { typeof(decimal), "decimal" }
			} }
		};

		private static readonly string[] initializerBasePropertyNames =
		{
			"FirstArgument",
			"SecondArgument",
			"ThirdArgument",
			"FourthArgument",
			"FifthArgument",
			"SixthArgument",
			"SeventhArgument",
			"EighthArgument",
			"NinthArgument"
		};

		private static readonly HashSet<string> reservedKeywords = new()
		{
			"abstract",
			"as",
			"base",
			"bool",
			"break",
			"byte",
			"case",
			"catch",
			"char",
			"checked",
			"class",
			"const",
			"continue",
			"decimal",
			"default",
			"delegate",
			"do",
			"double",
			"else",
			"enum",
			"event",
			"explicit",
			"extern",
			"false",
			"finally",
			"fixed",
			"float",
			"for",
			"foreach",
			"goto",
			"if",
			"implicit",
			"in",
			"int",
			"interface",
			"internal",
			"is",
			"lock",
			"long",
			"namespace",
			"new",
			"null",
			"object",
			"operator",
			"out",
			"override",
			"params",
			"private",
			"protected",
			"public",
			"readonly",
			"ref",
			"return",
			"sbyte",
			"sealed",
			"short",
			"sizeof",
			"stackalloc",
			"static",
			"string",
			"struct",
			"switch",
			"this",
			"throw",
			"true",
			"try",
			"typeof",
			"uint",
			"ulong",
			"unchecked",
			"unsafe",
			"ushort",
			"using",
			"virtual",
			"void",
			"volatile",
			"while"
		};

		internal static string CreateInitializer([DisallowNull] object target)
		{
			if(target is MonoScript script)
			{
				return CreateInitializer(script);
			}

			if(target is MonoBehaviour monoBehaviour && MonoScript.FromMonoBehaviour(monoBehaviour) is { } monoBehaviourScript)
			{
				return CreateInitializer(monoBehaviourScript);
			}

			if(target is ScriptableObject scriptableObject && MonoScript.FromScriptableObject(scriptableObject) is { } scriptableObjectScript)
			{
				return CreateInitializer(scriptableObjectScript);
			}

			return CreateInitializerAtUserSelectedPath(target.GetType());
		}

		internal static string CreateInitializer([DisallowNull] MonoScript clientScript)
		{
			var clientType = clientScript.GetClass();
			if(!AssetDatabase.CanOpenForEdit(clientScript)) 
			{
				return CreateInitializerAtUserSelectedPath(clientType);
			}

			string clientScriptPath = AssetDatabase.GetAssetPath(clientScript);
			string clientTypeName = clientType != null ? NameOfWithoutIllegalCharacters(clientType) : Path.GetFileNameWithoutExtension(clientScriptPath);
			string saveAtPath = Path.Combine(Path.GetDirectoryName(clientScriptPath), clientTypeName + "Initializer.cs");
			return CreateInitializer(saveAtPath, clientType);
		}

		internal static string CreateInitializerAtUserSelectedPath(Type clientType = null)
		{
			string initialDirectoryToShow = Application.dataPath;
			if(clientType is not null && Find.Script(clientType, out MonoScript clientScript))
			{
				string clientScriptPath = AssetDatabase.GetAssetPath(clientScript);
				if(!string.IsNullOrEmpty(clientScriptPath))
				{
					string clientDirectoryPath = Path.GetDirectoryName(clientScriptPath);
					if(!string.IsNullOrEmpty(clientDirectoryPath) && clientDirectoryPath.StartsWith(Application.dataPath))
					{
						initialDirectoryToShow = clientDirectoryPath;
					}
				}
			}

			string initializerPath = EditorUtility.SaveFilePanel("Select Save Path", initialDirectoryToShow, NameOfWithoutIllegalCharacters(clientType) + "Initializer", "cs");
			#if DEV_MODE
			Debug.Log($"initializerPath:{initializerPath}, initialDirectoryToShow:{initialDirectoryToShow}");
			#endif
			if(string.IsNullOrEmpty(initializerPath))
			{
				return initializerPath;
			}

			return CreateInitializer(FileUtil.GetProjectRelativePath(initializerPath), clientType);
		}

		private static string CreateInitializer(string initializerPath, Type clientType = null)
		{
			string initializerTypeAsString = Path.GetFileNameWithoutExtension(initializerPath);
			string clientTypeAsString, @namespace;
			Type[] parametersTypes;

			if(clientType != null)
			{
				clientTypeAsString = TypeUtility.ToString(clientType);
				@namespace = clientType.Namespace ?? DefaultNamespace;
				parametersTypes = GetInitParameters(clientType);
			}
			else
			{
				clientTypeAsString = Path.GetFileNameWithoutExtension(initializerPath);
				if(clientTypeAsString.EndsWith("Initializer"))
				{
					clientTypeAsString = clientTypeAsString.Substring(0, clientTypeAsString.Length - "Initializer".Length);
				}

				@namespace = DefaultNamespace;
				parametersTypes = Type.EmptyTypes;
			}

			string genericArguments;
			string members = "";
			int parameterCount = parametersTypes.Length;
			HashSet<string> namespaces = new HashSet<string>() { "Sisus.Init" };

			bool canSerializeAllArgumentsAsReferences = true;
			HashSet<string> initializerFieldNames = new HashSet<string>();
			string[] typeNames = new string[parameterCount];
			string[] argumentTargetMemberNames = new string[parameterCount];
			InitializerType initializerType = GetInitializerType(clientType);

			if(parameterCount > 0)
			{
				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parametersTypes[i];
					if(!CanSerializeAsReference(parameterType))
					{
						canSerializeAllArgumentsAsReferences = false;
					}
				}
				
				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parametersTypes[i];
					string parameterTypeName = NameOf(parameterType, namespaces);
					typeNames[i] = parameterTypeName;

					string name;
					if(TryGetArgumentTargetMember(clientType, parameterType, i, initializerType == InitializerType.CustomInitializer, out var member))
					{
						argumentTargetMemberNames[i] = member.Name;

						foreach(var attribute in member.GetCustomAttributes(typeof(PropertyAttribute), false))
						{
							if(attribute is TooltipAttribute tooltip)
							{
								namespaces.Add(typeof(TooltipAttribute).Namespace);
								members += $"\t\t\t[Tooltip(\"{tooltip.tooltip}\")]\r\n";
								continue;
							}

							if(attribute is RangeAttribute range && (parameterType == typeof(int) || parameterType == typeof(float)))
							{
								namespaces.Add(typeof(RangeAttribute).Namespace);
								members += $"\t\t\t[Range({range.min}, {range.max})]\r\n";
								continue;
							}

							if(attribute is TextAreaAttribute textArea && parameterType == typeof(string))
							{
								namespaces.Add(typeof(TextAreaAttribute).Namespace);
								if(textArea.minLines == 3 && textArea.maxLines == 3)
								{
									members += "\t\t\t[TextArea]\r\n";
								}
								else
								{
									members += $"\t\t\t[TextArea({textArea.minLines}, {textArea.maxLines})]\r\n";
								}
							}
						}

						name = GetLabel(member.Name).Replace(" ", "");
					}
					else
					{
						name = GetLabel(NameOfWithoutIllegalCharacters(parameterType)).Replace(" ", "");
					}

					if(string.IsNullOrEmpty(name))
					{
						name = "argument" + i;
					}
					else if(reservedKeywords.Contains(name))
					{
						name = "@" + name;
					}

					while(!initializerFieldNames.Add(name))
					{
						if(int.TryParse(name.Substring(name.Length - 1), out int intSuffix))
						{
							name = name.Substring(0, name.Length - 1) + (intSuffix + 1);
						}
						else
						{
							name += "2";
						}
					}

					if(argumentTargetMemberNames[i] is null)
					{
						argumentTargetMemberNames[i] = name;
					}

					if(canSerializeAllArgumentsAsReferences)
					{
						members += $"\t\t\tpublic {parameterTypeName} {name} = default;\r\n";
					}
					else
					{
						namespaces.Add(typeof(SerializeField).Namespace);

						string fieldName = char.ToLowerInvariant(name[0]) + name.Substring(1);
						if(reservedKeywords.Contains(fieldName))
						{
							fieldName = "@" + name;
						}
						
						string propertyName = GetInitializerBasePropertyName(i, parameterCount);
						Type serializableType = GetAsUnitySerializableType(parameterType);
						if(serializableType != parameterType)
						{
							string serializableTypeName = NameOf(serializableType, namespaces);
							members += "\t\t[SerializeField]\r\n";
							members += $"\t\tprivate {serializableTypeName} {fieldName};\r\n";
							members += $"\t\tprotected override {parameterTypeName} {propertyName} {{ get => {fieldName}; set => {fieldName} = value as {serializableTypeName}; }}\r\n";
						}
						else
						{
							members += "\t\t[SerializeField]\r\n";
							members += $"\t\tprivate {parameterTypeName} {fieldName};\r\n";
							members += $"\t\tprotected override {parameterTypeName} {propertyName} {{ get => {fieldName}; set => {fieldName} = value; }}\r\n";
						}
					}
				}

				genericArguments = string.Join(", ", typeNames);
			}
			else
			{
				genericArguments = "FirstArgument, ..., LastArgument";
			}

			if(@namespace != null)
			{
				namespaces.Remove(@namespace);
			}

			var usings = namespaces.Count == 0 ? "" : string.Join("", namespaces.Select(n => "using " + n + ";\r\n"));

			string baseClassName, humanReadableType;
			string template = canSerializeAllArgumentsAsReferences ? InitializerTemplate : InitializerBaseTemplate;

			switch(initializerType)
			{
				case InitializerType.WrapperInitializer:
					baseClassName = canSerializeAllArgumentsAsReferences ? "WrapperInitializer" : "WrapperInitializerBase";
					string wrappedTypeAsString = clientTypeAsString.Replace("Component", "").Replace("Wrapper", "");

					if(clientType is not null)
					{
						foreach((Type wrapped, Type[] wrappers) in Find.typesToWrapperTypes)
						{
							if(Array.IndexOf(wrappers, clientType) != -1)
							{
								wrappedTypeAsString = TypeUtility.ToString(wrapped);
								break;
							}						
						}
					}

					genericArguments = clientTypeAsString + ", " + wrappedTypeAsString + ", " + genericArguments;
					clientTypeAsString = wrappedTypeAsString;
					humanReadableType = "wrapped object";
					template = canSerializeAllArgumentsAsReferences ? WrapperInitializerTemplate : WrapperInitializerBaseTemplate;

					string[] parameterNamesLowerCase = argumentTargetMemberNames.Select(name => char.ToLower(name[0]) + name.Substring(1)).ToArray();
					for(int i = 0; i < parameterCount; i++)
					{
						var parameterName = parameterNamesLowerCase[i];
						if(reservedKeywords.Contains(parameterName))
						{
							parameterNamesLowerCase[i] = "@" + parameterName;
						}
					}
					string parameterList = string.Join(", ", typeNames.Select((typeName, index) => typeName + " " + parameterNamesLowerCase[index]));
					string argumentNameList = string.Join(", ", parameterNamesLowerCase);

					string wrapperInitializerCode = string.Format(template, usings, @namespace, initializerTypeAsString, clientTypeAsString, humanReadableType, baseClassName, genericArguments, members, parameterList, argumentNameList);
					return WriteCodeToFile(initializerPath, wrapperInitializerCode);
				case InitializerType.PlainOldClassObjectInitializer:
					baseClassName = canSerializeAllArgumentsAsReferences ? "WrapperInitializer" : "WrapperInitializerBase";
					string wrapperTypeAsString;
					if(clientType != null && Find.typesToWrapperTypes.TryGetValue(clientType, out Type[] wrapperTypes))
					{
						wrapperTypeAsString = TypeUtility.ToString(wrapperTypes[0]);
					}
					else
					{
						wrapperTypeAsString = clientTypeAsString + "Component";
					}

					genericArguments = wrapperTypeAsString + ", " + clientTypeAsString + ", " + genericArguments;
					humanReadableType = "wrapped object";
					break;
				case InitializerType.StateMachineBehaviourInitializer:
					baseClassName = canSerializeAllArgumentsAsReferences ? "StateMachineBehaviourInitializer" : "StateMachineBehaviourInitializerBase";
					humanReadableType = "state machine behaviour";
					break;
				case InitializerType.ScriptableObjectInitializer:
					baseClassName = canSerializeAllArgumentsAsReferences ? "ScriptableObjectInitializer" : "ScriptableObjectInitializerBase";
					humanReadableType = "scriptable object";
					break;
				case InitializerType.Initializer:
					baseClassName = canSerializeAllArgumentsAsReferences ? "Initializer" : "InitializerBase";
					humanReadableType = "component";
					break;
				case InitializerType.VisualElementInitializer:
					string initTargetParameters = $"{clientTypeAsString} target";
					int index = 0;
					foreach(string fieldName in initializerFieldNames)
					{
						string variableName = ToCamelCase(fieldName);
						if(reservedKeywords.Contains(variableName))
						{
							variableName = "@" + variableName;
						}

						initTargetParameters += $", {typeNames[index]} {variableName}";
						index++;
					}

					baseClassName = canSerializeAllArgumentsAsReferences ? "VisualElementInitializer" : "VisualElementInitializerBase";
 					string initializerCode = string.Format(VisualElementInitializerTemplate, usings, @namespace, initializerTypeAsString, clientTypeAsString, baseClassName, genericArguments, members, initTargetParameters, "throw new NotImplementedException();");
					return WriteCodeToFile(initializerPath, initializerCode);
				default:
					initTargetParameters = $"{clientTypeAsString} target";
					string initTargetAssignArguments = "";
					index = 0;
					foreach(string fieldName in initializerFieldNames)
					{
						string variableName = ToCamelCase(fieldName);
						if(reservedKeywords.Contains(variableName))
						{
							variableName = "@" + variableName;
						}

						initTargetParameters += $", {typeNames[index]} {variableName}";
						initTargetAssignArguments += $"\t\t\ttarget.{argumentTargetMemberNames[index]} = {variableName};\r\n";
						index++;
					}

					baseClassName = canSerializeAllArgumentsAsReferences ? "CustomInitializer" : "CustomInitializerBase";
 					initializerCode = string.Format(CustomInitializerTemplate, usings, @namespace, initializerTypeAsString, clientTypeAsString, baseClassName, genericArguments, members, initTargetParameters, initTargetAssignArguments);
					return WriteCodeToFile(initializerPath, initializerCode);
			}

			string code = string.Format(template, usings, @namespace, initializerTypeAsString, clientTypeAsString, humanReadableType, baseClassName, genericArguments, members);
			return WriteCodeToFile(initializerPath, code);

			static string WriteCodeToFile(string initializerPath, string code)
			{
				if(File.Exists(initializerPath) && !EditorUtility.DisplayDialog("Overwrite Existing File?", $"The file '{Path.GetFileName(initializerPath)}' already exists at the path '{initializerPath}'.\n\nWould you like to overwrite it?", "Overwrite", "Cancel"))
				{
					return initializerPath;
				}

				File.WriteAllText(initializerPath, code);
				AssetDatabase.ImportAsset(initializerPath);
				return initializerPath;
			}

			static string ToCamelCase(string pascalCase)
			{
				var sb = new StringBuilder();
				string result = pascalCase;
				for(int i = 0; i < pascalCase.Length; i++)
				{
					if(!char.IsUpper(pascalCase[i]))
					{
						break;
					}

					sb.Append(char.ToLower(pascalCase[i]));

					result = result.Substring(0, i) + char.ToLower(result[i]) + result.Substring(i + 1);
				}

				return result;
			}

			static InitializerType GetInitializerType(Type clientType)
			{
				bool isWrapper = typeof(IWrapper).IsAssignableFrom(clientType);
				bool isPlainOldClassObject = !typeof(Object).IsAssignableFrom(clientType);

				if(isWrapper)
				{
					return InitializerType.WrapperInitializer;
				}
            
				if(isPlainOldClassObject)
				{
					#if DEV_MODE
					if(typeof(UnityEngine.UIElements.VisualElement).IsAssignableFrom(clientType))
					{
						return InitializerType.VisualElementInitializer;
					}
					#endif

					return InitializerType.PlainOldClassObjectInitializer;
				}
            
				if(typeof(StateMachineBehaviour).IsAssignableFrom(clientType))
				{
					return InitializerType.StateMachineBehaviourInitializer;
				}
			
				if(typeof(ScriptableObject).IsAssignableFrom(clientType))
				{
					return InitializerType.ScriptableObjectInitializer;
				}

				if(GetClientInitArgumentCount(clientType) > 0)
				{
					return InitializerType.Initializer;
				}
			
				return InitializerType.CustomInitializer;
			}
		}

		private static bool CanSerializeAsReference(Type type)
		{
			if(type is null)
			{
				return true;
			}

			if(type.IsGenericType)
			{
				if(type.IsGenericTypeDefinition)
				{
					return type == typeof(List<>);
				}

				return type.GetGenericTypeDefinition() == typeof(List<>);
			}

			// SerializeReference can only handle a subset of value types.
			// Primitives, enums and Unity's internal types and Vector3 all seem to work,
			// but custom struct types don't
			//if(type.IsValueType && !type.IsPrimitive && !type.IsEnum && (!(type.Namespace is string namespaceName) || !namespaceName.Contains("Unity")
			//{
			//    return false;
			//}

			return true;
		}

		private static Type[] GetInitParameters(Type clientType)
		{
			if(typeof(IWrapper).IsAssignableFrom(clientType))
			{
				foreach((Type wrappedType, Type[] wrapperTypes) in Find.typesToWrapperTypes)
				{
					if(Array.IndexOf(wrapperTypes, clientType) != -1)
					{
						clientType = wrappedType;
						break;
					}
				}
			}

			if(Find.typesToWrapperTypes.ContainsKey(clientType))
			{
				foreach(var constructor in clientType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
				{
					var parameters = constructor.GetParameters();
					if(parameters.Length > 0)
					{
						return parameters.Select(p => p.ParameterType).ToArray();
					}
				}
			}

			if(InitializableUtility.TryGetParameterTypes(clientType, out var parameterTypes))
			{
				return parameterTypes;
			}

			if(!typeof(Object).IsAssignableFrom(clientType))
			{
				foreach(var constructor in clientType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
				{
					var parameters = constructor.GetParameters();
					if(parameters.Length > 0)
					{
						return parameters.Select(p => p.ParameterType).ToArray();
					}
				}
			}

			foreach(var initializerType in GetInitializerTypes(clientType))
			{
				if(InitializerUtility.TryGetInitArgumentTypes(initializerType, out var initArgumentTypes))
				{
					return initArgumentTypes;
				}
			}

			var allProperties = clientType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
											.Where(p => p.CanRead && p.GetCustomAttribute<ObsoleteAttribute>() is null);

			List<Type> memberTypes;

			// With Object-derived types can't use constructor injection, so all target properties must have a setter.
			if(typeof(Object).IsAssignableFrom(clientType))
			{
				memberTypes = allProperties
								.Where(p => p.CanWrite)
								.Select(p => p.PropertyType)
								.ToList();
			}
			else
			{
				memberTypes = allProperties
								.Select(p => p.PropertyType)
								.ToList();
			}

			if(memberTypes.Count == 0)
			{
				memberTypes = clientType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
										.Where(f => !f.IsInitOnly && f.GetCustomAttribute<ObsoleteAttribute>() is null)
										.Select(f => f.FieldType)
										.ToList();
			}

			List<Type> objectAndInterfaceTypes = memberTypes.Where(t => typeof(Object).IsAssignableFrom(t) || t.IsInterface).ToList();
			if(objectAndInterfaceTypes.Count > 0)
			{
				memberTypes = objectAndInterfaceTypes;
			}

			return memberTypes.ToArray();
		}

		internal static string CreateWrapper(string assetPath, MonoScript script)
		{
			var @class = !script ? null : script.GetClass();
			string className = @class != null ? NameOfWithoutIllegalCharacters(@class) : Path.GetFileNameWithoutExtension(assetPath);
			var @namespace = @class?.Namespace ?? "MyNamespace";
			return CreateWrapper(assetPath, className, @namespace);
		}

		internal static string CreateWrapper(string forScriptAtPath, string wrappedObjectTypeName, string @namespace)
		{
			string path = forScriptAtPath;
			path = Path.GetDirectoryName(path);
			string filename = wrappedObjectTypeName + "Component.cs";
			path = Path.Combine(path, wrappedObjectTypeName + "Component.cs");

			if(File.Exists(path) && !EditorUtility.DisplayDialog("Overwrite Existing File?", $"The file '{filename}' already exists at the path '{path}'.\n\nWould you like to overwrite it?", "Overwrite", "Cancel"))
			{
				return path;
			}

			string inspectorTitle = ObjectNames.NicifyVariableName(wrappedObjectTypeName);
			string code = string.Format(WrapperTemplate, @namespace, wrappedObjectTypeName, inspectorTitle);
			File.WriteAllText(path, code);
			AssetDatabase.ImportAsset(path);

			return path;
		}

		private static Type GetAsUnitySerializableType(Type parameterType)
		{
			if(parameterType.IsGenericType)
			{
				var typeDefinition = parameterType.GetGenericTypeDefinition();
				if(typeDefinition == typeof(IEnumerable<>)
				|| typeDefinition == typeof(ICollection<>)
				|| typeDefinition == typeof(IReadOnlyList<>)
				|| typeDefinition == typeof(IList<>)
				|| typeDefinition == typeof(IReadOnlyCollection<>)
				|| typeDefinition == typeof(ICollection<>))
				{
					return parameterType.GetGenericArguments()[0].MakeArrayType();
				}
			}

			return parameterType;
		}

		private static string NameOf(Type type, HashSet<string> namespaces)
		{
			if(TryGetBuiltInTypeAlias(type, out string alias))
			{
				return alias;
			}

			RegisterNamespaces(type, namespaces);

			return TypeUtility.ToString(type, '\0', toStringCache);

			void RegisterNamespaces(Type type, HashSet<string> namespaces)
			{
				if(type.Namespace != null)
				{
					namespaces.Add(type.Namespace);

					if(type == typeof(Object))
					{
						namespaces.Add("Object = UnityEngine.Object");
					}
				}

				if(!type.IsGenericType)
				{
					return;
				}

				foreach(var genericType in type.GetGenericArguments())
				{
					if(!TryGetBuiltInTypeAlias(genericType, out _))
					{
						RegisterNamespaces(genericType, namespaces);
					}
				}
			}
		}

		private static bool TryGetBuiltInTypeAlias(Type type, out string alias)
		{
			if(type.IsEnum)
			{
				alias = null;
				return false;
			}

			switch(Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
					alias = "bool";
					return true;
				case TypeCode.Char:
					alias = "char";
					return true;
				case TypeCode.SByte:
					alias = "sbyte";
					return true;
				case TypeCode.Byte:
					alias = "byte";
					return true;
				case TypeCode.Int16:
					alias = "short";
					return true;
				case TypeCode.UInt16:
					alias = "ushort";
					return true;
				case TypeCode.Int32:
					alias = "int";
					return true;
				case TypeCode.UInt32:
					alias = "uint";
					return true;
				case TypeCode.Int64:
					alias = "long";
					return true;
				case TypeCode.UInt64:
					alias = "ulong";
					return true;
				case TypeCode.Single:
					alias = "float";
					return true;
				case TypeCode.Double:
					alias = "double";
					return true;
				case TypeCode.Decimal:
					alias = "decimal";
					return true;
				case TypeCode.String:
					alias = "string";
					return true;
			}

			if(type == typeof(object))
			{
				alias = "object";
				return true;
			}

			alias = null;
			return false;
		}

		private static string NameOfWithoutIllegalCharacters(Type type)
		{
			string name = type.Name;

			// Get name without `1, `2 etc. at the end
			if(type.IsGenericType)
			{
				var typeDefinition = type.GetGenericTypeDefinition();
				if(typeDefinition == typeof(List<>)
				|| typeDefinition == typeof(IEnumerable<>)
				|| typeDefinition == typeof(IReadOnlyList<>)
				|| typeDefinition == typeof(IReadOnlyCollection<>)
				|| typeDefinition == typeof(ICollection<>)
				|| typeDefinition == typeof(IList<>))
				{
					name = NameOfWithoutIllegalCharacters(type.GetGenericArguments()[0]) + "s";
				}
				else
				{
					// Get name without `1, `2 etc. at the end
					name = name.Substring(0, name.Length - 2);
				}
			}

			// Get name without [], [,] etc. at the end
			if(type.IsArray)
			{
				name = NameOfWithoutIllegalCharacters(type.GetElementType()) + "s";
			}

			if(type.IsNested)
			{
				int cutAtIndex = name.LastIndexOf('.');
				if(cutAtIndex != -1)
				{
					name = name.Substring(cutAtIndex + 1);
				}
			}

			return name;
		}

		private static string GetInitializerBasePropertyName(int index, int parameterCount) => parameterCount <= 1 ? "Argument" : initializerBasePropertyNames[index];

		private enum InitializerType
		{
			PlainOldClassObjectInitializer,
			WrapperInitializer,
			StateMachineBehaviourInitializer,
			VisualElementInitializer,
			ScriptableObjectInitializer,
			Initializer,
			CustomInitializer
		}
	}
}