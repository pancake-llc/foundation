using System;
using System.Collections.Generic;
using Pancake.Common;
using Pancake.Tracking;
using Sirenix.Serialization;
using UnityEngine;

namespace Pancake.LevelSystem
{
    public class LevelInstantiateDimension
    {
        internal event Action ReCreateLevelLoadedEvent;

        public void RecreateLevelLoaded() { ReCreateLevelLoadedEvent?.Invoke(); }
    }

    public class LevelInstantiate : GameComponent
    {
        [SerializeField] private StringKey type;
        [SerializeField] private Transform root;
        [OdinSerialize] private ITracking[] trackingStartLevels;

        private static readonly Dictionary<string, LevelInstantiateDimension> Dimensions = new();

        public void Awake()
        {
            Dimensions[type] = new LevelInstantiateDimension();
            foreach (var t in trackingStartLevels)
            {
                t.Track();
            }

            LevelComponent levelComponent;
#if UNITY_EDITOR
            levelComponent = LevelDebug.IsTest ? LevelDebug.LevelPrefab : LevelCoordinator.GetNextLevelLoaded(type);
#else
            levelComponent = LevelCoordinator.GetNextLevelLoaded(type.Name);
#endif
            Instantiate(levelComponent, root, false);
        }

        protected void OnEnable() { RegisterActionRecreateLevel(type, OnRecreateLevelLoaded); }

        protected void OnDisable()
        {
            UnRegisterActionRecreateLevel(type, OnRecreateLevelLoaded);
            Dimensions.Remove(type);
        }

        private void OnRecreateLevelLoaded()
        {
            root.RemoveAllChildren(true);
            LevelComponent levelComponent;
#if UNITY_EDITOR
            levelComponent = LevelDebug.IsTest ? LevelDebug.LevelPrefab : LevelCoordinator.GetNextLevelLoaded(type);
#else
            levelComponent = LevelCoordinator.GetNextLevelLoaded(type.Name);
#endif
            foreach (var t in trackingStartLevels)
            {
                t.Track();
            }

            Instantiate(levelComponent, root, false);
        }

        public static void RegisterActionRecreateLevel(string id, Action action) { Dimensions[id].ReCreateLevelLoadedEvent += action; }
        public static void UnRegisterActionRecreateLevel(string id, Action action) { Dimensions[id].ReCreateLevelLoadedEvent -= action; }
        public static void RecreateLevelLoaded(string id) { Dimensions[id].RecreateLevelLoaded(); }
    }
}