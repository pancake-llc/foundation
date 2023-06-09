using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [HideMonoScript]
    [EditorIcon("script_mono")]
    public class LevelComponent : GameComponent
    {
        [SerializeField] private bool loadLevelOnStartup = true;
        [SerializeField] private int currentLevel;
        [SerializeField] private LevelSystemSetting setting;
        [SerializeField] private ScriptableLevelListCallback levelListCallback;

        private LevelNode _currentLevelNode;
        private LevelExtraInfoComponent _extraInfoComponent;

        public bool IsLoaded { get; private set; }
        public List<Transform> LoadedObjects { get; private set; } = new List<Transform>();

        private void Awake()
        {
            if (loadLevelOnStartup && currentLevel > 0) LoadLevel(currentLevel);
        }

        private void LoadLevel(int levelNumber)
        {
            IsLoaded = false;
            levelListCallback.isLevelLoaded = IsLoaded;
            var level = LevelReader.Read(setting, levelNumber);
            if (level == null)
            {
                Debug.LogError($"Level System: Error! Level {levelNumber} not found");
                return;
            }

            if (!LevelHelper.IsValid(setting))
            {
                Debug.LogError(
                    "Level System: Error! Please check the setting file. You have to specify not null prefabs with a valid ID. The prefabs count must be greater than 0");
                return;
            }

            _currentLevelNode = level;
            var notSpawnedGos = new List<LevelGameObject>();
            foreach (var o in level.objects)
            {
                AddGameObject(o, notSpawnedGos);
            }

            if (notSpawnedGos.Count > 0)
            {
                string msg = "Level System: Error! Some objects were not instantiated because they were not in the config prefab list.Objects in error: " +
                             string.Join(", ", notSpawnedGos.Map(x => x.id).Distinct());
                Debug.LogError(msg);
            }

            if (level.extraInfos is {Length: > 0})
            {
                _extraInfoComponent = GetComponent<LevelExtraInfoComponent>();
                if (_extraInfoComponent == null) _extraInfoComponent = gameObject.AddComponent<LevelExtraInfoComponent>();
                _extraInfoComponent.Init(level.extraInfos);
            }

            foreach (var action in levelListCallback)
            {
                levelListCallback.Remove(action);
                action();
            }            

            IsLoaded = true;
            levelListCallback.isLevelLoaded = IsLoaded;
        }

        private void AddGameObject(LevelGameObject go, List<LevelGameObject> notSpawnedGos, Transform parent = null)
        {
            if (parent == null) parent = transform;
            var prefab = setting.Units.FirstOrDefault(_ => _.id == go.id);
            if (prefab == null)
            {
                notSpawnedGos.Add(go);
                return;
            }

            var newObject = Instantiate(prefab.prefab, parent);
            newObject.transform.localPosition = go.pos;
            newObject.transform.localRotation = go.rot;
            newObject.transform.localScale = go.sc;

            LoadedObjects.Add(newObject.transform);

            if (go.ex is {Length: > 0}) // if extra info is specified, add the LevelExtraInfo controller to manage it
            {
                var levelEx = newObject.AddComponent<LevelExtraInfoComponent>();
                levelEx.Init(go.ex);
            }

            // instantiate children
            if (go.c is {Length: > 0})
            {
                foreach (var child in go.c)
                {
                    AddGameObject(child, notSpawnedGos, newObject.transform);
                }
            }
        }
    }
}