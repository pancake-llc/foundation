using System.Collections;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine.SceneManagement;

namespace Pancake.DebugView
{
    using UnityEngine;

    public class DebugAutoFillHighestCamera : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private void OnEnable()
        {
            App.StartCoroutine(Find());
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode loaded)
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            App.StartCoroutine(Find());
        }

        private IEnumerator Find()
        {
            yield return null;
            var cams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            cams = cams.OrderByDescending(c => c.depth);

            var flag = false;
            foreach (var c in cams)
            {
                if (!c.name.Equals("PlaceHolderCamera")) continue;
                canvas.worldCamera = c;
                flag = true;
            }

            if (!flag) canvas.worldCamera = cams[0];
        }

        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoad; }
    }
}