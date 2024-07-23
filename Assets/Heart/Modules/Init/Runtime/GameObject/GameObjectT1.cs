#define DEBUG_ENABLED

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Builder for creating a new <see cref="GameObject"/> with a <see cref="Component">component</see> of type <typeparamref name="TComponent"/>.
	/// <para>
	/// The <see cref="Component">component</see> must be initialized by calling an <see cref="GameObjectT1Extensions.Init{TComponent, TArgument}">Init</see>
	/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interface that <typeparamref name="TComponent"/> implements.
	/// </para>
	/// <para>
	/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
	/// <see cref="GameObjectT1Extensions.Init{TComponent}">the parameterless Init</see> function should be used.
	/// </para>
	/// <para>
	/// The new <see cref="GameObject"/> is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
	/// once the component has been initialized. This ensures that the Awake and OnEnable events for the component
	/// are only called after the component has received all its dependencies.
	/// </para>
	/// </summary>
	/// <typeparam name="TComponent"> Type of the added component. </typeparam>
	public struct GameObject<TComponent> where TComponent : Component
	{
		internal GameObject gameObject;
		internal readonly bool leaveInactive;
		internal readonly bool isClone;
		private bool isInitialized;

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		public GameObject(string name)
		{
			gameObject = new GameObject(name);
			gameObject.SetActive(false);
			leaveInactive = false;
			isClone = false;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(string name, Transform parent, bool active = true)
		{
			gameObject = new GameObject(name);
			gameObject.SetActive(false);
			leaveInactive = !active;
			gameObject.transform.SetParent(parent);
			isClone = false;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(string name, bool active) : this(name)
		{
			leaveInactive = !active;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		/// <param name="hideFlags"> Should the object be hidden, saved with the Scene or modifiable by the user? </param>
		public GameObject(string name, HideFlags hideFlags) : this(name)
		{
			gameObject.hideFlags = hideFlags;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(bool active) : this(typeof(TComponent).Name, active) { }

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, bool active = true) : this(typeof(TComponent).Name, parent, active)	{ }

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="positionInWorldSpace">
		/// <see langword="true"/> to position the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set the GameObject's position <see cref="Transform.localPosition">relative</see> to its new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, bool positionInWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(positionInWorldSpace)
			{
				gameObject.transform.position = position;
			}
			else
			{
				gameObject.transform.localPosition = position;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Quaternion rotation, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(inWorldSpace || parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				gameObject.transform.localScale = scale;
			}
			else if(inWorldSpace)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
				gameObject.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the new GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, bool inWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace && parent != null)
			{
				gameObject.transform.position = position;
				gameObject.transform.eulerAngles = eulerAngles;
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localEulerAngles = eulerAngles;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the new GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, bool inWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace && parent != null)
			{
				gameObject.transform.position = position;
				gameObject.transform.eulerAngles = eulerAngles;
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localEulerAngles = eulerAngles;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(inWorldSpace && parent != null)
			{
				gameObject.transform.position = position;
				gameObject.transform.eulerAngles = eulerAngles;
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localEulerAngles = eulerAngles;
				gameObject.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Vector3 position, bool active = true) : this(typeof(TComponent).Name, active)
		{
			gameObject.transform.localPosition = position;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		/// <param name="position"> The <see cref="Transform.position">position</see> for the GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(string name, Vector3 position, bool active = true) : this(name, active)
		{
			gameObject.transform.localPosition = position;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		public GameObject([DisallowNull] GameObject original)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(original.activeSelf)
			{
				original.SetActive(false);
				gameObject = Object.Instantiate(original);
				original.SetActive(true);
				leaveInactive = false;
			}
			else
			{
				gameObject = Object.Instantiate(original);
				leaveInactive = true;
			}

			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(original.activeSelf)
			{
				original.SetActive(false);
				gameObject = Object.Instantiate(original, parent);
				original.SetActive(true);
				leaveInactive = false;
			}
			else
			{
				gameObject = Object.Instantiate(original, parent);
				leaveInactive = true;
			}

			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, bool active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(original.activeSelf)
			{
				original.SetActive(false);
				gameObject = Object.Instantiate(original, parent);
				original.SetActive(true);
			}
			else
			{
				gameObject = Object.Instantiate(original, parent);
			}

			leaveInactive = !active;
			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="positionInWorldSpace">
		/// <see langword="true"/> to position the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set the GameObject's position <see cref="Transform.localPosition">relative</see> to its new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, bool positionInWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(positionInWorldSpace)
			{
				gameObject.transform.position = position;
			}
			else
			{
				gameObject.transform.localPosition = position;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="positionInWorldSpace">
		/// <see langword="true"/> to position the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set the GameObject's position <see cref="Transform.localPosition">relative</see> to its new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, bool positionInWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(positionInWorldSpace)
			{
				gameObject.transform.position = position;
			}
			else
			{
				gameObject.transform.localPosition = position;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Quaternion rotation, bool inWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace || parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Quaternion rotation, bool inWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace || parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, bool inWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				gameObject.transform.localScale = scale;
			}
			else if(inWorldSpace)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
				gameObject.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, bool inWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(parent == null)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				gameObject.transform.localScale = scale;
			}
			else if(inWorldSpace)
			{
				gameObject.transform.SetPositionAndRotation(position, rotation);
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localRotation = rotation;
				gameObject.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool inWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace && parent != null)
			{
				gameObject.transform.position = position;
				gameObject.transform.eulerAngles = eulerAngles;
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localEulerAngles = eulerAngles;
				gameObject.transform.localScale = scale;
			}
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>.
		/// <para>
		/// The component must be initialized by calling an <see cref="GameObjectT1Extensions.Init(GameObject{TComponent}, TArgument)">Init</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TComponent"/> implements.
		/// </para>
		/// <para>
		/// If <typeparamref name="TComponent"/> does not implement any IArgs interface then
		/// <see cref="GameObjectT1Extensions.Init(GameObject{TComponent})">the parameterless Init</see> function should be used.
		/// </para>
		/// <para>
		/// The new GameObject is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once the component has been initialized. This ensures that the Awake and OnEnable event functions for the component
		/// are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="original"> An existing object that you want to make a copy of. </param>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the new GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the new GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool inWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original != null, "GameObject constructor called with null original.");

			if(inWorldSpace && parent != null)
			{
				gameObject.transform.position = position;
				gameObject.transform.eulerAngles = eulerAngles;
				Vector3 parentScale = parent.lossyScale;
				gameObject.transform.localScale = new Vector3(parentScale.x == 0f ? scale.x : scale.x / parentScale.x, parentScale.y == 0f ? scale.y : scale.y / parentScale.x, parentScale.z == 0f ? scale.z : scale.z / parentScale.z);
			}
			else
			{
				gameObject.transform.localPosition = position;
				gameObject.transform.localEulerAngles = eulerAngles;
				gameObject.transform.localScale = scale;
			}
		}

		internal void OnBeforeInit()
		{
			if(gameObject == null)
			{
				gameObject = new GameObject(typeof(TComponent).Name);
				gameObject.SetActive(false);
			}
		}

		internal void OnAfterInit()
		{
			isInitialized = true;

			if(!leaveInactive)
			{
				gameObject.SetActive(true);
			}
		}

		internal void OnBeforeException()
		{
			OnAfterInit();
		}

		public void Deconstruct(out GameObject gameObject, out TComponent component)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out component);
				return;
			}

			component = this.Init();
			gameObject = component.gameObject;
		}

		/// <summary>
		/// Gets the <see cref="GameObject"/> to which the component is added.
		/// </summary>
		/// <param name="this"> Builder for creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>. </param>
		public static implicit operator GameObject(GameObject<TComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject : @this.Init().gameObject;
		}

		/// <summary>
		/// Gets the <see cref="Transform"/> component of the <see cref="GameObject"/> to which the component is added.
		/// </summary>
		/// <param name="this"> Builder for creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>. </param>
		public static implicit operator Transform(GameObject<TComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.transform : @this.Init().transform;
		}

		/// <summary>
		/// Gets the component that is added to the GameObject.
		/// </summary>
		/// <param name="this"> Builder for creating a new <see cref="GameObject"/> with a component of type <typeparamref name="TComponent"/>. </param>
		public static implicit operator TComponent(GameObject<TComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.GetComponent<TComponent>() : @this.Init();
		}
	}
}