using UnityEditor;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	using static ScriptGenerator;

	internal static class MonoScriptMenuItems
	{
		[MenuItem("CONTEXT/MonoScript/Generate Initializer")]
		private static void GenerateInitializerFromScript(MenuCommand command)
		{
			var script = command.context as MonoScript;
			CreateInitializer(script);
		}

		[MenuItem("CONTEXT/MonoScript/Generate Initializer", validate = true)]
		private static bool GenerateInitializerFromScriptEnabled(MenuCommand command)
		{
			return GenerateInitializerEnabled(command.context as MonoScript);
		}


		[MenuItem("CONTEXT/AssetImporter/Generate Initializer")]
		private static void GenerateInitializerFromScriptImporter(MenuCommand command)
		{
			var monoScriptImporter = command.context as AssetImporter;
			string assetPath = monoScriptImporter.assetPath;
			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
			CreateInitializer(script);
		}

		[MenuItem("CONTEXT/AssetImporter/Generate Initializer", validate = true)]
		private static bool GenerateInitializerFromScriptImporterEnabled(MenuCommand command)
		{
			var assetImporter = command.context as AssetImporter;
			string assetPath = assetImporter.assetPath;
			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
			return GenerateInitializerEnabled(script);
		}


		[MenuItem("CONTEXT/MonoScript/Generate Wrapper")]
		private static void GenerateWrapperFromScript(MenuCommand command)
		{
			var script = command.context as MonoScript;
			CreateWrapper(AssetDatabase.GetAssetPath(script), script);
		}

		[MenuItem("CONTEXT/MonoScript/Generate Wrapper", validate = true)]
		private static bool GenerateWrapperFromScriptEnabled(MenuCommand command)
		{
			var script = command.context as MonoScript;
			return GenerateWrapperEnabled(script);
		}


		[MenuItem("CONTEXT/AssetImporter/Generate Wrapper")]
		private static void GenerateWrapperFromScriptImporter(MenuCommand command)
		{
			var monoScriptImporter = command.context as AssetImporter;
			string assetPath = monoScriptImporter.assetPath;
			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
			CreateWrapper(assetPath, script);
		}

		[MenuItem("CONTEXT/AssetImporter/Generate Wrapper", validate = true)]
		private static bool GenerateWrapperFromScriptImporterEnabled(MenuCommand command)
		{
			var assetImporter = command.context as AssetImporter;
			string assetPath = assetImporter.assetPath;
			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
			return GenerateWrapperEnabled(script);
		}

		private static bool GenerateInitializerEnabled(MonoScript script)
		{
			if(script == null)
			{
				return false;
			}

			var type = script.GetClass();
			if(type == null)
			{
				return false;
			}

			foreach(var @interface in type.GetInterfaces())
			{
				if(!@interface.IsGenericType)
				{
					continue;
				}

				var genericTypeDefinition = @interface.GetGenericTypeDefinition();
				if(genericTypeDefinition == typeof(IInitializable<>)
					|| genericTypeDefinition == typeof(IInitializable<,>)
					|| genericTypeDefinition == typeof(IInitializable<,,>)
					|| genericTypeDefinition == typeof(IInitializable<,,,>)
					|| genericTypeDefinition == typeof(IInitializable<,,,,>)
					|| genericTypeDefinition == typeof(IInitializable<,,,,,>))
				{
					return true;
				}
			}

			if(Find.typesToWrapperTypes.TryGetValue(type, out _))
			{
				return true;
			}

			return false;
		}

		private static bool GenerateWrapperEnabled(MonoScript script)
		{
			if(script == null)
			{
				return false;
			}

			var type = script.GetClass();
			return type == null || (!typeof(Object).IsAssignableFrom(type) && !type.IsEnum && type.IsClass);
		}
	}
}