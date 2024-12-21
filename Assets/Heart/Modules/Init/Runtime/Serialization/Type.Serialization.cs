#pragma warning disable CS0649

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Serialization
{
	/// <summary>
	/// A wrapper for a <see cref="System.Type"/> value that makes it serializable by Unity's serialization system.
	/// <para>
	/// Used to serialize <see cref="System.Type"/> values by <see cref="Internal.ServiceDefinition"/>.
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class _Type : ISerializationCallbackReceiver, IValueProvider<Type>
	{
		private static readonly HashSet<string> typeNotFoundWarningHandledFor = new HashSet<string>();

		[SerializeField]
		private string scriptAssetGuid;

		[SerializeField]
		private string TypeNameAndAssembly;

		private Type value;

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private bool shouldLogTypeNotFoundDelayed;
		#endif

		#if UNITY_EDITOR
		private bool isDirty;
		#endif

		/// <summary>
		/// Gets or sets type of class reference.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="value"/> does not have a full name.
		/// </exception>
		[MaybeNull, AllowNull]
		public Type Value
		{
			get => value;

			set
			{
				#if UNITY_EDITOR
				GuardAgainstUnserializable(value);

				if(this.value == value)
				{
					return;
				}
				#endif

				this.value = value;

				#if UNITY_EDITOR
				TypeNameAndAssembly = GetTypeNameAndAssembly(value);
				isDirty = true;
				#endif
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="_Type"/> class.
		/// </summary>
		public _Type() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="_Type"/> class.
		/// </summary>
		/// <param name="assemblyQualifiedTypeName"> Assembly qualified name of the wrapped type. </param>
		/// <param name="guid"> (Optional:) The GUID of the script asset in which the wrapped type is defined. </param>
		public _Type([AllowNull] string assemblyQualifiedTypeName, string guid = null)
		{
			Value = !string.IsNullOrEmpty(assemblyQualifiedTypeName) ? Type.GetType(assemblyQualifiedTypeName) : null;
			scriptAssetGuid = guid;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="_Type"/> class.
		/// </summary>
		/// <param name="value"> the wrapped type. </param>
		/// <param name="guid"> (Optional:) The GUID of the script asset in which the wrapped type is defined. </param>
		public _Type([AllowNull] Type value, string guid = null)
		{
			Value = value;
			scriptAssetGuid = guid;
		}

		public static implicit operator Type(_Type typeReference) => typeReference?.Value;

		public static implicit operator _Type(Type type) => new _Type(type);

		public override string ToString() => Value?.FullName is null ? "None" : Value.Name;

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if(string.IsNullOrEmpty(TypeNameAndAssembly))
			{
				value = null;
			}
			else
			{
				value = Type.GetType(TypeNameAndAssembly);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				// If GUID is not empty, there is still hope the type will be found in the TryUpdatingTypeUsingGUID method.
				if(value == null && scriptAssetGuid.Length == 0)
				{
					shouldLogTypeNotFoundDelayed = true;

					#if UNITY_EDITOR
					UnityEditor.EditorApplication.delayCall += HandleLogTypeNotFound;
					#else
					Internal.Updater.InvokeAtEndOfFrame(HandleLogTypeNotFound);
					#endif
				}
				#endif
			}
			
			#if UNITY_EDITOR
			if(value is null && !string.IsNullOrEmpty(scriptAssetGuid))
			{
				UnityEditor.EditorApplication.delayCall += TryUpdatingTypeUsingGUID;
			}
			#endif
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			#if UNITY_EDITOR
			if(isDirty)
			{
				SetClassGuidIfExists(Value);
				isDirty = false;
			}

			UnsubscribeFromDelayCall();
			#endif
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private void HandleLogTypeNotFound()
		{
			if(shouldLogTypeNotFoundDelayed)
			{
				shouldLogTypeNotFoundDelayed = false;
				LogTypeNotFound();
			}
		}
		#endif

		private void LogTypeNotFound()
		{
			if(typeNotFoundWarningHandledFor.Add(TypeNameAndAssembly))
			{
				Debug.LogWarning($"'{TypeNameAndAssembly}' was referenced but such type was not found.");
			}
		}

		public bool Equals(_Type other)
		{
			if(ReferenceEquals(other, null))
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return scriptAssetGuid?.Length != 0 && other.scriptAssetGuid?.Length != 0 && (Value == null || other.Value == null) ? scriptAssetGuid == other.scriptAssetGuid : Value == other.Value;
		}

		public static bool operator ==(_Type a, _Type b) => a?.Equals(b) ?? b is null;

		public static bool operator !=(_Type a, _Type b) => !(a == b);
		public static bool operator ==(_Type a, Type b) => a?.Value?.Equals(b) ?? b is null;
		public static bool operator !=(_Type a, Type b) => !(a == b);

		public override bool Equals(object obj) => Equals(obj as _Type);

		public override int GetHashCode() => value is null ? 0 : value.GetHashCode();

		#if UNITY_EDITOR
		private void SetClassGuidIfExists(Type type)
		{
			// common case optimization
			if(TypeCannotHaveGUID())
			{
				scriptAssetGuid = "";
				return;
			}

			try
			{
				scriptAssetGuid = GetGuidOfScriptAssetForClass(type);
			}
			catch(UnityException) // thrown on assembly recompiling if field initialization is used on field.
			{
				scriptAssetGuid = "";
			}
		}

		private bool TypeCannotHaveGUID()
		{
			if(string.IsNullOrEmpty(TypeNameAndAssembly))
				return false;

			int charAfterWhiteSpace = TypeNameAndAssembly.IndexOf(' ') + 1;

			string assemblyName = TypeNameAndAssembly.Substring(
				charAfterWhiteSpace,
				TypeNameAndAssembly.Length - charAfterWhiteSpace);

			return assemblyName == "mscorlib"
					|| assemblyName == "netstandard"
					|| assemblyName.StartsWith("System.")
					|| assemblyName.StartsWith("Microsoft.")
					|| assemblyName.StartsWith("Unity.")
					|| assemblyName.StartsWith("UnityEngine.")
					|| assemblyName.StartsWith("UnityEditor.");
		}

		private void UnsubscribeFromDelayCall()
		{
			UnityEditor.EditorApplication.delayCall -= TryUpdatingTypeUsingGUID;
			UnityEditor.EditorApplication.delayCall -= HandleLogTypeNotFound;
		}

		private static string GetGuidOfScriptAssetForClass(Type type)
		{
			if(type == null || type.FullName == null)
			{
				return "";
			}

			var script = Find.Script(type);
			if(!script)
			{
				return "";
			}

			string path = UnityEditor.AssetDatabase.GetAssetPath(script);
			var guid = UnityEditor.AssetDatabase.GUIDFromAssetPath(path);
			return guid.ToString();
		}

		private void TryUpdatingTypeUsingGUID()
		{
			if(value != null || string.IsNullOrEmpty(scriptAssetGuid))
			{
				return;
			}

			string scriptAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(scriptAssetGuid);
			var scriptAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(scriptAssetPath);

			if(scriptAsset == null)
			{
				LogTypeNotFound();
				scriptAssetGuid = "";
				return;
			}

			var typeFromScriptAsset = scriptAsset.GetClass();

			if(typeFromScriptAsset == null)
			{
				LogTypeNotFound();
				scriptAssetGuid = "";
				return;
			}

			value = typeFromScriptAsset;
			string previousTypeName = TypeNameAndAssembly;
			TypeNameAndAssembly = GetTypeNameAndAssembly(value);

			Debug.Log($"Type reference has been automatically updated from '{previousTypeName}' to '{TypeNameAndAssembly}'.");
		}

		private static string GetTypeNameAndAssembly([MaybeNull] Type type)
		{
			GuardAgainstUnserializable(type);
			return type is not null ? $"{type.FullName}, {type.Assembly.GetName().Name}" : "";
		}

		private static void GuardAgainstUnserializable([MaybeNull] Type type)
		{
			if(type != null && type.FullName == null)
			{
				throw new ArgumentException($"'{type}' does not have full name.", nameof(type));
			}
		}
		#endif
	}
}