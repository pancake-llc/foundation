using System;
using System.Collections.Generic;
using System.Reflection;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sisus.Init.Testing
{
	/// <summary>
	/// A components that wraps a <see cref="GameObject"/> and can be used
	/// to invoke Unity event functions on <see cref="Component">components</see>
	/// found on the <see cref="GameObject"/> using reflection.
	/// <para>
	/// This can be useful for unit testing components in edit mode.
	/// </para>
	/// </summary>
	public sealed class Testable : IDisposable
	{
		private readonly GameObject gameObject = null;
		private readonly List<TestableComponent> components;

		public Transform transform => gameObject.transform;
		public string tag { get => gameObject.tag; set => gameObject.tag = value; }
		public string name { get => gameObject.name; set => gameObject.name = value; }
		public int layer { get => gameObject.layer; set => gameObject.layer = value; }

		public Testable(string name, bool invokeAllInitMethods = true) : this(new GameObject(name), invokeAllInitMethods) { }

		public Testable(GameObject gameObject, bool invokeAllInitMethods = true)
		{
			this.gameObject = gameObject;

			components = new List<TestableComponent>();

			foreach(var component in gameObject.GetComponentsNonAlloc<Component>())
			{
				if(component != null)
				{
					components.Add(new TestableComponent(component));
				}
			}

			components.Sort();

			if(!gameObject.activeInHierarchy || !invokeAllInitMethods || Application.isPlaying)
			{
				return;
			}

			foreach(var component in components)
			{
				if(!component.executesInEditMode)
				{
					component.ExecuteAwake();
					component.ExecuteOnEnable();
					component.ExecuteStart();
				}
			}
		}

		public static Testable Create<TComponent>(bool invokeAllInitMethods = true) where TComponent : Component
		{
			return new Testable(new GameObject<TComponent>(), invokeAllInitMethods);
		}

		public static Testable Create<TFirstComponent, TSecondComponent>(bool invokeAllInitMethods = true) where TFirstComponent : Component where TSecondComponent : Component
		{
			return new Testable(new GameObject<TFirstComponent, TSecondComponent>(), invokeAllInitMethods);
		}

		public static Testable Create<TFirstComponent, TSecondComponent, TThirdComponent>(bool invokeAllInitMethods = true) where TFirstComponent : Component where TSecondComponent : Component where TThirdComponent : Component
		{
			return new Testable(new GameObject<TFirstComponent, TSecondComponent, TThirdComponent>(), invokeAllInitMethods);
		}

		public void Awake<TComponent>()
		{
			foreach(var component in components)
			{
				if(component.component is TComponent)
				{
					component.ExecuteAwake();
				}
			}
		}

		public void Awake()
		{
			foreach(var component in components)
			{
				component.ExecuteAwake();
			}
		}

		public void OnEnable()
		{
			foreach(var component in components)
			{
				component.ExecuteOnEnable();
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnTriggerEnter(other);
			}
		}

		public void OnCollisionEnter(Collision other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionEnter(other);
			}
		}
        
		public void OnCollisionExit(Collision other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionExit(other);
			}
		}

		public void OnCollisionStay(Collision other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionStay(other);
			}
		}

		public void OnCollisionEnter2D(Collision2D other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionEnter2D(other);
			}
		}

		public void OnCollisionExit2D(Collision2D other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionExit2D(other);
			}
		}

		public void OnCollisionStay2D(Collision2D other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnCollisionStay2D(other);
			}
		}

		public void OnTriggerExit(Collider other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnTriggerExit(other);
			}
		}

		public void OnTriggerStay(Collider other)
		{
			foreach(var component in components)
			{
				component.ExecuteOnTriggerStay(other);
			}
		}

		public void Start()
		{
			foreach(var component in components)
			{
				component.ExecuteStart();
			}
		}

		public void Update(int times = 1)
		{
			for(int i = 0; i < times; i++)
			{
				foreach(var component in components)
				{
					component.ExecuteUpdate();
				}
			}
		}

		public void Reset()
		{
			foreach(var component in components)
			{
				component.ExecuteReset();
			}
		}

		public void OnValidate()
		{
			foreach(var component in components)
			{
				component.ExecuteOnValidate();
			}
		}

		public Testable SetActive(bool value)
		{
			gameObject.SetActive(value);
			return this;
		}

		public T GetComponent<T>() where T : class => gameObject.GetComponent<T>();

		public T AddComponent<T>(bool invokeInitFunctions = true) where T : Component
		{
			T component = gameObject.AddComponent<T>();
			var testable = new TestableComponent(component);

			components.Add(testable);

			if(!gameObject.activeInHierarchy || !invokeInitFunctions || !testable.executesInEditMode || Application.isPlaying)
			{
				return component;
			}

			testable.ExecuteAwake();
			testable.ExecuteOnEnable();
			testable.ExecuteStart();

			return component;
		}

		/// <summary>
		/// Destroys the GameObject that is wrapped by this <see cref="Testable"/> object.
		/// </summary>
		/// <param name="invokeRelatedEventFunctions"> If <see langword="true"/> then
		/// the OnDisable and OnDestroy event functions will be invoked on
		/// all <see cref="Component">components</see> that are attached to the
		/// <see cref="GameObject"/> before it is destroyed. </param>
		public void Destroy(bool invokeRelatedEventFunctions = true)
		{
			if(invokeRelatedEventFunctions && !Application.isPlaying)
			{
				foreach(var component in components)
				{
					if(!component.executesInEditMode)
					{
						if(gameObject.activeInHierarchy && component.Enabled)
						{
							component.ExecuteOnDisable();
						}
						component.ExecuteOnDestroy();
					}
				}
			}

			if(Application.isPlaying)
			{
				Object.Destroy(gameObject);
				return;
			}

			Object.DestroyImmediate(gameObject);
		}

		public void Dispose()
		{
			if(gameObject == null)
			{
				return;
			}

			Destroy();
		}

		public static implicit operator GameObject(Testable testable) => testable.gameObject;
		public static implicit operator Transform(Testable testable) => testable.transform;

		private class TestableComponent : IComparable<TestableComponent>
		{
			private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			public readonly Component component;
			public readonly Type componentType;
			public readonly bool executesInEditMode;

			public bool Enabled => !(component is Behaviour behaviour) || behaviour.enabled;

			public TestableComponent(Component component)
			{
				this.component = component;
				componentType = component.GetType();
				executesInEditMode = component.GetType().GetCustomAttributes(typeof(ExecuteAlways), false).Length > 0;         
			}

			public void ExecuteAllInitFunctions()
			{
				ExecuteAwake();
				ExecuteOnEnable();
				ExecuteStart();
			}

			public void ExecuteAwake() => Execute("Awake");
			public void ExecuteOnEnable() => Execute("OnEnable");
			public void ExecuteStart() => Execute("Start");

			public void ExecuteUpdate() => Execute("Update");
			public void ExecuteFixedUpdate() => Execute("FixedUpdate");
			public void ExecuteLateUpdate() => Execute("LateUpdate");

			public void ExecuteOnDisable() => Execute("OnDisable");
			public void ExecuteOnDestroy() => Execute("OnDestroy");

			public void ExecuteOnCollisionEnter(Collision other) => Execute("OnCollisionEnter", new Type[] { typeof(Collision) }, new object[] { other });
			public void ExecuteOnCollisionExit(Collision other) => Execute("OnCollisionExit", new Type[] { typeof(Collision) }, new object[] { other });
			public void ExecuteOnCollisionStay(Collision other) => Execute("OnCollisionStay", new Type[] { typeof(Collision) }, new object[] { other });

			public void ExecuteOnCollisionEnter2D(Collision2D other) => Execute("OnCollisionEnter2D", new Type[] { typeof(Collision2D) }, new object[] { other });
			public void ExecuteOnCollisionExit2D(Collision2D other) => Execute("OnCollisionExit2D", new Type[] { typeof(Collision2D) }, new object[] { other });
			public void ExecuteOnCollisionStay2D(Collision2D other) => Execute("OnCollisionStay2D", new Type[] { typeof(Collision2D) }, new object[] { other });

			public void ExecuteOnTriggerEnter(Collider other) => Execute("OnTriggerEnter", new Type[] { typeof(Collider) }, new object[] { other });
			public void ExecuteOnTriggerExit(Collider other) => Execute("OnTriggerExit", new Type[] { typeof(Collider) }, new object[] { other });
			public void ExecuteOnTriggerStay(Collider other) => Execute("OnTriggerStay", new Type[] { typeof(Collider) }, new object[] { other });

			public void ExecuteReset() => Execute("Reset");
			public void ExecuteOnValidate() => Execute("OnValidate");

			private MethodInfo GetMethod(string methodName) => GetMethod(methodName, Type.EmptyTypes);
			private MethodInfo GetMethod(string methodName, Type[] argumentTypes) => componentType.GetMethod(methodName, Flags, null, CallingConventions.Any, argumentTypes, null);

			private void Execute(string methodName) => GetMethod(methodName)?.Invoke(component, null);
			private void Execute(string methodName, Type[] parameterTypes, object[] parameters) => GetMethod(methodName, parameterTypes)?.Invoke(component, parameters);

			public int CompareTo(TestableComponent other)
			{
				if(other is null)
				{
					return Random.Range(0, 2) == 0 ? -1 : 1;
				}

				int a = ScriptExecutionOrder();
				int b = other.ScriptExecutionOrder();

				if(a == b)
				{
					return Random.Range(0, 2) == 0 ? -1 : 1;
				}
				return a.CompareTo(b);
			}

			private int ScriptExecutionOrder()
			{
				if(!(component is MonoBehaviour monoBehaviour))
				{
					return int.MaxValue;
				}

				MonoScript monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
				if(monoScript == null)
				{
					return int.MaxValue;
				}

				return MonoImporter.GetExecutionOrder(monoScript);
			}
		}
	}
}