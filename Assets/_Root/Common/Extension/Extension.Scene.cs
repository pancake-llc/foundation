namespace Pancake.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;
    public static partial class C
    {
        /// <summary>
        /// Returns an array filled with all the currently loaded scenes
        /// </summary>
        /// <returns></returns>
        public static Scene[] GetLoadedScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            return loadedScenes;
        }

        /// <summary>
        /// Returns a list of all the scenes present in the build
        /// </summary>
        /// <returns></returns>
        public static List<string> GetScenesInBuild()
        {
            List<string> scenesInBuild = new List<string>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
                scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1));
            }

            return scenesInBuild;
        }

        /// <summary>
        /// Returns true if a scene by the specified name is present in the build
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public static bool SceneContains(string sceneName) { return GetScenesInBuild().Contains(sceneName); }
    }
}