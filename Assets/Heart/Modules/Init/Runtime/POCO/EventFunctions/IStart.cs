using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that wants to receive a callback during the Start event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
    /// </summary>
    [RequireImplementors]
    public interface IStart
    {
        /// <summary>
        /// <see cref="Start"/> is called after the object, and any other objects attached to the same loaded scene or prefab,
        /// have been loaded.
        /// <para>
        /// If the object has been attached to an <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/>
        /// at edit time, then <see cref="Start"/> is called after the scene or prefab that contains the <see cref="GameObject"/>
        /// has been loaded.
        /// </para>
        /// <para>
        /// If the object is attached to an <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/>
        /// at runtime, then <see cref="Start"/> is called after a short delay, just before the next <see cref="IUpdate.Update"/>
        /// event.
        /// </para>
        /// <para>
        /// If the object is attached to an <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/>,
        /// then <see cref="Start"/> will only be called when the <see cref="GameObject"/> becomes <see cref="GameObject.activeInHierarchy">active</see>.
        /// </para>
        /// <para>
        /// A class that has the Service attribute receives the event after all services have being initialized,
        /// and the initial scene has been loaded.
        /// </para>
        /// <para>
        /// While <see cref="IAwake.Awake"/> and <see cref="IOnEnable.OnEnable"/> are called (in that order)
        /// immediately for all enabled and active objects when a scene or a prefab is being loaded,
        /// <see cref="Start"/> only gets called after a short delay, just before the first <see cref="IUpdate.Update"/> event.
        /// </para>
        /// </summary>
        void Start();
    }
}