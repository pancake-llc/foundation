using System;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using System.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("pancake@initargs.editor")]
#endif

namespace Pancake.Init
{
    /// <summary>
    /// The <see cref="Services"/> component can be attached to a <see cref="GameObject"/> and used to
    /// define services which can be injected to its clients as part of their initialization process.
    /// <para>
    /// Clients which have access to all the services defined in the component can be configured to be
    /// limited only to members beloning to any of the following groups:
    /// <list type="table">
    /// <item><term> In GameObject </term>
    /// <description> Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
    /// <see cref="Services"/> component have access to its services.
    /// </description></item>
    /// <item><term> In Children </term>
    /// <description> Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
    /// <see cref="Services"/> component or any child GameObjects below it (including nested children)
    /// have access to its services.
    /// </description></item>
    /// <item><term> In Parents </term>
    /// <description> Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
    /// <see cref="Services"/> component or any parent GameObjects above it (including nested parents)
    /// have access to its services.
    /// </description></item>
    /// <item><term> In Hierarchy Root Children </term>
    /// <description> Only clients that are attached to the <see cref="UnityEngine.GameObject"/> at the
    /// <see cref="UnityEngine.Transform.root"/> of the hierarchy when traversing up the <see cref="Services"/>
    /// component's parent chain, or any child GameObjects below the root GameObject (including nested children)
    /// have access to its services.
    /// </description></item>
    /// <item><term> In Scene </term>
    /// <description> Only clients belonging to the same <see cref="UnityEngine.GameObject.scene"/> as the
    /// <see cref="Services"/> component have acceess to its services.
    /// </description></item>
    /// <item><term> In All Scenes </term>
    /// <description>
    /// All clients beloning to any scene have access to the services in the <see cref="Services"/> component.
    /// <para>
    /// Clients that don't belong to any scene, such as <see cref="UnityEngine.ScriptableObject">ScriptableObjects</see>
    /// and plain old classes that are not attached to a <see cref="UnityEngine.GameObject"/> via a <see cref="Wrapper{}"/> component,
    /// can not access the services in the <see cref="Services"/> component.
    /// </para>
    /// </description></item>
    /// <item><term> Everywhere </term>
    /// <description>
    /// All clients have access to the services in the <see cref="Services"/> component without limitations.
    /// <para>
    /// This includes clients that don't belong to any scene, such as <see cref="UnityEngine.ScriptableObject">ScriptableObjects</see>
    /// and plain old classes.
    /// </para>
    /// </description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <seealso cref="ServiceTag{TComponent, TDefiningType}"/>
    [ExecuteAlways, AddComponentMenu("Initialization/Services"), DefaultExecutionOrder(ExecutionOrder.Services)]
    public partial class Services : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("provideServices")]
        internal ServiceDefinition[] providesServices = new ServiceDefinition[0];

        [SerializeField, Tooltip("Limits what clients have access to the services in this Services by their location in the scene hierarchy.\n\nWhen set to 'Children' only clients that are attached to the same GameObject as this Services or or any of its children (including nested children) can access the services in this Services.\n\nWhen set to 'Scene' only clients that are in the same scene as this Services can access the services in this Services.\n\nWhen set to 'Global', all clients are allowed to access the services in this Services regardless of where they are in the scene hierarchy.")]
        internal Clients toClients = Clients.InChildren;

        protected virtual void OnEnable() => Register(this, providesServices, toClients);
        protected virtual void OnDisable() => Deregister(this);

        internal virtual bool AreAvailableToAnyClient() => toClients == Clients.Everywhere;

        internal virtual bool AreAvailableToClient([NotNull] GameObject client)
        {
            Debug.Assert(client != null);
            Debug.Assert(this != null);

            switch(toClients)
            {
                case Clients.InGameObject:
                    return client == gameObject;
                case Clients.InChildren:
                    var injectorTransform = transform;
                    for(var parent = client.transform; parent != null; parent = parent.parent)
                    {
                        if(parent == injectorTransform)
                        {
                            return true;
                        }
                    }
                    return false;
                case Clients.InParents:
                    var clientTransform = client.transform;
                    for(var parent = transform; parent != null; parent = parent.parent)
                    {
                        if(parent == clientTransform)
                        {
                            return true;
                        }
                    }
                    return false;
                case Clients.InHierarchyRootChildren:
                    return transform.root == client.transform.root;
                case Clients.InScene:
                    return client.scene == gameObject.scene;
                case Clients.InAllScenes:
                case Clients.Everywhere:
                    return true;
                default:
                    Debug.LogError($"Unrecognized {nameof(Clients)} value: {toClients}.", this);
                    return false;
            }
        }

        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            bool isSelected = Array.IndexOf(UnityEditor.Selection.gameObjects, gameObject) != -1;

            if(gameObject.scene.IsValid())
            {
                var services = isSelected ? providesServices.Where(s => s.service != null && s.definingType.Value != null) : providesServices;
                Register(this, services, toClients);
            }

            if(!isSelected)
            {
                for(int i = 0, count = providesServices.Length; i < count; i++)
                {
                    if(providesServices[i].service == null)
                    {
                        Debug.LogWarning($"Service #{i} on \"{name}\" is missing.", this);
                    }
                    else if(providesServices[i].definingType == null)
                    {
                        Debug.LogWarning($"Defining Type of service #{i} on \"{name}\" is missing.", this);
                    }
                }
                return;
            }

            for(int i = providesServices.Length - 1; i >= 0; i--)
            {
                ServiceDefinition.OnValidate(this, ref providesServices[i]);
            }
        }
        #endif
    }
}