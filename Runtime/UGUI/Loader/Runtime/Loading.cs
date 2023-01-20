using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Loader
{
    public sealed class Loading : MonoBehaviour
    {
        public LoadingComponent prefabLoading;
        public List<GameObject> dontDestroyOnLoad = new List<GameObject>();
        public UnityEvent onBeginEvents;
        public UnityEvent onFinishEvents;

        /// <summary>
        /// <see cref="funcWaiting"/> and <see cref="prepareActiveScene"/> only use for fakeloading
        /// </summary>
        /// <param name="sceneName">name scene to load</param>
        /// <param name="funcWaiting"> condition to waiting done loading progress</param>
        /// <param name="actionWaitBeforeLoadScene">action wait done before call load scene async</param>
        /// <param name="prepareActiveScene">action prepare call before action scene</param>
        public void LoadScene(string sceneName, Func<bool> funcWaiting = null, Func<bool> actionWaitBeforeLoadScene = null, Action prepareActiveScene = null)
        {
            LoadingComponent.instance = Instantiate(prefabLoading);
            LoadingComponent.instance.LoadScene(sceneName,
                funcWaiting,
                actionWaitBeforeLoadScene,
                prepareActiveScene,
                onBeginEvents,
                onFinishEvents);

            foreach (var obj in dontDestroyOnLoad)
            {
                DontDestroyOnLoad(obj);
            }
        }


        /// <summary>
        /// <see cref="funcWaiting"/> and <see cref="prepareActiveScene"/> only use for fakeloading
        /// </summary>
        /// <param name="sceneName">main scene name to load</param>
        /// <param name="subScene">sub scene name to load</param>
        /// <param name="funcWaiting"> condition to waiting done loading progress</param>
        /// <param name="actionWaitBeforeLoadScene">action wait done before call load scene async</param>
        /// <param name="prepareActiveScene">action prepare call before action scene</param>
        public void LoadScene(
            string sceneName,
            string subScene,
            Func<bool> funcWaiting = null,
            Func<bool> actionWaitBeforeLoadScene = null,
            Action prepareActiveScene = null)
        {
            LoadingComponent.instance = Instantiate(prefabLoading);
            LoadingComponent.instance.LoadScene(sceneName,
                subScene,
                funcWaiting,
                actionWaitBeforeLoadScene,
                prepareActiveScene,
                onBeginEvents,
                onFinishEvents);

            foreach (var obj in dontDestroyOnLoad)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }
}