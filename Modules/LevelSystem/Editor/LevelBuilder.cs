using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pancake.LevelSystem;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.LevelSystemEditor
{
    [ExecuteInEditMode]
    public sealed class LevelBuilder : GameComponent
    {
        [SerializeField] private LevelSystemSetting setting;
        [SerializeField] private LevelExtraInfo[] levelExtraInfos;
        [SerializeField] private int currentLevel;

        private string _currentLevelJson;


        /// <summary>
        /// Delete all object from the root
        /// </summary>
        private void ClearLevel()
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
            if (string.IsNullOrEmpty(r)) EditorUtility.DisplayDialog("Warning", r, "Ok");
            else _currentLevelJson = JsonConvert.SerializeObject(levelNode);
        }

        private void Save()
        {
            
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