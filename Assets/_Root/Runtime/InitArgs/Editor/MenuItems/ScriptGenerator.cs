using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Init.Internal;
using UnityEditor;
using UnityEngine;
using static Pancake.Init.EditorOnly.InitializerEditorUtility;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	internal static class ScriptGenerator
    {
        private const string InitializerTemplate =
            "{0}\r\n" +
            "namespace {1}\r\n" +
            "{{\r\n" +
            "\t/// <summary>\r\n" +
            "\t/// <see cref=\"Initializer{{,}}\"/> for the <see cref=\"{2}\"/> component.\r\n" +
            "\t/// </summary>\r\n" +
            "\tpublic sealed class {2}Initializer : {3}<{2}, {4}>\r\n" +
            "\t{{\r\n" +
            "\t\t#if UNITY_EDITOR\r\n" +
			"\t\t#pragma warning disable CS0649\r\n" +
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
            "{5}" +
            "\t\t}}\r\n" +
			"\t\t#pragma warning restore CS0649\r\n" +
            "\t\t#endif\r\n" +
            "\t}}\r\n" +
            "}}\r\n";

		private const string InitializerBaseTemplate =
            "{0}\r\n" +
            "namespace {1}\r\n" +
            "{{\r\n" +
            "\t/// <summary>\r\n" +
            "\t/// <see cref=\"Initializer{{,}}\"/> for the <see cref=\"{2}\"/> component.\r\n" +
            "\t/// </summary>\r\n" +
            "\tpublic sealed class {2}Initializer : {3}<{2}, {4}>\r\n" +
            "\t{{\r\n" +
			"\t\t#pragma warning disable CS0649\r\n" +
            "{5}" +
			"\t\t#pragma warning restore CS0649\r\n" +
            "\t}}\r\n" +
            "}}\r\n";

        private const string WrapperTemplate =
            "using UnityEngine;\r\n" +
            "using Pancake.Init;\r\n" +
            "\r\n" +
            "namespace {0}\r\n" +
            "{{\r\n" +
            "\t/// <summary>\r\n" +
            "\t/// <see cref=\"Wrapper{{}}\"/> for the <see cref=\"{1}\"/> component.\r\n" +
            "\t/// </summary>\r\n" +
            "\t[AddComponentMenu(\"Wrapper/{1}\")]\r\n" +
            "\tpublic sealed class {1}Component : Wrapper<{1}> {{ }}\r\n" +
            "}}\r\n";

        private static readonly Dictionary<char, Dictionary<Type, string>> toStringCache = new Dictionary<char, Dictionary<Type, string>>(1)
		{
			{ '\0', new Dictionary<Type, string>(4096) {
				{ typeof(int), "int" }, { typeof(uint), "unit" },
				{ typeof(float), "float" }, { typeof(double), "double" },
				{ typeof(bool), "bool" }, { typeof(string), "string" },
				{ typeof(short), "short" }, { typeof(ushort), "ushort" },
				{ typeof(byte), "byte" },{ typeof(sbyte), "sbyte" },
				{ typeof(long), "long" }, { typeof(ulong), "ulong" },
				{ typeof(object), "object" }, { typeof(decimal), "decimal" }
			} }
		};

		private static readonly string[] initializerBasePropertyNames = new string[]
		{
			"FirstArgument",
			"SecondArgument",
			"ThirdArgument",
			"FourthArgument",
			"FifthArgument",
			"SixthArgument",
			"SeventhArgument",
			"EigthArgument",
			"NinthArgument"
		};

        internal static string CreateInitializer(MonoScript script) => CreateInitializer(AssetDatabase.GetAssetPath(script), script);

        internal static string CreateInitializer(string forScriptAtPath, MonoScript script)
        {
            var @class = script == null ? null : script.GetClass();
            string className = @class != null ? NameOfWithoutGenericTypes(@class) : Path.GetFileNameWithoutExtension(forScriptAtPath);
            var @namespace = @class?.Namespace ?? "MyNamespace";
            Type[] interfaces = @class?.GetInterfaces() ?? Type.EmptyTypes;
            return CreateInitializer(forScriptAtPath, @class, className, @namespace, interfaces);
        }

		private static string CreateInitializer(string forScriptAtPath, Type @class, string className, string @namespace, Type[] interfaces)
		{
			var parameters = GetInitParameters(interfaces);
			string parameterNames;
			string members = "";
			int parameterCount = parameters.Length;
			HashSet<string> namespaces = new HashSet<string>() { "Pancake.Init" };

            bool canSerializeAllArgumentsAsReferences = true;

			if(parameterCount > 0)
			{
				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parameters[i];
                    if(!CanSerializeAsReference(parameterType))
					{
                        canSerializeAllArgumentsAsReferences = false;
					}
				}

				HashSet<string> fieldNames = new HashSet<string>();
				string[] typeNames = new string[parameterCount];
				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parameters[i];
					string parameterTypeName = NameOf(parameterType, namespaces);
					typeNames[i] = parameterTypeName;

					string name;
					if(TryGetArgumentTargetMember(@class, parameterType, out var member))
					{
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
									members += $"\t\t\t[TextArea]\r\n";
								}
								else
								{
									members += $"\t\t\t[TextArea({textArea.minLines}, {textArea.maxLines})]\r\n";
								}
							}
						}

						name = member.Name;

						if(name.StartsWith("<"))
						{
							int propertyNameEnd = name.IndexOf('>');
							name = name.Substring(1, propertyNameEnd - 1);
						}
					}
					else
					{
						name = GetLabel(NameOfWithoutGenericTypes(parameterType)).text.Replace(" ", "");
					}

					if(string.IsNullOrEmpty(name))
					{
						name = "argument" + i;
					}

					while(!fieldNames.Add(name))
					{
						if(int.TryParse(name.Substring(name.Length - 1), out int intSuffix))
						{
							name = name.Substring(0, name.Length - 1) + (intSuffix + 1).ToString();
						}
						else
						{
							name += "2";
						}
					}

					if(canSerializeAllArgumentsAsReferences)
					{
						members += $"\t\t\tpublic {parameterTypeName} {name};\r\n";
					}
					else
					{
						namespaces.Add(typeof(SerializeField).Namespace);

						string fieldName = char.ToLowerInvariant(name[0]) + name.Substring(1);
						string propertyName = GetInitializerBasePropertyName(i, parameterCount);

						members += $"\t\t[SerializeField]\r\n";
						members += $"\t\tprivate {parameterTypeName} {fieldName};\r\n";
						members += $"\t\tprotected override {parameterTypeName} {propertyName} {{ get => {fieldName}; set => {fieldName} = value; }}\r\n";
					}
				}

				parameterNames = string.Join(", ", typeNames);
			}
			else
			{
				parameterNames = "FirstArgument, ..., LastArgument";
			}

			if(@namespace != null)
			{
				namespaces.Remove(@namespace);
			}

			var usings = namespaces.Count == 0 ? "" : string.Join("", namespaces.Select(n => "using " + n + ";\r\n"));
            bool isWrapper = typeof(IWrapper).IsAssignableFrom(@class);
            bool isPlainOldClassObject = !typeof(Object).IsAssignableFrom(@class);
            string baseClassName;
            if(isWrapper)
			{
                baseClassName = canSerializeAllArgumentsAsReferences ? "WrapperInitializer" : "WrapperInitializerBase";
                parameterNames = className + ", " + parameterNames;
            }
            else if(isPlainOldClassObject)
            {
				baseClassName = canSerializeAllArgumentsAsReferences ? "WrapperInitializer" : "WrapperInitializerBase";
                parameterNames = className + "Component, " + parameterNames;
            }
            else if(typeof(StateMachineBehaviour).IsAssignableFrom(@class))
			{
				baseClassName = canSerializeAllArgumentsAsReferences ? "StateMachineBehaviourInitializer" : "StateMachineBehaviourInitializerBase";
			}
			else
			{
                baseClassName = canSerializeAllArgumentsAsReferences ? "Initializer" : "InitializerBase";
            }

			string template = canSerializeAllArgumentsAsReferences ? InitializerTemplate : InitializerBaseTemplate;
			string code = string.Format(template, usings, @namespace, className, baseClassName, parameterNames, members);
			string path = forScriptAtPath;
			path = Path.GetDirectoryName(path);
			string filename = className + "Initializer.cs";
			path = Path.Combine(path, filename);

			if(File.Exists(path) && !EditorUtility.DisplayDialog("Overwrite Existing File?", $"The file '{filename}' already exists at the path '{path}'.\n\nWould you like to overwrite it?", "Overwrite", "Cancel"))
			{
				return path;
			}

			File.WriteAllText(path, code);
			AssetDatabase.ImportAsset(path);
			return path;
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

		private static Type[] GetInitParameters(Type[] interfaces)
        {
            foreach(var @interface in interfaces)
            {
                var args = GetInitParameters(@interface);
                if(args.Length > 0)
                {
                    return args;
                }
            }

            return Type.EmptyTypes;
        }

        private static Type[] GetInitParameters(Type @interface)
        {
            if(!@interface.IsGenericType || @interface.IsGenericTypeDefinition)
            {
                return Type.EmptyTypes;
            }

            var definition = @interface.GetGenericTypeDefinition();
            if(definition == typeof(IArgs<,,,,>)
            || definition == typeof(IArgs<,,,>)
            || definition == typeof(IArgs<,,>)
            || definition == typeof(IArgs<,>)
            || definition == typeof(IArgs<>))
            {
                return @interface.GetGenericArguments();
            }

            return Type.EmptyTypes;
        }


        internal static string CreateWrapper(string assetPath, MonoScript script)
        {
            var @class = script == null ? null : script.GetClass();
            string className = @class != null ? NameOfWithoutGenericTypes(@class) : Path.GetFileNameWithoutExtension(assetPath);
            var @namespace = @class?.Namespace ?? "MyNamespace";
            return CreateWrapper(assetPath, className, @namespace);
        }

        internal static string CreateWrapper(string forScriptAtPath, string className, string @namespace)
        {
            string code = string.Format(WrapperTemplate, @namespace, className);
            string path = forScriptAtPath;
            path = Path.GetDirectoryName(path);
            string filename = className + "Component.cs";
            path = Path.Combine(path, className + "Component.cs");

            if(File.Exists(path) && !EditorUtility.DisplayDialog("Overwrite Existing File?", $"The file '{filename}' already exists at the path '{path}'.\n\nWould you like to overwrite it?", "Overwrite", "Cancel"))
			{
                return path;
			}

            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);

            return path;
        }


        private static string NameOf(Type type, HashSet<string> namespaces)
        {
            if(TryGetBuiltInTypeAlias(type, out string alias))
			{
                return alias;
			}

            void RegisterNamespaces(Type type, HashSet<string> namespaces)
			{
                if(type.Namespace != null)
                {
                    namespaces.Add(type.Namespace);
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

            RegisterNamespaces(type, namespaces);

            return TypeUtility.ToString(type, '\0', toStringCache);
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

        private static string NameOfWithoutGenericTypes(Type type)
        {
            if(!type.IsGenericType)
			{
                return type.Name;
			}

            // Get name without `1, `2 etc. at the end
            string typeName = type.Name;
            return type.Name.Substring(0, typeName.Length - 2);
		}

		private static string GetInitializerBasePropertyName(int index, int parameterCount) => parameterCount <= 1 ? "Argument" : initializerBasePropertyNames[index];
    }
}