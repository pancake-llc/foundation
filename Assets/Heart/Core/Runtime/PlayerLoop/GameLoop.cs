using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Pancake.PlayerLoop
{
    public static class GameLoop
    {
        public static ulong Frame { get; private set; }

#if UNITY_EDITOR
        private static readonly List<IEarlyUpdate> EarlyUpdates = new();
        private static readonly List<IFixedUpdate> FixedUpdates = new();
        private static readonly List<IPreUpdate> PreUpdates = new();
        private static readonly List<IUpdate> Updates = new();
        private static readonly List<IPreLateUpdate> PreLateUpdates = new();
        private static readonly List<IPostLateUpdate> PostLateUpdates = new();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Frame = 0;
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
#if UNITY_EDITOR
            CheckIntegrity(currentPlayerLoop);
            // Make sure we unregister before we do register.
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
            currentPlayerLoop.subSystemList[UpdateType.Update.ToIndex()].updateDelegate += FrameCounter;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

#if UNITY_EDITOR
        private static void CheckIntegrity(PlayerLoopSystem playerLoop)
        {
            for (var uType = UpdateType.EarlyUpdate; uType <= UpdateType.PostLateUpdate; uType++)
                Debug.Assert(playerLoop.subSystemList[uType.ToIndex()].type == uType.ToType(), $"Fatal Error: Unity player-loop incompatible ({uType})!");
        }

        private static void OnPlayModeState(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    var defPls = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
                    UnregisterAll(ref defPls);
                    UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(defPls);
                    EditorApplication.playModeStateChanged -= OnPlayModeState;
                    break;

                case PlayModeStateChange.EnteredPlayMode: break;
                case PlayModeStateChange.ExitingEditMode: break;
                case PlayModeStateChange.ExitingPlayMode: break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void UnregisterAll(ref PlayerLoopSystem playerLoop)
        {
            playerLoop.subSystemList[UpdateType.Update.ToIndex()].updateDelegate -= FrameCounter;
            foreach (var earlyUpdate in EarlyUpdates) playerLoop.subSystemList[UpdateType.EarlyUpdate.ToIndex()].updateDelegate -= earlyUpdate.OnEarlyUpdate;
            foreach (var fixedUpdate in FixedUpdates) playerLoop.subSystemList[UpdateType.FixedUpdate.ToIndex()].updateDelegate -= fixedUpdate.OnFixedUpdate;
            foreach (var preUpdate in PreUpdates) playerLoop.subSystemList[UpdateType.PreUpdate.ToIndex()].updateDelegate -= preUpdate.OnPreUpdate;
            foreach (var update in Updates) playerLoop.subSystemList[UpdateType.Update.ToIndex()].updateDelegate -= update.OnUpdate;
            foreach (var preLateUpdate in PreLateUpdates) playerLoop.subSystemList[UpdateType.PreLateUpdate.ToIndex()].updateDelegate -= preLateUpdate.OnPreLateUpdate;
            foreach (var postLateUpdate in PostLateUpdates)
                playerLoop.subSystemList[UpdateType.PostLateUpdate.ToIndex()].updateDelegate -= postLateUpdate.OnPostLateUpdate;
            EarlyUpdates.Clear();
            FixedUpdates.Clear();
            PreUpdates.Clear();
            Updates.Clear();
            PreLateUpdates.Clear();
            PostLateUpdates.Clear();
        }
#endif

        private static void FrameCounter() => Frame++;

        public static void AddListener(IEarlyUpdate client)
        {
#if UNITY_EDITOR
            EarlyUpdates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.EarlyUpdate.ToIndex()].updateDelegate += client.OnEarlyUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void AddListener(IFixedUpdate client)
        {
#if UNITY_EDITOR
            FixedUpdates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.FixedUpdate.ToIndex()].updateDelegate += client.OnFixedUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void AddListener(IPreUpdate client)
        {
#if UNITY_EDITOR
            PreUpdates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PreUpdate.ToIndex()].updateDelegate += client.OnPreUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void AddListener(IUpdate client)
        {
#if UNITY_EDITOR
            Updates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.Update.ToIndex()].updateDelegate += client.OnUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void AddListener(IPreLateUpdate client)
        {
#if UNITY_EDITOR
            PreLateUpdates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PreLateUpdate.ToIndex()].updateDelegate += client.OnPreLateUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void AddListener(IPostLateUpdate client)
        {
#if UNITY_EDITOR
            PostLateUpdates.Add(client);
#endif
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PostLateUpdate.ToIndex()].updateDelegate += client.OnPostLateUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public static void RemoveListener(IEarlyUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.EarlyUpdate.ToIndex()].updateDelegate -= client.OnEarlyUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            EarlyUpdates.Remove(client);
#endif
        }

        public static void RemoveListener(IFixedUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.FixedUpdate.ToIndex()].updateDelegate -= client.OnFixedUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            FixedUpdates.Remove(client);
#endif
        }

        public static void RemoveListener(IPreUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PreUpdate.ToIndex()].updateDelegate -= client.OnPreUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            PreUpdates.Remove(client);
#endif
        }

        public static void RemoveListener(IUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.Update.ToIndex()].updateDelegate -= client.OnUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            Updates.Remove(client);
#endif
        }

        public static void RemoveListener(IPreLateUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PreLateUpdate.ToIndex()].updateDelegate -= client.OnPreLateUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            PreLateUpdates.Remove(client);
#endif
        }

        public static void RemoveListener(IPostLateUpdate client)
        {
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            currentPlayerLoop.subSystemList[UpdateType.PostLateUpdate.ToIndex()].updateDelegate -= client.OnPostLateUpdate;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            PostLateUpdates.Remove(client);
#endif
        }
    }
}