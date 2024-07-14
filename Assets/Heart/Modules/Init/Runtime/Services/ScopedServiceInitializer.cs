using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Class that is responsible for initializing a service registered via a ServiceTag
	/// or a Services component with all its dependencies.
	/// </summary>
	internal abstract class ScopedServiceInitializer
	{
		private static readonly Dictionary<Type, ScopedServiceInitializer> initializers = new(16);

		public static void Register([DisallowNull] Type clientType, [DisallowNull] ScopedServiceInitializer initializer) => initializers[clientType] = initializer;

		/// <summary>
		/// The service defined by ServiceTag or Services component should be passed here during the Awake event function,
		/// to have it automatically receive other services it depends on if it implements an <see cref="IInitializable{}"/> interface.
		/// </summary>
		/// <param name="service"> The service to initialize. </param>
		public static void Init([DisallowNull] Component service)
		{
			if(initializers.TryGetValue(service.GetType(), out var initializer))
			{
				initializer.InitTarget(service);
			}
		}

		/// <summary>
		/// Initializes the object that implements an <see cref="IInitializable{}"/> interface.
		/// </summary>
		/// <param name="client"> The object to initializer. </param>
		public abstract void InitTarget(Component client);
	}

	internal sealed class ScopedServiceInitializer<TArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TArgument argument;

		// ServiceTag or Services component. Maybe service with LazyInit?
		private readonly IValueByTypeProvider argumentProvider;

		public ScopedServiceInitializer(TArgument argument, IValueByTypeProvider argumentProvider)
		{
			this.argument = argument;
			this.argumentProvider = argumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TArgument>)client).Init(argument ?? (argumentProvider.TryGetFor(client, out TArgument arg) ? arg : default));
	}

	internal sealed class ScopedServiceInitializer<TFirstArgument, TSecondArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TFirstArgument firstArgument;
		private readonly TSecondArgument secondArgument;

		// ServiceTag or Services component:
		private readonly IValueByTypeProvider firstArgumentProvider;
		private readonly IValueByTypeProvider secondArgumentProvider;

		public ScopedServiceInitializer(TFirstArgument firstArgument, TSecondArgument secondArgument,
										   IValueByTypeProvider firstArgumentProvider, IValueByTypeProvider secondArgumentProvider)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.firstArgumentProvider = firstArgumentProvider;
			this.secondArgumentProvider = secondArgumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TFirstArgument, TSecondArgument>)client).Init
		(
			firstArgument ?? (firstArgumentProvider.TryGetFor(client, out TFirstArgument first) ? first : default),
			secondArgument ?? (secondArgumentProvider.TryGetFor(client, out TSecondArgument second) ? second : default)
		);
	}

	internal sealed class ScopedServiceInitializer<TFirstArgument, TSecondArgument, TThirdArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TFirstArgument firstArgument;
		private readonly TSecondArgument secondArgument;
		private readonly TThirdArgument thirdArgument;

		// ServiceTag or Services component:
		private readonly IValueByTypeProvider firstArgumentProvider;
		private readonly IValueByTypeProvider secondArgumentProvider;
		private readonly IValueByTypeProvider thirdArgumentProvider;

		public ScopedServiceInitializer(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
										   IValueByTypeProvider firstArgumentProvider, IValueByTypeProvider secondArgumentProvider, IValueByTypeProvider thirdArgumentProvider)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;

			this.firstArgumentProvider = firstArgumentProvider;
			this.secondArgumentProvider = secondArgumentProvider;
			this.thirdArgumentProvider = thirdArgumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>)client).Init
		(
			firstArgument ?? (firstArgumentProvider.TryGetFor(client, out TFirstArgument first) ? first : default),
			secondArgument ?? (secondArgumentProvider.TryGetFor(client, out TSecondArgument second) ? second : default),
			thirdArgument ?? (thirdArgumentProvider.TryGetFor(client, out TThirdArgument third) ? third : default)
		);
	}

	internal sealed class ScopedServiceInitializer<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TFirstArgument firstArgument;
		private readonly TSecondArgument secondArgument;
		private readonly TThirdArgument thirdArgument;
		private readonly TFourthArgument fourthArgument;

		// ServiceTag or Services component:
		private readonly IValueByTypeProvider firstArgumentProvider;
		private readonly IValueByTypeProvider secondArgumentProvider;
		private readonly IValueByTypeProvider thirdArgumentProvider;
		private readonly IValueByTypeProvider fourthArgumentProvider;

		public ScopedServiceInitializer(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
										   IValueByTypeProvider firstArgumentProvider, IValueByTypeProvider secondArgumentProvider,
										   IValueByTypeProvider thirdArgumentProvider, IValueByTypeProvider fourthArgumentProvider)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;

			this.firstArgumentProvider = firstArgumentProvider;
			this.secondArgumentProvider = secondArgumentProvider;
			this.thirdArgumentProvider = thirdArgumentProvider;
			this.fourthArgumentProvider = fourthArgumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>)client).Init
		(
			firstArgument ?? (firstArgumentProvider.TryGetFor(client, out TFirstArgument first) ? first : default),
			secondArgument ?? (secondArgumentProvider.TryGetFor(client, out TSecondArgument second) ? second : default),
			thirdArgument ?? (thirdArgumentProvider.TryGetFor(client, out TThirdArgument third) ? third : default),
			fourthArgument ?? (fourthArgumentProvider.TryGetFor(client, out TFourthArgument fourth) ? fourth : default)
		);
	}

	internal sealed class ScopedServiceInitializer<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TFirstArgument firstArgument;
		private readonly TSecondArgument secondArgument;
		private readonly TThirdArgument thirdArgument;
		private readonly TFourthArgument fourthArgument;
		private readonly TFifthArgument fifthArgument;

		// ServiceTag or Services component:
		private readonly IValueByTypeProvider firstArgumentProvider;
		private readonly IValueByTypeProvider secondArgumentProvider;
		private readonly IValueByTypeProvider thirdArgumentProvider;
		private readonly IValueByTypeProvider fourthArgumentProvider;
		private readonly IValueByTypeProvider fifthArgumentProvider;

		public ScopedServiceInitializer(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
										   TFourthArgument fourthArgument, TFifthArgument fifthArgument,
										   IValueByTypeProvider firstArgumentProvider, IValueByTypeProvider secondArgumentProvider, IValueByTypeProvider thirdArgumentProvider,
										   IValueByTypeProvider fourthArgumentProvider, IValueByTypeProvider fifthArgumentProvider)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;

			this.firstArgumentProvider = firstArgumentProvider;
			this.secondArgumentProvider = secondArgumentProvider;
			this.thirdArgumentProvider = thirdArgumentProvider;
			this.fourthArgumentProvider = fourthArgumentProvider;
			this.fifthArgumentProvider = fifthArgumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>)client).Init
		(
			firstArgument ?? (firstArgumentProvider.TryGetFor(client, out TFirstArgument first) ? first : default),
			secondArgument ?? (secondArgumentProvider.TryGetFor(client, out TSecondArgument second) ? second : default),
			thirdArgument ?? (thirdArgumentProvider.TryGetFor(client, out TThirdArgument third) ? third : default),
			fourthArgument ?? (fourthArgumentProvider.TryGetFor(client, out TFourthArgument fourth) ? fourth : default),
			fifthArgument ?? (fifthArgumentProvider.TryGetFor(client, out TFifthArgument fifth) ? fifth : default)
		);
	}

	internal sealed class ScopedServiceInitializer<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : ScopedServiceInitializer
	{
		// Direct reference to service registered via ServiceAttribute:
		private readonly TFirstArgument firstArgument;
		private readonly TSecondArgument secondArgument;
		private readonly TThirdArgument thirdArgument;
		private readonly TFourthArgument fourthArgument;
		private readonly TFifthArgument fifthArgument;
		private readonly TSixthArgument sixthArgument;

		// ServiceTag or Services component:
		private readonly IValueByTypeProvider firstArgumentProvider;
		private readonly IValueByTypeProvider secondArgumentProvider;
		private readonly IValueByTypeProvider thirdArgumentProvider;
		private readonly IValueByTypeProvider fourthArgumentProvider;
		private readonly IValueByTypeProvider fifthArgumentProvider;
		private readonly IValueByTypeProvider sixthArgumentProvider;

		public ScopedServiceInitializer(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
										   TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
										   IValueByTypeProvider firstArgumentProvider, IValueByTypeProvider secondArgumentProvider, IValueByTypeProvider thirdArgumentProvider,
										   IValueByTypeProvider fourthArgumentProvider, IValueByTypeProvider fifthArgumentProvider, IValueByTypeProvider sixthArgumentProvider)
		{
			this.firstArgument = firstArgument;
			this.secondArgument = secondArgument;
			this.thirdArgument = thirdArgument;
			this.fourthArgument = fourthArgument;
			this.fifthArgument = fifthArgument;
			this.sixthArgument = sixthArgument;

			this.firstArgumentProvider = firstArgumentProvider;
			this.secondArgumentProvider = secondArgumentProvider;
			this.thirdArgumentProvider = thirdArgumentProvider;
			this.fourthArgumentProvider = fourthArgumentProvider;
			this.fifthArgumentProvider = fifthArgumentProvider;
			this.sixthArgumentProvider = sixthArgumentProvider;
		}

		public override void InitTarget(Component client) => ((IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>)client).Init
		(
			firstArgument ?? (firstArgumentProvider.TryGetFor(client, out TFirstArgument first) ? first : default),
			secondArgument ?? (secondArgumentProvider.TryGetFor(client, out TSecondArgument second) ? second : default),
			thirdArgument ?? (thirdArgumentProvider.TryGetFor(client, out TThirdArgument third) ? third : default),
			fourthArgument ?? (fourthArgumentProvider.TryGetFor(client, out TFourthArgument fourth) ? fourth : default),
			fifthArgument ?? (fifthArgumentProvider.TryGetFor(client, out TFifthArgument fifth) ? fifth : default),
			sixthArgument ?? (sixthArgumentProvider.TryGetFor(client, out TSixthArgument sixth) ? sixth : default)
		);
	}
}