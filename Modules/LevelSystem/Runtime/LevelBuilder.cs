#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pancake.LevelSystem;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.LevelSystemEditor
{
    [EditorIcon("script_mono")]
    public sealed class LevelBuilder : GameComponent
    {
        [SerializeField] private LevelSystemSetting setting;
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private LevelExtraInfo[] levelExtraInfos;

        private string _currentLevelJson;

        public int CurrentLevel { get => currentLevel; set => currentLevel = value; }

        /// <summary>
        /// Delete all object from the root
        /// </summary>
        public void ClearLevel()
        {
            transform.RemoveAllChildren();
            levelExtraInfos = Array.Empty<LevelExtraInfo>();
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

            var newObject = (GameObject) PrefabUtility.InstantiatePrefab(prefab.prefab, parent);
            newObject.transform.localPosition = go.pos;
            newObject.transform.localRotation = go.rot;
            newObject.transform.localScale = go.sc;

            if (go.ex is {Length: > 0})
            {
                var extraComponent = newObject.AddComponent<LevelExtraInfoComponent>();
                extraComponent.Init(go.ex);
            }

            if (go.c is {Length: > 0})
            {
                foreach (var o in go.c)
                {
                    AddGameObject(o, notSpawnedGos, newObject.transform);
                }
            }
        }

        private void SaveJson(int level)
        {
            var levelNode = CreateLevelNode(true);
            if (levelNode == null) return;

            levelNode.level = level;
            string r = LevelReader.Write(setting, levelNode, level);
            if (!string.IsNullOrEmpty(r)) EditorUtility.DisplayDialog("Warning", r, "Ok");
            else _currentLevelJson = JsonConvert.SerializeObject(levelNode);
        }

        /// <summary>
        /// Save to file
        /// </summary>
        public void Save()
        {
            if (!LevelHelper.IsValid(setting))
            {
                EditorUtility.DisplayDialog("Warning",
                    "Please check the setting file.\nYou have to specify not null prefabs with a valid ID or units is empty",
                    "Ok");
                return;
            }

            SaveJson(currentLevel);
        }

        /// <summary>
        /// Delete level from json file
        /// </summary>
        /// <param name="level"></param>
        public void DeleteLevel(int level)
        {
            LevelReader.DeleteLevel(setting, level);
            if (currentLevel == level) ClearLevel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="noCheckEdit"></param>
        /// <returns></returns>
        public LevelNode OpenLevel(int level, bool noCheckEdit = true)
        {
            if (!noCheckEdit) // check if there are pending changes in the current level
            {
                var levelNode = CreateLevelNode();
                string str = JsonConvert.SerializeObject(levelNode);
                if (!string.IsNullOrEmpty(_currentLevelJson) && _currentLevelJson != str)
                {
                    if (!EditorUtility.DisplayDialog("Confirm Open", "The changes in the current level will be lost. Continue?", "Open Level", "Cancel"))
                    {
                        return null;
                    }
                }
            }

            if (!LevelHelper.IsValid(setting))
            {
                EditorUtility.DisplayDialog("Warning",
                    "Please check the config file. You have to specify not null prefabs with a valid ID. The prefabs count must be greater than 0",
                    "Ok");
                return null;
            }

            var l = LevelReader.Read(setting, level);
            if (l == null) return null;

            currentLevel = level;
            _currentLevelJson = JsonConvert.SerializeObject(l);
            ClearLevel();
            var notSpawnedGos = new List<LevelGameObject>();
            foreach (var o in l.objects)
            {
                AddGameObject(o, notSpawnedGos);
            }

            if (notSpawnedGos.Count > 0)
            {
                string msg = "Attention: Some objects were not instantiated because they were not in the config prefab list.\nObjects in error: " +
                             string.Join(", ", notSpawnedGos.Map(x => x.id).Distinct());
                EditorUtility.DisplayDialog("Warning", msg, "Ok");
            }

            if (l.extraInfos is {Length: > 0}) levelExtraInfos = (LevelExtraInfo[]) l.extraInfos.Clone();
            return l;
        }

        /// <summary>
        /// how many levels are already built in the files?
        /// </summary>
        /// <returns></returns>
        public int GetLevelCount()
        {
            return setting != null ? LevelReader.GetLevelCount(setting) : 0;
        }

        private LevelNode CreateLevelNode(bool warningMessage = false)
        {
            int childCount = transform.childCount;
            var childrenDatas = new List<LevelGameObject>();
            var missingObjects = new List<Transform>();

            for (var i = 0; i < childCount; i++)
            {
                var childData = CreateLevelGameObject(transform.GetChild(i), ref missingObjects);
                if (childData != null) childrenDatas.Add(childData);
            }

            var levelNode = new LevelNode {level = currentLevel, objects = childrenDatas.ToArray()};

            if (!levelExtraInfos.IsNullOrEmpty()) levelNode.extraInfos = (LevelExtraInfo[]) levelExtraInfos.Clone();

            if (missingObjects.Count > 0 && warningMessage)
            {
                string msg = "Attention: Some objects can't be saved because they can't be found in the prefab list. \nObjects in error: " +
                             string.Join(", ", missingObjects.Map(x => x.name).Distinct()) + ". \nSave the level anyway?";
                if (!EditorUtility.DisplayDialog("Confirm save", msg, "Save", "Don't save"))
                {
                    levelNode = null;
                }
            }

            return levelNode;
        }

        private LevelGameObject CreateLevelGameObject(Transform t, ref List<Transform> missingObjects)
        {
            if (t == null) return null;

            if (!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) return null; // check if it's a prefab root, otherwise continue

            LevelUnit unit = null;
            var source = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
            foreach (var levelUnit in setting.Units)
            {
                if (levelUnit.prefab == source)
                {
                    unit = levelUnit;
                    break;
                }
            }

            if (unit == null)
            {
                // this object is not specified in any prefab of the list, but check if the source makes part of any other prefab in the list, to send an error message
                var isInAnyPrefab = false;
                foreach (var prefab in setting.Units)
                {
                    if (IsObjectInPrefab(prefab.prefab, source.transform))
                    {
                        isInAnyPrefab = true;
                        break;
                    }
                }

                if (!isInAnyPrefab) missingObjects.Add(t);
                return null;
            }

            var levelGameObject = new LevelGameObject {pos = t.localPosition, rot = t.localRotation, sc = t.localScale, id = unit.id};
            t.TryGetComponent(out LevelExtraInfoComponent levelExtraComponent);
            if (levelExtraComponent != null)
            {
                var extraInfo = levelExtraComponent.GetAllExtraInfos();
                levelGameObject.ex = extraInfo;
            }

            var childrenDatas = new List<LevelGameObject>();
            int childCount = t.childCount;
            // check recursively if there are other children inside this object
            for (int i = 0; i < childCount; i++)
            {
                var childData = CreateLevelGameObject(t.GetChild(i), ref missingObjects);
                if (childData != null) childrenDatas.Add(childData);
            }

            if (childrenDatas.Count > 0) levelGameObject.c = childrenDatas.ToArray();
            return levelGameObject;
        }

        // check if an object makes part of a prefab
        private bool IsObjectInPrefab(GameObject prefab, Transform obj)
        {
            if (prefab == null) return false;
            if (prefab.transform == obj) return true;

            for (var i = 0; i < prefab.transform.childCount; i++)
            {
                if (IsObjectInPrefab(prefab.transform.GetChild(i).gameObject, obj)) return true;
            }

            return false;
        }
    }
}
#endif