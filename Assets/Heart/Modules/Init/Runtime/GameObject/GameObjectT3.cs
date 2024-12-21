#define DEBUG_ENABLED

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Builder for creating a new <see cref="GameObject"/> with three <see cref="Component">components</see>.
	/// <para>
	/// The components must be initialized by first calling an
	/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
	/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
	/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
	/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
	/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
	/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
	/// interface that <typeparamref name="TThirdComponent"/> implements.
	/// </para>
	/// <para>
	/// If none of the three components implements any IArgs interfaces then a single call to
	/// <see cref="GameObjectT3Extensions.Init{TComponent}">the parameterless Init</see> function can be used to initialize all components.
	/// </para>
	/// <para>
	/// The new <see cref="GameObject"/> is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
	/// once all three components have been initialized. This ensures that the Awake and OnEnable events for the components
	/// are only called after both components have received all their dependencies.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstComponent"> Type of the first added component. </typeparam>
	/// <typeparam name="TSecondComponent"> Type of the second added component. </typeparam>
	/// <typeparam name="TThirdComponent"> Type of the third added component. </typeparam>
	public struct GameObject<TFirstComponent, TSecondComponent, TThirdComponent> where TFirstComponent : Component where TSecondComponent : Component where TThirdComponent : Component
	{
		internal GameObject gameObject;
		internal readonly bool leaveInactive;
		internal readonly bool isClone;
		private bool isInitialized;

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with <see cref="Component">components</see> of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new <see cref="GameObject"/> is first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been initialized. This ensures that the Awake and OnEnable event functions for the components
		/// are only called after both components have been added and initialized with their dependencies.
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

		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// <param name="name"> The <see cref="GameObject.name">name</param> that the GameObject is created with. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(string name, bool active) : this(name)
		{
			leaveInactive = !active;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
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
			isClone = false;
			gameObject.transform.SetParent(parent);
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(bool active) : this(typeof(TFirstComponent).Name, active) { }

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, bool active = true) : this(typeof(TFirstComponent).Name, parent, active)	{ }

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the GameObject. </param>
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
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the GameObject. </param>
		/// <param name="rotation"> The orientation of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Quaternion rotation, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(inWorldSpace || !parent)
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
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the GameObject. </param>
		/// <param name="rotation"> The orientation of the GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(!parent)
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
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="parent"> The <see cref="Transform.parent">parent</see> of the created GameObject. </param>
		/// <param name="position"> The position for the GameObject. </param>
		/// <param name="eulerAngles"> The orientation of the GameObject. </param>
		/// <param name="scale"> The scale of the GameObject. </param>
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position, rotation and scale for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Transform parent, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool inWorldSpace = false, bool active = true) : this(parent, active)
		{
			if(inWorldSpace && parent)
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
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
		/// This ensures that the Awake and OnEnable event functions for the component are only called after the component has been initialized with its dependencies.
		/// </para>
		/// </summary>
		/// <param name="position"> The position for the GameObject. </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject(Vector3 position, bool active = true) : this(typeof(TFirstComponent).Name, active)
		{
			gameObject.transform.localPosition = position;
		}

		/// <summary>
		/// Starts the process of creating a new <see cref="GameObject"/> with components of type <typeparamref name="TFirstComponent"/> and <typeparamref name="TSecondComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
		/// </para>
		/// <para>
		/// The new GameObject is always first created in an <see cref="GameObject.activeSelf">inactive state</see> and only activated
		/// once both components have been added and initialized and only if <paramref name="active"/> is set to <see langword="true"/>.
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			bool wasActive = original.activeSelf;
			original.SetActive(false);
			gameObject = Object.Instantiate(original);
			original.SetActive(wasActive);

			leaveInactive = !wasActive;
			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			bool wasActive = original.activeSelf;
			original.SetActive(false);
			gameObject = Object.Instantiate(original, parent);
			original.SetActive(wasActive);

			leaveInactive = !wasActive;
			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			bool wasActive = original.activeSelf;
			original.SetActive(false);
			gameObject = Object.Instantiate(original, parent);
			original.SetActive(wasActive);

			leaveInactive = !active;
			isClone = true;
			isInitialized = false;
		}

		/// <summary>
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace || !parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace || !parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(!parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(!parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, bool inWorldSpace = false) : this(original, parent, original.activeSelf)
		{
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace && parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
		/// <param name="inWorldSpace">
		/// <see langword="true"/> to set the position and rotation for the new GameObject directly in <see cref="Transform.position">world space</see>
		/// or <see langword="false"/> to set them <see cref="Transform.localPosition">relative</see> to the GameObject's new <paramref name="parent"/>.
		/// </param>
		/// <param name="active"> Defines whether the created GameObject is <see cref="GameObject.activeSelf">active</see> in the Scene. </param>
		public GameObject([DisallowNull] GameObject original, Transform parent, Vector3 position, Vector3 eulerAngles, bool inWorldSpace, bool active) : this(original, parent, active)
		{
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace && parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace && parent)
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
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type
		/// <typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// Starts the process of cloning the <paramref name="original"/> <see cref="GameObject"/> with a component of type\n<typeparamref name="TFirstComponent"/>, <typeparamref name="TSecondComponent"/> and <typeparamref name="TThirdComponent"/>.
		/// <para>
		/// The components must be initialized by first calling an
		/// <see cref="GameObjectT3Extensions.Init1{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init1</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TFirstComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init2{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init2</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TSecondComponent"/> implements and then calling an
		/// <see cref="GameObjectT3Extensions.Init3{TFirstComponent, TSecondComponent, TThirdComponent, TArgument}">Init3</see>
		/// function with arguments matching the argument list defined by the <see cref="IArgs{TArgument}">IArgs</see>
		/// interface that <typeparamref name="TThirdComponent"/> implements.
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
			Debug.Assert(original, "GameObject constructor called with null original.");

			if(inWorldSpace && parent)
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

		internal void OnBeforeFirstInit()
		{
			if(!gameObject)
			{
				gameObject = new GameObject(typeof(TFirstComponent).Name);
				gameObject.SetActive(false);
			}
		}

		internal void OnAfterLastInit()
		{
			isInitialized = true;

			if(!leaveInactive)
			{
				gameObject.SetActive(true);
			}
		}

		internal void OnBeforeException()
		{
			OnAfterLastInit();
		}

		public void Deconstruct(out TFirstComponent firstComponent, out TSecondComponent secondComponent)
		{
			if(isInitialized)
			{
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out secondComponent);
				return;
			}

			var initialized = this.Init();
			firstComponent = initialized.first;
			secondComponent = initialized.second;
		}

		public void Deconstruct(out TFirstComponent firstComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			firstComponent = initialized.first;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out TSecondComponent secondComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject.TryGetComponent(out secondComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			secondComponent = initialized.second;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out TFirstComponent firstComponent, out TSecondComponent secondComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out secondComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			firstComponent = initialized.first;
			secondComponent = initialized.second;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out GameObject gameObject, out TFirstComponent firstComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out firstComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			firstComponent = initialized.first;
		}

		public void Deconstruct(out GameObject gameObject, out TSecondComponent secondComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out secondComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			secondComponent = initialized.second;
		}

		public void Deconstruct(out GameObject gameObject, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out GameObject gameObject, out TFirstComponent firstComponent, out TSecondComponent secondComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out secondComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			firstComponent = initialized.first;
			secondComponent = initialized.second;
		}

		public void Deconstruct(out GameObject gameObject, out TFirstComponent firstComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			firstComponent = initialized.first;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out GameObject gameObject, out TSecondComponent secondComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out secondComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			secondComponent = initialized.second;
			thirdComponent = initialized.third;
		}

		public void Deconstruct(out GameObject gameObject, out TFirstComponent firstComponent, out TSecondComponent secondComponent, out TThirdComponent thirdComponent)
		{
			if(isInitialized)
			{
				gameObject = this.gameObject;
				gameObject.TryGetComponent(out firstComponent);
				gameObject.TryGetComponent(out secondComponent);
				gameObject.TryGetComponent(out thirdComponent);
				return;
			}

			var initialized = this.Init();
			gameObject = this.gameObject;
			firstComponent = initialized.first;
			secondComponent = initialized.second;
			thirdComponent = initialized.third;
		}

		/// <summary>
		/// Gets the <see cref="GameObject"/> to which the components are added.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator GameObject(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject : @this.Init().first.gameObject;
		}

		/// <summary>
		/// Gets the <see cref="Transform"/> component of the <see cref="GameObject"/> to which the components are added.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator Transform(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.transform : @this.Init().first.transform;
		}

		/// <summary>
		/// Gets the first component added to the GameObject.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator TFirstComponent(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.GetComponent<TFirstComponent>() : @this.Init().first;
		}

		/// <summary>
		/// Gets the second component added to the GameObject.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator TSecondComponent(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.GetComponent<TSecondComponent>() : @this.Init().second;
		}

		/// <summary>
		/// Gets the third component added to the GameObject.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator TThirdComponent(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized ? @this.gameObject.GetComponent<TThirdComponent>() : @this.Init().third;
		}

		/// <summary>
		/// Gets a <see cref="System.ValueTuple{TFirstComponent, TSecondComponent}">tuple</see> containing the three components added to the GameObject.
		/// </summary>
		/// Builder for creating a new <see cref="GameObject"/> with three components.
		public static implicit operator (TFirstComponent, TSecondComponent, TThirdComponent)(GameObject<TFirstComponent, TSecondComponent, TThirdComponent> @this)
		{
			return @this.isInitialized
				? (@this.gameObject.GetComponent<TFirstComponent>(), @this.gameObject.GetComponent<TSecondComponent>(), @this.gameObject.GetComponent<TThirdComponent>())
				: @this.Init();
		}
	}
}