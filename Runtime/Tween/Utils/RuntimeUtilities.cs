using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake.Tween
{
    /// <summary>
    /// RuntimeUtilities
    /// </summary>
    public struct RuntimeUtilities
    {
        private static GameObject _globalGameObject;

        public static event Action fixedUpdate;
        public static event Action waitForFixedUpdate;
        public static event Action update;
        public static event Action lateUpdate;
        public static event Action waitForEndOfFrame;

        public static event Action fixedUpdateOnce;
        public static event Action waitForFixedUpdateOnce;
        public static event Action updateOnce;
        public static event Action lateUpdateOnce;
        public static event Action waitForEndOfFrameOnce;

        /// <summary>
        /// An update callback can be used in edit-mode
        /// Note: works with unitedDeltaTime
        /// </summary>
        public static event Action unitedUpdate;

        /// <summary>
        /// An update callback (will be executed once) can be used in edit-mode
        /// </summary>
        public static event Action unitedUpdateOnce;

        /// <summary>
        /// Get deltaTime in unitedUpdate
        /// </summary>
        private static float UnitedDeltaTime
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return EditorUtilities.UnscaledDeltaTime * Time.timeScale;
                return Time.deltaTime;
#else
                    return Time.deltaTime;
#endif
            }
        }

        /// <summary>
        /// Get unscaled deltaTime in unitedUpdate
        /// </summary>
        private static float UnitedUnscaledDeltaTime
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return EditorUtilities.UnscaledDeltaTime;
                return Time.unscaledDeltaTime;
#else
                    return Time.unscaledDeltaTime;
#endif
            }
        }

        /// <summary>
        /// FixedUpdate times
        /// </summary>
        public static int fixedFrameCount { get; private set; }

        public static float GetUnitedDeltaTime(TimeMode mode) { return mode == TimeMode.Normal ? UnitedDeltaTime : UnitedUnscaledDeltaTime; }

        public static void AddUpdate(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    fixedUpdate += action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    waitForFixedUpdate += action;
                    return;
                case UpdateMode.Update:
                    update += action;
                    return;
                case UpdateMode.LateUpdate:
                    lateUpdate += action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    waitForEndOfFrame += action;
                    return;
            }
        }

        public static void RemoveUpdate(UpdateMode mode, Action action)
        {
            switch (mode)
            {
                case UpdateMode.FixedUpdate:
                    fixedUpdate -= action;
                    return;
                case UpdateMode.WaitForFixedUpdate:
                    waitForFixedUpdate -= action;
                    return;
                case UpdateMode.Update:
                    update -= action;
                    return;
                case UpdateMode.LateUpdate:
                    lateUpdate -= action;
                    return;
                case UpdateMode.WaitForEndOfFrame:
                    waitForEndOfFrame -= action;
                    return;
            }
        }
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _globalGameObject = new GameObject("GlobalGameObject");
            _globalGameObject.AddComponent<GlobalComponent>();
            _globalGameObject.hideFlags = HideFlags.HideInHierarchy;
            _globalGameObject.transform.Reset();
            UnityEngine.Object.DontDestroyOnLoad(_globalGameObject);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            UnityEditor.EditorApplication.update += () =>
            {
                if (!Application.isPlaying)
                {
                    unitedUpdate?.Invoke();

                    if (unitedUpdateOnce != null)
                    {
                        var call = unitedUpdateOnce;
                        unitedUpdateOnce = null;
                        call();
                    }
                }
            };
        }
#endif

        public class GlobalComponent : MonoBehaviour
        {
            private void Start()
            {
                StartCoroutine(WaitForFixedUpdate());
                StartCoroutine(WaitForEndOfFrame());

                IEnumerator WaitForFixedUpdate()
                {
                    var wait = new WaitForFixedUpdate();
                    while (true)
                    {
                        yield return wait;

                        fixedFrameCount += 1;

                        waitForFixedUpdate?.Invoke();

                        if (waitForFixedUpdateOnce != null)
                        {
                            var call = waitForFixedUpdateOnce;
                            waitForFixedUpdateOnce = null;
                            call();
                        }
                    }
                }

                IEnumerator WaitForEndOfFrame()
                {
                    var wait = new WaitForEndOfFrame();
                    while (true)
                    {
                        yield return wait;
                        waitForEndOfFrame?.Invoke();

                        if (waitForEndOfFrameOnce != null)
                        {
                            var call = waitForEndOfFrameOnce;
                            waitForEndOfFrameOnce = null;
                            call();
                        }
                    }
                }
            }

            private void FixedUpdate()
            {
                fixedUpdate?.Invoke();

                if (fixedUpdateOnce != null)
                {
                    var call = fixedUpdateOnce;
                    fixedUpdateOnce = null;
                    call();
                }
            }

            private void Update()
            {
                update?.Invoke();
                unitedUpdate?.Invoke();

                if (updateOnce != null)
                {
                    var call = updateOnce;
                    updateOnce = null;
                    call();
                }

                if (unitedUpdateOnce != null)
                {
                    var call = unitedUpdateOnce;
                    unitedUpdateOnce = null;
                    call();
                }
            }

            private void LateUpdate()
            {
                lateUpdate?.Invoke();

                if (lateUpdateOnce != null)
                {
                    var call = lateUpdateOnce;
                    lateUpdateOnce = null;
                    call();
                }
            }
        }
        
        
#if UNITY_EDITOR
        
    public struct EditorUtilities
    {
        private static float unscaledDeltaTime;
        private static double lastTimeSinceStartup = -0.01;
        
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += () =>
            {
                unscaledDeltaTime = (float) (EditorApplication.timeSinceStartup - lastTimeSinceStartup);
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            };
        }

        public static float UnscaledDeltaTime => unscaledDeltaTime;
    }
#endif
    }
}