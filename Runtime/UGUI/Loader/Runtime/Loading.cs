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
        /// <param name="sceneName"></param>
        /// <param name="funcWaiting"> condition to waiting done loading progress</param>
        /// <param name="prepareActiveScene">action prepare call before action scene</param>
        public void LoadScene(string sceneName, Func<bool> funcWaiting = null, Action prepareActiveScene = null)
        {
            LoadingComponent.instance = Instantiate(prefabLoading);
            LoadingComponent.instance.LoadScene(sceneName,
                funcWaiting,
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
        /// <param name="sceneName"></param>
        /// <param name="subScene"></param>
        /// <param name="funcWaiting"> condition to waiting done loading progress</param>
        /// <param name="prepareActiveScene">action prepare call before action scene</param>
        public void LoadScene(string sceneName, string subScene, Func<bool> funcWaiting = null, Action prepareActiveScene = null)
        {
            LoadingComponent.instance = Instantiate(prefabLoading);
            LoadingComponent.instance.LoadScene(sceneName,
                subScene,
                funcWaiting,
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