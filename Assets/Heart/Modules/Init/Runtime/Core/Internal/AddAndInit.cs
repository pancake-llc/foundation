using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Utility class to help with getting and adding components to GameObjects when using new <see cref="GameObject{}"/>.
	/// </summary>
	internal static class AddAndInit
	{
		internal static TComponent Component<TComponent, TArgument>(GameObject gameObject, TArgument argument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), argument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), argument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		internal static TComponent Component<TComponent, TFirstArgument, TSecondArgument>(GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		internal static TComponent Component<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>(GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		internal static TComponent Component<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		internal static TComponent Component<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		internal static TComponent Component<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, bool setActive) where TComponent : Component
		{
			TComponent component;

			if(!gameObject.TryGetComponent(out component))
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				component = gameObject.AddComponent<TComponent>();
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			if(component is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return component;
			}

			if(setActive && !gameObject.activeSelf)
			{
				InitArgs.Set(typeof(TComponent), firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				gameObject.SetActive(true);
				if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(typeof(TComponent)))
				{
					return component;
				}
			}

			OnBeforeException(gameObject, setActive);
			throw new InitArgumentsNotReceivedException(typeof(TComponent), $"GameObject<{typeof(TComponent).Name}>.Init");
		}

		private static void OnBeforeException(GameObject gameObject, bool setActive)
		{
			if(setActive)
			{
				gameObject.SetActive(true);
			}
		}
	}
}