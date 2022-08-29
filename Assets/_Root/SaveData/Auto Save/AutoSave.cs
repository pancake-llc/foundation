using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Pancake.SaveData
{
    public class AutoSave : MonoBehaviour
    {
        private static AutoSave current;
    
        public static AutoSave Current
        {
            get
            {
                if (current == null)
                {
                    var scene = SceneManager.GetActiveScene();
                    var roots = scene.GetRootGameObjects();
    
                    // First, look for Save Data Manager in the top-level.
                    foreach (var root in roots)
                        if (root.name == "ArchiveManager")
                            return current = root.GetComponent<AutoSave>();
    
                    // If the user has moved or renamed the Archivemanager, we need to perform a deep search.
                    foreach (var root in roots)
                        if ((current = root.GetComponentInChildren<AutoSave>()) != null)
                            return current;
                }
    
                return current;
            }
        }
    
        public enum LoadEvent
        {
            None,
            Awake,
            Start
        }
    
        public enum SaveEvent
        {
            None,
            OnApplicationQuit,
            OnApplicationPause
        }
        
        public SaveEvent saveType = SaveEvent.OnApplicationQuit;
        public LoadEvent loadType = LoadEvent.Awake;
        [Space]
        public UnityEvent saveEvent;
        public UnityEvent loadEvent;
    
        private void Save() { saveEvent?.Invoke(); }
    
        private void Load() { loadEvent?.Invoke(); }
    
        private void Start()
        {
            if (loadType == LoadEvent.Start) Load();
        }
    
        public void Awake()
        {
            current = this;
    
            if (loadType == LoadEvent.Awake) Load();
        }
    
        private void OnApplicationQuit()
        {
            if (saveType == SaveEvent.OnApplicationQuit) Save();
        }
    
        private void OnApplicationPause(bool paused)
        {
            if ((saveType == SaveEvent.OnApplicationPause || (Application.isMobilePlatform && saveType == SaveEvent.OnApplicationQuit)) && paused) Save();
        }
    }
}