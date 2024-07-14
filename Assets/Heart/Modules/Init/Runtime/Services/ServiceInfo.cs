using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sisus.Init
{
	/// <summary>
	/// Information about a service registered using the <see cref="ServiceAttribute"/>.
	/// </summary>
	internal sealed class ServiceInfo
	{
		[NotNull] public readonly Type classWithAttribute;

		/// <summary>
		/// Can in theory be null in rare instances, if the <see cref="ServiceAttribute"/> was attached to an initializer
		/// like a <see cref="CustomInitializer"/> where the generic type for the initialized object is abstract.
		/// <para>
		/// Also can be a generic type definition.
		/// </para>
		/// </summary>
		[MaybeNull] public readonly Type concreteType;

		[NotNull] public readonly Type[] definingTypes;

		[NotNull] public readonly ServiceAttribute[] attributes;

		/// <summary>
		/// Returns <see cref="concreteType"/> if it's not <see langword="null"/>,
		/// otherwise returns first element from <see cref="definingTypes"/>.
		/// <para>
		/// Can be an abstract or a generic type definition.
		/// </para>
		/// </summary>
		[NotNull] public Type ConcreteOrDefiningType => concreteType ?? definingTypes.FirstOrDefault();

		public bool LazyInit
		{
			get
			{
				foreach(var attribute in attributes)
				{
					if(attribute.LazyInit)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool LoadAsync
		{
			get
			{
				foreach(var attribute in attributes)
				{
					if(attribute.LoadAsync)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool FindFromScene
		{
			get
			{
				foreach(var attribute in attributes)
				{
					if(attribute.FindFromScene)
					{
						return true;
					}
				}

				return false;
			}
		}

		public string ResourcePath
		{
			get
			{

				foreach(var attribute in attributes)
				{
					if(attribute.ResourcePath is string resourcePath)
					{
						return resourcePath;
					}
				}

				return null;
			}
		}

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		public string AddressableKey
		{
			get
			{

				foreach(var attribute in attributes)
				{
					if(attribute.AddressableKey is string addressableKey)
					{
						return addressableKey;
					}
				}

				return null;
			}
		}
		#endif

		public ServiceInfo([DisallowNull] Type classWithAttribute, [AllowNull] Type concreteType, [DisallowNull] Type[] definingTypes, [DisallowNull] ServiceAttribute[] attributes)
		{
			this.classWithAttribute = classWithAttribute;
			this.concreteType = concreteType;
			this.definingTypes = definingTypes;
			this.attributes = attributes;
		}

		public bool HasDefiningType(Type type) => Array.IndexOf(definingTypes, type) != -1;
	}
}