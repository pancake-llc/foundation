using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Pancake.PlayerLoop
{
    public static class GameLoop
    {
        private static readonly PlayerLoopRunner[] LoopRunners = new PlayerLoopRunner[12];
        private static readonly Dictionary<object, PlayerLoopItem> Disposables = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Application.quitting += Quit;

            LoopRunners[(int) PlayerLoopTiming.PreTimeUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PostTimeUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PreInitialization] = new PlayerLoopRunner(true);
            LoopRunners[(int) PlayerLoopTiming.PostInitialization] = new PlayerLoopRunner(true);
            LoopRunners[(int) PlayerLoopTiming.PreStart] = new PlayerLoopRunner(true);
            LoopRunners[(int) PlayerLoopTiming.PostStart] = new PlayerLoopRunner(true);
            LoopRunners[(int) PlayerLoopTiming.PreFixedUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PostFixedUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PreUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PostUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PreLateUpdate] = new PlayerLoopRunner();
            LoopRunners[(int) PlayerLoopTiming.PostLateUpdate] = new PlayerLoopRunner();

            var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            var subSystemList = playerLoop.subSystemList.ToArray();

            int timeUpdateSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(TimeUpdate), subSystemList);
            int initializeSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(Initialization), subSystemList);
            int earlyUpdateSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(EarlyUpdate), subSystemList);
            int fixedUpdateSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(FixedUpdate), subSystemList);
            int updateSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(Update), subSystemList);
            int lateUpdateSystemIndex = PlayerLoopHelper.FindLoopSystemIndex(typeof(PreLateUpdate), subSystemList);

            ref var timeUpdateSystem = ref subSystemList[timeUpdateSystemIndex];
            ref var initializeSystem = ref subSystemList[initializeSystemIndex];
            ref var earlyUpdateSystem = ref subSystemList[earlyUpdateSystemIndex];
            ref var fixedUpdateSystem = ref subSystemList[fixedUpdateSystemIndex];
            ref var updateSystem = ref subSystemList[updateSystemIndex];
            ref var lateUpdateSystem = ref subSystemList[lateUpdateSystemIndex];

            var preTimeUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreTimeUpdate>(LoopRunners[(int) PlayerLoopTiming.PreTimeUpdate].Run);
            var postTimeUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostTimeUpdate>(LoopRunners[(int) PlayerLoopTiming.PostTimeUpdate].Run);
            var preInitializeSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreInitialization>(LoopRunners[(int) PlayerLoopTiming.PreInitialization].Run);
            var postInitializeSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostInitialization>(LoopRunners[(int) PlayerLoopTiming.PostInitialization].Run);
            var preStartSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreStart>(LoopRunners[(int) PlayerLoopTiming.PreStart].Run);
            var postStartSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostStart>(LoopRunners[(int) PlayerLoopTiming.PostStart].Run);
            var preFixedUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreFixedUpdate>(LoopRunners[(int) PlayerLoopTiming.PreFixedUpdate].Run);
            var postFixedUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostFixedUpdate>(LoopRunners[(int) PlayerLoopTiming.PostFixedUpdate].Run);
            var preUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreUpdate>(LoopRunners[(int) PlayerLoopTiming.PreUpdate].Run);
            var postUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostUpdate>(LoopRunners[(int) PlayerLoopTiming.PostUpdate].Run);
            var preLateUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPreLateUpdate>(LoopRunners[(int) PlayerLoopTiming.PreLateUpdate].Run);
            var postLateUpdateSystem = PlayerLoopHelper.CreateLoopSystem<GameLoopPostLateUpdate>(LoopRunners[(int) PlayerLoopTiming.PostLateUpdate].Run);

            PlayerLoopHelper.InsertSubSystem(ref timeUpdateSystem, 0, ref preTimeUpdateSystem);
            PlayerLoopHelper.AppendSubSystem(ref timeUpdateSystem, ref postTimeUpdateSystem);

            PlayerLoopHelper.InsertSubSystem(ref initializeSystem, 0, ref preInitializeSystem);
            PlayerLoopHelper.AppendSubSystem(ref initializeSystem, ref postInitializeSystem);

            PlayerLoopHelper.InsertSubSystem(ref earlyUpdateSystem,
                typeof(EarlyUpdate.ScriptRunDelayedStartupFrame),
                PlayerLoopHelper.InsertPosition.Before,
                ref preStartSystem);
            PlayerLoopHelper.InsertSubSystem(ref earlyUpdateSystem,
                typeof(EarlyUpdate.ScriptRunDelayedStartupFrame),
                PlayerLoopHelper.InsertPosition.After,
                ref postStartSystem);

            PlayerLoopHelper.InsertSubSystem(ref fixedUpdateSystem,
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                PlayerLoopHelper.InsertPosition.Before,
                ref preFixedUpdateSystem);
            PlayerLoopHelper.InsertSubSystem(ref fixedUpdateSystem,
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                PlayerLoopHelper.InsertPosition.After,
                ref postFixedUpdateSystem);

            PlayerLoopHelper.InsertSubSystem(ref updateSystem, typeof(Update.ScriptRunBehaviourUpdate), PlayerLoopHelper.InsertPosition.Before, ref preUpdateSystem);
            PlayerLoopHelper.InsertSubSystem(ref updateSystem, typeof(Update.ScriptRunBehaviourUpdate), PlayerLoopHelper.InsertPosition.After, ref postUpdateSystem);

            PlayerLoopHelper.InsertSubSystem(ref lateUpdateSystem,
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                PlayerLoopHelper.InsertPosition.Before,
                ref preLateUpdateSystem);
            PlayerLoopHelper.InsertSubSystem(ref lateUpdateSystem,
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                PlayerLoopHelper.InsertPosition.After,
                ref postLateUpdateSystem);

            playerLoop.subSystemList = subSystemList;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void Quit()
        {
            foreach (var loopRunner in LoopRunners)
            {
                loopRunner.Clear();
            }

            foreach (var disposable in Disposables.Values)
            {
                disposable.action?.Invoke();
            }

            Disposables.Clear();
        }

        public static void Register(Action action, PlayerLoopTiming timing)
        {
            LoopRunners[(int) timing].Register(action.Target, action);

            if (action.Target is IDisposable disposable)
            {
                if (!Disposables.ContainsKey(action.Target))
                {
                    var playerLoopItem = new PlayerLoopItem() {target = action.Target, action = disposable.Dispose};
                    Disposables.Add(playerLoopItem.target, playerLoopItem);
                }
            }
        }

        public static void Register(object target, Action action, PlayerLoopTiming timing)
        {
            LoopRunners[(int) timing].Register(target, action);

            if (target is IDisposable disposable)
            {
                if (!Disposables.ContainsKey(target))
                {
                    var playerLoopItem = new PlayerLoopItem() {target = target, action = disposable.Dispose};
                    Disposables.Add(playerLoopItem.target, playerLoopItem);
                }
            }
        }

        public static void Register(object target)
        {
            if (target is IInitialize initialize) Register(target, initialize.OnInitialize, PlayerLoopTiming.PreInitialization);
            if (target is IPostInitialize postInitialize) Register(target, postInitialize.OnPostInitialize, PlayerLoopTiming.PostInitialization);
            if (target is IStart start) Register(target, start.OnStartup, PlayerLoopTiming.PreStart);
            if (target is IPostStart postStart) Register(target, postStart.OnPostStartup, PlayerLoopTiming.PostStart);
            if (target is IFixedUpdate fixedUpdate) Register(target, fixedUpdate.OnFixedUpdate, PlayerLoopTiming.PreFixedUpdate);
            if (target is IPostFixedUpdate postFixedUpdate) Register(target, postFixedUpdate.OnPostFixedUpdate, PlayerLoopTiming.PostFixedUpdate);
            if (target is IUpdate update) Register(target, update.OnUpdate, PlayerLoopTiming.PreUpdate);
            if (target is IPostUpdate postUpdate) Register(target, postUpdate.OnPostUpdate, PlayerLoopTiming.PostUpdate);
            if (target is ILateUpdate lateUpdate) Register(target, lateUpdate.OnLateUpdate, PlayerLoopTiming.PreLateUpdate);
            if (target is IPostLateUpdate postLateUpdate) Register(target, postLateUpdate.OnPostLateUpdate, PlayerLoopTiming.PostLateUpdate);
        }

        public static void Unregister(object target, PlayerLoopTiming timing) { LoopRunners[(int) timing].Unregister(target); }

        public static void Unregister(object target)
        {
            foreach (var loopRunner in LoopRunners)
            {
                loopRunner.Unregister(target);
            }

            if (Disposables.TryGetValue(target, out var disposable))
            {
                disposable.action?.Invoke();
                Disposables.Remove(target);
            }
        }

        private struct GameLoopPreTimeUpdate
        {
        }

        private struct GameLoopPostTimeUpdate
        {
        }

        private struct GameLoopPreInitialization
        {
        }

        private struct GameLoopPostInitialization
        {
        }

        private struct GameLoopPreStart
        {
        }

        private struct GameLoopPostStart
        {
        }

        private struct GameLoopPreFixedUpdate
        {
        }

        private struct GameLoopPostFixedUpdate
        {
        }

        private struct GameLoopPreUpdate
        {
        }

        private struct GameLoopPostUpdate
        {
        }

        private struct GameLoopPreLateUpdate
        {
        }

        private struct GameLoopPostLateUpdate
        {
        }
    }
}