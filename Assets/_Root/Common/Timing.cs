using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

// /////////////////////////////////////////////////////////////////////////////////////////
//                              More Effective Coroutines
//                                        v3.11.0
// 
// This is an improved implementation of coroutines that boasts zero per-frame memory allocations,
// runs about twice as fast as Unity's built in coroutines, and has a range of extra features.
// 
// For manual, support, or upgrade guide visit http://trinary.tech/
// 
// Created by Teal Rogers
// Trinary Software
// All rights preserved
// trinaryllc@gmail.com
// /////////////////////////////////////////////////////////////////////////////////////////

namespace Pancake
{
    public class Timing : MonoBehaviour
    {
        /// <summary>
        /// The time between calls to SlowUpdate.
        /// </summary>
        [Tooltip("How quickly the SlowUpdate segment ticks.")]
        public float TimeBetweenSlowUpdateCalls = 1f / 7f;
        /// <summary>
        /// The amount that each coroutine should be seperated inside the Unity profiler. NOTE: When the profiler window
        /// is not open this value is ignored and all coroutines behave as if "None" is selected.
        /// </summary>
        [Tooltip("How much data should be sent to the profiler window when it's open.")]
        public DebugInfoType ProfilerDebugAmount;
        /// <summary>
        /// Whether the manual timeframe should automatically trigger during the update segment.
        /// </summary>
        [Tooltip("When using manual timeframe, should it run automatically after the update loop or only when TriggerManualTimframeUpdate is called.")]
        public bool AutoTriggerManualTimeframe = true;
        /// <summary>
        /// The number of coroutines that are being run in the Update segment.
        /// </summary>
        [Tooltip("A count of the number of Update coroutines that are currently running."), Space(12)]
        public int UpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the FixedUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of FixedUpdate coroutines that are currently running.")]
        public int FixedUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the LateUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of LateUpdate coroutines that are currently running.")]
        public int LateUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the SlowUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of SlowUpdate coroutines that are currently running.")]
        public int SlowUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the RealtimeUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of RealtimeUpdate coroutines that are currently running.")]
        public int RealtimeUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the EditorUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of EditorUpdate coroutines that are currently running.")]
        public int EditorUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the EditorSlowUpdate segment.
        /// </summary>
        [Tooltip("A count of the number of EditorSlowUpdate coroutines that are currently running.")]
        public int EditorSlowUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the EndOfFrame segment.
        /// </summary>
        [Tooltip("A count of the number of EndOfFrame coroutines that are currently running.")]
        public int EndOfFrameCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the ManualTimeframe segment.
        /// </summary>
        [Tooltip("A count of the number of ManualTimeframe coroutines that are currently running.")]
        public int ManualTimeframeCoroutines;

        /// <summary>
        /// The time in seconds that the current segment has been running.
        /// </summary>
        [System.NonSerialized]
        public float localTime;
        /// <summary>
        /// The time in seconds that the current segment has been running.
        /// </summary>
        public static float LocalTime { get { return Instance.localTime; } }
        /// <summary>
        /// The amount of time in fractional seconds that elapsed between this frame and the last frame.
        /// </summary>
        [System.NonSerialized]
        public float deltaTime;
        /// <summary>
        /// The amount of time in fractional seconds that elapsed between this frame and the last frame.
        /// </summary>
        public static float DeltaTime { get { return Instance.deltaTime; } }
        /// <summary>
        /// When defined, this function will be called every time manual timeframe needs to be set. The last manual timeframe time is passed in, and
        /// the new manual timeframe time needs to be returned. If this function is left as null, manual timeframe will be set to the current Time.time.
        /// </summary>
        public System.Func<float, float> SetManualTimeframeTime;
        /// <summary>
        /// Used for advanced coroutine control.
        /// </summary>
        public static System.Func<IEnumerator<float>, CoroutineHandle, IEnumerator<float>> ReplacementFunction;
        /// <summary>
        /// This event fires just before each segment is run.
        /// </summary>
        public static event System.Action OnPreExecute;
        /// <summary>
        /// You can use "yield return Timing.WaitForOneFrame;" inside a coroutine function to go to the next frame. 
        /// </summary>
        public const float WaitForOneFrame = float.NegativeInfinity;
        /// <summary>
        /// The main thread that (almost) everything in unity runs in.
        /// </summary>
        public static System.Threading.Thread MainThread { get; private set; }
        /// <summary>
        /// The handle of the current coroutine that is running.
        /// </summary>
        public static CoroutineHandle CurrentCoroutine
        {
            get
            {
                for (int i = 0; i < ActiveInstances.Length; i++)
                    if (ActiveInstances[i] != null && ActiveInstances[i].currentCoroutine.IsValid)
                        return ActiveInstances[i].currentCoroutine;
                return default(CoroutineHandle);
            }
        }
        /// <summary>
        /// The handle of the current coroutine that is running.
        /// </summary>
        public CoroutineHandle currentCoroutine { get; private set; }


        private static object _tmpRef;
        private static int _tmpInt;
        private static bool _tmpBool;
        private static Segment _tmpSegment;
        private static CoroutineHandle _tmpHandle;

        private int _currentUpdateFrame;
        private int _currentLateUpdateFrame;
        private int _currentSlowUpdateFrame;
        private int _currentRealtimeUpdateFrame;
        private int _currentEndOfFrameFrame;
        private int _nextUpdateProcessSlot;
        private int _nextLateUpdateProcessSlot;
        private int _nextFixedUpdateProcessSlot;
        private int _nextSlowUpdateProcessSlot;
        private int _nextRealtimeUpdateProcessSlot;
        private int _nextEditorUpdateProcessSlot;
        private int _nextEditorSlowUpdateProcessSlot;
        private int _nextEndOfFrameProcessSlot;
        private int _nextManualTimeframeProcessSlot;
        private int _lastUpdateProcessSlot;
        private int _lastLateUpdateProcessSlot;
        private int _lastFixedUpdateProcessSlot;
        private int _lastSlowUpdateProcessSlot;
        private int _lastRealtimeUpdateProcessSlot;
#if UNITY_EDITOR
        private int _lastEditorUpdateProcessSlot;
        private int _lastEditorSlowUpdateProcessSlot;
#endif
        private int _lastEndOfFrameProcessSlot;
        private int _lastManualTimeframeProcessSlot;
        private float _lastUpdateTime;
        private float _lastLateUpdateTime;
        private float _lastFixedUpdateTime;
        private float _lastSlowUpdateTime;
        private float _lastRealtimeUpdateTime;
#if UNITY_EDITOR
        private float _lastEditorUpdateTime;
        private float _lastEditorSlowUpdateTime;
#endif
        private float _lastEndOfFrameTime;
        private float _lastManualTimeframeTime;
        private float _lastSlowUpdateDeltaTime;
        private float _lastEditorUpdateDeltaTime;
        private float _lastEditorSlowUpdateDeltaTime;
        private float _lastManualTimeframeDeltaTime;
        private ushort _framesSinceUpdate;
        private ushort _expansions = 1;
        [SerializeField, HideInInspector]
        private byte _instanceID;
        private bool _EOFPumpRan;

        private static readonly Dictionary<CoroutineHandle, HashSet<CoroutineHandle>> Links = new Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>();
        private static readonly WaitForEndOfFrame EofWaitObject = new WaitForEndOfFrame();
        private readonly Dictionary<CoroutineHandle, HashSet<CoroutineHandle>> _waitingTriggers = new Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>();
        private readonly HashSet<CoroutineHandle> _allWaiting = new HashSet<CoroutineHandle>();
        private readonly Dictionary<CoroutineHandle, ProcessIndex> _handleToIndex = new Dictionary<CoroutineHandle, ProcessIndex>();
        private readonly Dictionary<ProcessIndex, CoroutineHandle> _indexToHandle = new Dictionary<ProcessIndex, CoroutineHandle>();
        private readonly Dictionary<CoroutineHandle, string> _processTags = new Dictionary<CoroutineHandle, string>();
        private readonly Dictionary<string, HashSet<CoroutineHandle>> _taggedProcesses = new Dictionary<string, HashSet<CoroutineHandle>>();
        private readonly Dictionary<CoroutineHandle, int> _processLayers = new Dictionary<CoroutineHandle, int>();
        private readonly Dictionary<int, HashSet<CoroutineHandle>> _layeredProcesses = new Dictionary<int, HashSet<CoroutineHandle>>();

        private IEnumerator<float>[] UpdateProcesses = new IEnumerator<float>[InitialBufferSizeLarge];
        private IEnumerator<float>[] LateUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] FixedUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
        private IEnumerator<float>[] SlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
        private IEnumerator<float>[] RealtimeUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] EditorUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] EditorSlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] EndOfFrameProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] ManualTimeframeProcesses = new IEnumerator<float>[InitialBufferSizeSmall];

        private bool[] UpdatePaused = new bool[InitialBufferSizeLarge];
        private bool[] LateUpdatePaused = new bool[InitialBufferSizeSmall];
        private bool[] FixedUpdatePaused = new bool[InitialBufferSizeMedium];
        private bool[] SlowUpdatePaused = new bool[InitialBufferSizeMedium];
        private bool[] RealtimeUpdatePaused = new bool[InitialBufferSizeSmall];
        private bool[] EditorUpdatePaused = new bool[InitialBufferSizeSmall];
        private bool[] EditorSlowUpdatePaused = new bool[InitialBufferSizeSmall];
        private bool[] EndOfFramePaused = new bool[InitialBufferSizeSmall];
        private bool[] ManualTimeframePaused = new bool[InitialBufferSizeSmall];

        private bool[] UpdateHeld = new bool[InitialBufferSizeLarge];
        private bool[] LateUpdateHeld = new bool[InitialBufferSizeSmall];
        private bool[] FixedUpdateHeld = new bool[InitialBufferSizeMedium];
        private bool[] SlowUpdateHeld = new bool[InitialBufferSizeMedium];
        private bool[] RealtimeUpdateHeld = new bool[InitialBufferSizeSmall];
        private bool[] EditorUpdateHeld = new bool[InitialBufferSizeSmall];
        private bool[] EditorSlowUpdateHeld = new bool[InitialBufferSizeSmall];
        private bool[] EndOfFrameHeld = new bool[InitialBufferSizeSmall];
        private bool[] ManualTimeframeHeld = new bool[InitialBufferSizeSmall];

        private CoroutineHandle _eofWatcherHandle;
        private const ushort FramesUntilMaintenance = 64;
        private const int ProcessArrayChunkSize = 64;
        private const int InitialBufferSizeLarge = 256;
        private const int InitialBufferSizeMedium = 64;
        private const int InitialBufferSizeSmall = 8;


        private static Timing[] ActiveInstances = new Timing[16];
        private static Timing _instance;
        public static Timing Instance
        {
            get
            {
                if (_instance == null || !_instance.gameObject)
                {
                    GameObject instanceHome = GameObject.Find("Timing Controller");

                    if (instanceHome == null)
                    {
                        instanceHome = new GameObject { name = "Timing Controller" };

#if UNITY_EDITOR
                        if (Application.isPlaying)
                            DontDestroyOnLoad(instanceHome);
#else
                        DontDestroyOnLoad(instanceHome);
#endif
                    }

                    _instance = instanceHome.GetComponent<Timing>() ?? instanceHome.AddComponent<Timing>();

                    _instance.InitializeInstanceID();
                }

                return _instance;
            }

            set { _instance = value; }
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        void OnEnable()
        {
            if (MainThread == null)
                MainThread = System.Threading.Thread.CurrentThread;

            if (_nextEditorUpdateProcessSlot > 0 || _nextEditorSlowUpdateProcessSlot > 0)
                OnEditorStart();

            InitializeInstanceID();

            if (_nextEndOfFrameProcessSlot > 0)
                RunCoroutineSingletonOnInstance(_EOFPumpWatcher(), "MEC_EOFPumpWatcher", SingletonBehavior.Abort);
        }

        void OnDisable()
        {
            if (_instanceID < ActiveInstances.Length)
                ActiveInstances[_instanceID] = null;
        }

        private void InitializeInstanceID()
        {
            if (ActiveInstances[_instanceID] == null)
            {
                if (_instanceID == 0x00)
                    _instanceID++;

                for (; _instanceID <= 0x10; _instanceID++)
                {
                    if (_instanceID == 0x10)
                    {
                        GameObject.Destroy(gameObject);
                        throw new System.OverflowException("You are only allowed 15 different contexts for MEC to run inside at one time.");
                    }

                    if (ActiveInstances[_instanceID] == null)
                    {
                        ActiveInstances[_instanceID] = this;
                        break;
                    }
                }
            }
        }

        void Update()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (_lastSlowUpdateTime + TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup && _nextSlowUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.SlowUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastSlowUpdateProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!SlowUpdatePaused[coindex.i] && !SlowUpdateHeld[coindex.i] && SlowUpdateProcesses[coindex.i] != null && !(localTime < SlowUpdateProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine (Slow Update), " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine (Slow Update)");
                            }

                            if (!SlowUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (SlowUpdateProcesses[coindex.i] != null && float.IsNaN(SlowUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    SlowUpdateProcesses[coindex.i] = ReplacementFunction(SlowUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.SlowUpdate);");
                    }
                }
            }

            if (_nextRealtimeUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.RealtimeUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastRealtimeUpdateProcessSlot = _nextRealtimeUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastRealtimeUpdateProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!RealtimeUpdatePaused[coindex.i] && !RealtimeUpdateHeld[coindex.i] && RealtimeUpdateProcesses[coindex.i] != null && !(localTime < RealtimeUpdateProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine (Realtime Update), " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine (Realtime Update)");
                            }

                            if (!RealtimeUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (RealtimeUpdateProcesses[coindex.i] != null && float.IsNaN(RealtimeUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    RealtimeUpdateProcesses[coindex.i] = ReplacementFunction(RealtimeUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.RealtimeUpdate);");
                    }
                }
            }

            if (_nextUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.Update };
                if (UpdateTimeValues(coindex.seg))
                    _lastUpdateProcessSlot = _nextUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastUpdateProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!UpdatePaused[coindex.i] && !UpdateHeld[coindex.i] && UpdateProcesses[coindex.i] != null && !(localTime < UpdateProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine, " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine");
                            }

                            if (!UpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (UpdateProcesses[coindex.i] != null && float.IsNaN(UpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    UpdateProcesses[coindex.i] = ReplacementFunction(UpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.Update);");
                    }
                }
            }

            if (AutoTriggerManualTimeframe)
            {
                TriggerManualTimeframeUpdate();
            }
            else
            {
                if (++_framesSinceUpdate > FramesUntilMaintenance)
                {
                    _framesSinceUpdate = 0;

                    if (ProfilerDebugAmount != DebugInfoType.None)
                        Profiler.BeginSample("Maintenance Task");

                    RemoveUnused();

                    if (ProfilerDebugAmount != DebugInfoType.None)
                        Profiler.EndSample();
                }
            }

            currentCoroutine = default(CoroutineHandle);

        }

        void FixedUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (_nextFixedUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.FixedUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastFixedUpdateProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!FixedUpdatePaused[coindex.i] && !FixedUpdateHeld[coindex.i] && FixedUpdateProcesses[coindex.i] != null && !(localTime < FixedUpdateProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine, " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine");
                            }

                            if (!FixedUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (FixedUpdateProcesses[coindex.i] != null && float.IsNaN(FixedUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    FixedUpdateProcesses[coindex.i] = ReplacementFunction(FixedUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.FixedUpdate);");
                    }
                }

                currentCoroutine = default(CoroutineHandle);
            }
        }

        void LateUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (_nextLateUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.LateUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastLateUpdateProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!LateUpdatePaused[coindex.i] && !LateUpdateHeld[coindex.i] && LateUpdateProcesses[coindex.i] != null && !(localTime < LateUpdateProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine, " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine");
                            }

                            if (!LateUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (LateUpdateProcesses[coindex.i] != null && float.IsNaN(LateUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    LateUpdateProcesses[coindex.i] = ReplacementFunction(LateUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.LateUpdate);");
                    }
                }

                currentCoroutine = default(CoroutineHandle);
            }
        }

        /// <summary>
        /// This will trigger an update in the manual timeframe segment. If the AutoTriggerManualTimeframeDuringUpdate variable is set to true
        /// then this function will be automitically called every Update, so you would normally want to set that variable to false before
        /// calling this function yourself.
        /// </summary>
        public void TriggerManualTimeframeUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (_nextManualTimeframeProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.ManualTimeframe };
                if (UpdateTimeValues(coindex.seg))
                    _lastManualTimeframeProcessSlot = _nextManualTimeframeProcessSlot;

                for (coindex.i = 0; coindex.i < _lastManualTimeframeProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!ManualTimeframePaused[coindex.i] && !ManualTimeframeHeld[coindex.i] && ManualTimeframeProcesses[coindex.i] != null &&
                            !(localTime < ManualTimeframeProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine (Manual Timeframe), " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine (Manual Timeframe)");
                            }

                            if (!ManualTimeframeProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (ManualTimeframeProcesses[coindex.i] != null && float.IsNaN(ManualTimeframeProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    ManualTimeframeProcesses[coindex.i] = ReplacementFunction(ManualTimeframeProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.ManualTimeframe);");
                    }
                }
            }

            if (++_framesSinceUpdate > FramesUntilMaintenance)
            {
                _framesSinceUpdate = 0;

                if (ProfilerDebugAmount != DebugInfoType.None)
                    Profiler.BeginSample("Maintenance Task");

                RemoveUnused();

                if (ProfilerDebugAmount != DebugInfoType.None)
                    Profiler.EndSample();
            }

            currentCoroutine = default(CoroutineHandle);
        }

        private bool OnEditorStart()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return false;

            if (_lastEditorUpdateTime < 0.001)
                _lastEditorUpdateTime = (float)EditorApplication.timeSinceStartup;

            if (ActiveInstances[_instanceID] == null)
                OnEnable();

            EditorApplication.update -= OnEditorUpdate;

            EditorApplication.update += OnEditorUpdate;

            return true;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private void OnEditorUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                for (int i = 0; i < _nextEditorUpdateProcessSlot; i++)
                    EditorUpdateProcesses[i] = null;
                _nextEditorUpdateProcessSlot = 0;
                for (int i = 0; i < _nextEditorSlowUpdateProcessSlot; i++)
                    EditorSlowUpdateProcesses[i] = null;
                _nextEditorSlowUpdateProcessSlot = 0;

                EditorApplication.update -= OnEditorUpdate;
                _instance = null;
            }

            if (_lastEditorSlowUpdateTime + TimeBetweenSlowUpdateCalls < EditorApplication.timeSinceStartup && _nextEditorSlowUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.EditorSlowUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastEditorSlowUpdateProcessSlot = _nextEditorSlowUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastEditorSlowUpdateProcessSlot; coindex.i++)
                {
                    currentCoroutine = _indexToHandle[coindex];

                    try
                    {
                        if (!EditorSlowUpdatePaused[coindex.i] && !EditorSlowUpdateHeld[coindex.i] && EditorSlowUpdateProcesses[coindex.i] != null &&
                            !(EditorApplication.timeSinceStartup < EditorSlowUpdateProcesses[coindex.i].Current))
                        {
                            if (!EditorSlowUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (EditorSlowUpdateProcesses[coindex.i] != null && float.IsNaN(EditorSlowUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    EditorSlowUpdateProcesses[coindex.i] = ReplacementFunction(EditorSlowUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.EditorUpdate);");
                    }
                }
            }

            if (_nextEditorUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.EditorUpdate };
                if (UpdateTimeValues(coindex.seg))
                    _lastEditorUpdateProcessSlot = _nextEditorUpdateProcessSlot;

                for (coindex.i = 0; coindex.i < _lastEditorUpdateProcessSlot; coindex.i++)
                {
                    currentCoroutine = _indexToHandle[coindex];

                    try
                    {
                        if (!EditorUpdatePaused[coindex.i] && !EditorUpdateHeld[coindex.i] && EditorUpdateProcesses[coindex.i] != null &&
                            !(EditorApplication.timeSinceStartup < EditorUpdateProcesses[coindex.i].Current))
                        {
                            if (!EditorUpdateProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (EditorUpdateProcesses[coindex.i] != null && float.IsNaN(EditorUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    EditorUpdateProcesses[coindex.i] = ReplacementFunction(EditorUpdateProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.EditorUpdate);");
                    }
                }
            }

            if (++_framesSinceUpdate > FramesUntilMaintenance)
            {
                _framesSinceUpdate = 0;

                EditorRemoveUnused();
            }

            currentCoroutine = default(CoroutineHandle);
        }
#endif

        private IEnumerator<float> _EOFPumpWatcher()
        {
            while (_nextEndOfFrameProcessSlot > 0)
            {
                if (!_EOFPumpRan)
                    base.StartCoroutine(_EOFPump());

                _EOFPumpRan = false;

                yield return WaitForOneFrame;
            }

            _EOFPumpRan = false;
        }

        private System.Collections.IEnumerator _EOFPump()
        {
            while (_nextEndOfFrameProcessSlot > 0)
            {
                yield return EofWaitObject;

                if (OnPreExecute != null)
                    OnPreExecute();

                ProcessIndex coindex = new ProcessIndex { seg = Segment.EndOfFrame };
                _EOFPumpRan = true;
                if (UpdateTimeValues(coindex.seg))
                    _lastEndOfFrameProcessSlot = _nextEndOfFrameProcessSlot;

                for (coindex.i = 0; coindex.i < _lastEndOfFrameProcessSlot; coindex.i++)
                {
                    try
                    {
                        if (!EndOfFramePaused[coindex.i] && !EndOfFrameHeld[coindex.i] && EndOfFrameProcesses[coindex.i] != null && !(localTime < EndOfFrameProcesses[coindex.i].Current))
                        {
                            currentCoroutine = _indexToHandle[coindex];

                            if (ProfilerDebugAmount != DebugInfoType.None && _indexToHandle.ContainsKey(coindex))
                            {
                                Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags ? ("Processing Coroutine, " +
                                        (_processLayers.ContainsKey(_indexToHandle[coindex]) ? "layer " + _processLayers[_indexToHandle[coindex]] : "no layer") +
                                        (_processTags.ContainsKey(_indexToHandle[coindex]) ? ", tag " + _processTags[_indexToHandle[coindex]] : ", no tag"))
                                        : "Processing Coroutine");
                            }

                            if (!EndOfFrameProcesses[coindex.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(coindex))
                                    KillCoroutinesOnInstance(_indexToHandle[coindex]);
                            }
                            else if (EndOfFrameProcesses[coindex.i] != null && float.IsNaN(EndOfFrameProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    EndOfFrameProcesses[coindex.i] = ReplacementFunction(EndOfFrameProcesses[coindex.i], _indexToHandle[coindex]);
                                    ReplacementFunction = null;
                                }
                                coindex.i--;
                            }

                            if (ProfilerDebugAmount != DebugInfoType.None)
                                Profiler.EndSample();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);

                        if (ex is MissingReferenceException)
                            Debug.LogError("This exception can probably be fixed by adding \"CancelWith(gameObject)\" when you run the coroutine.\n"
                                + "Example: Timing.RunCoroutine(_foo().CancelWith(gameObject), Segment.EndOfFrame);");
                    }
                }
            }

            currentCoroutine = default(CoroutineHandle);
        }

        private void RemoveUnused()
        {
            var waitTrigsEnum = _waitingTriggers.GetEnumerator();
            while (waitTrigsEnum.MoveNext())
            {
                if (waitTrigsEnum.Current.Value.Count == 0)
                {
                    _waitingTriggers.Remove(waitTrigsEnum.Current.Key);
                    waitTrigsEnum = _waitingTriggers.GetEnumerator();
                    continue;
                }

                if (_handleToIndex.ContainsKey(waitTrigsEnum.Current.Key) && CoindexIsNull(_handleToIndex[waitTrigsEnum.Current.Key]))
                {
                    CloseWaitingProcess(waitTrigsEnum.Current.Key);
                    waitTrigsEnum = _waitingTriggers.GetEnumerator();
                }
            }

            ProcessIndex outer, inner;
            outer.seg = inner.seg = Segment.Update;
            for (outer.i = inner.i = 0; outer.i < _nextUpdateProcessSlot; outer.i++)
            {
                if (UpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        UpdateProcesses[inner.i] = UpdateProcesses[outer.i];
                        UpdatePaused[inner.i] = UpdatePaused[outer.i];
                        UpdateHeld[inner.i] = UpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextUpdateProcessSlot; outer.i++)
            {
                UpdateProcesses[outer.i] = null;
                UpdatePaused[outer.i] = false;
                UpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);
                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            UpdateCoroutines = _nextUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.FixedUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextFixedUpdateProcessSlot; outer.i++)
            {
                if (FixedUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        FixedUpdateProcesses[inner.i] = FixedUpdateProcesses[outer.i];
                        FixedUpdatePaused[inner.i] = FixedUpdatePaused[outer.i];
                        FixedUpdateHeld[inner.i] = FixedUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextFixedUpdateProcessSlot; outer.i++)
            {
                FixedUpdateProcesses[outer.i] = null;
                FixedUpdatePaused[outer.i] = false;
                FixedUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            FixedUpdateCoroutines = _nextFixedUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.LateUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextLateUpdateProcessSlot; outer.i++)
            {
                if (LateUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        LateUpdateProcesses[inner.i] = LateUpdateProcesses[outer.i];
                        LateUpdatePaused[inner.i] = LateUpdatePaused[outer.i];
                        LateUpdateHeld[inner.i] = LateUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextLateUpdateProcessSlot; outer.i++)
            {
                LateUpdateProcesses[outer.i] = null;
                LateUpdatePaused[outer.i] = false;
                LateUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            LateUpdateCoroutines = _nextLateUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.SlowUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextSlowUpdateProcessSlot; outer.i++)
            {
                if (SlowUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        SlowUpdateProcesses[inner.i] = SlowUpdateProcesses[outer.i];
                        SlowUpdatePaused[inner.i] = SlowUpdatePaused[outer.i];
                        SlowUpdateHeld[inner.i] = SlowUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextSlowUpdateProcessSlot; outer.i++)
            {
                SlowUpdateProcesses[outer.i] = null;
                SlowUpdatePaused[outer.i] = false;
                SlowUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            SlowUpdateCoroutines = _nextSlowUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.RealtimeUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextRealtimeUpdateProcessSlot; outer.i++)
            {
                if (RealtimeUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        RealtimeUpdateProcesses[inner.i] = RealtimeUpdateProcesses[outer.i];
                        RealtimeUpdatePaused[inner.i] = RealtimeUpdatePaused[outer.i];
                        RealtimeUpdateHeld[inner.i] = RealtimeUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextRealtimeUpdateProcessSlot; outer.i++)
            {
                RealtimeUpdateProcesses[outer.i] = null;
                RealtimeUpdatePaused[outer.i] = false;
                RealtimeUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            RealtimeUpdateCoroutines = _nextRealtimeUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.EndOfFrame;
            for (outer.i = inner.i = 0; outer.i < _nextEndOfFrameProcessSlot; outer.i++)
            {
                if (EndOfFrameProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        EndOfFrameProcesses[inner.i] = EndOfFrameProcesses[outer.i];
                        EndOfFramePaused[inner.i] = EndOfFramePaused[outer.i];
                        EndOfFrameHeld[inner.i] = EndOfFrameHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextEndOfFrameProcessSlot; outer.i++)
            {
                EndOfFrameProcesses[outer.i] = null;
                EndOfFramePaused[outer.i] = false;
                EndOfFrameHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            EndOfFrameCoroutines = _nextEndOfFrameProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.ManualTimeframe;
            for (outer.i = inner.i = 0; outer.i < _nextManualTimeframeProcessSlot; outer.i++)
            {
                if (ManualTimeframeProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        ManualTimeframeProcesses[inner.i] = ManualTimeframeProcesses[outer.i];
                        ManualTimeframePaused[inner.i] = ManualTimeframePaused[outer.i];
                        ManualTimeframeHeld[inner.i] = ManualTimeframeHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextManualTimeframeProcessSlot; outer.i++)
            {
                ManualTimeframeProcesses[outer.i] = null;
                ManualTimeframePaused[outer.i] = false;
                ManualTimeframeHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            ManualTimeframeCoroutines = _nextManualTimeframeProcessSlot = inner.i;
        }

        private void EditorRemoveUnused()
        {
            var waitTrigsEnum = _waitingTriggers.GetEnumerator();
            while (waitTrigsEnum.MoveNext())
            {
                if (_handleToIndex.ContainsKey(waitTrigsEnum.Current.Key) && CoindexIsNull(_handleToIndex[waitTrigsEnum.Current.Key]))
                {
                    CloseWaitingProcess(waitTrigsEnum.Current.Key);
                    waitTrigsEnum = _waitingTriggers.GetEnumerator();
                }
            }

            ProcessIndex outer, inner;
            outer.seg = inner.seg = Segment.EditorUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextEditorUpdateProcessSlot; outer.i++)
            {
                if (EditorUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        EditorUpdateProcesses[inner.i] = EditorUpdateProcesses[outer.i];
                        EditorUpdatePaused[inner.i] = EditorUpdatePaused[outer.i];
                        EditorUpdateHeld[inner.i] = EditorUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextEditorUpdateProcessSlot; outer.i++)
            {
                EditorUpdateProcesses[outer.i] = null;
                EditorUpdatePaused[outer.i] = false;
                EditorUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            EditorUpdateCoroutines = _nextEditorUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.EditorSlowUpdate;
            for (outer.i = inner.i = 0; outer.i < _nextEditorSlowUpdateProcessSlot; outer.i++)
            {
                if (EditorSlowUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        EditorSlowUpdateProcesses[inner.i] = EditorSlowUpdateProcesses[outer.i];
                        EditorUpdatePaused[inner.i] = EditorUpdatePaused[outer.i];
                        EditorUpdateHeld[inner.i] = EditorUpdateHeld[outer.i];

                        if (_indexToHandle.ContainsKey(inner))
                        {
                            RemoveGraffiti(_indexToHandle[inner]);
                            _handleToIndex.Remove(_indexToHandle[inner]);
                            _indexToHandle.Remove(inner);
                        }

                        _handleToIndex[_indexToHandle[outer]] = inner;
                        _indexToHandle.Add(inner, _indexToHandle[outer]);
                        _indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < _nextEditorSlowUpdateProcessSlot; outer.i++)
            {
                EditorSlowUpdateProcesses[outer.i] = null;
                EditorSlowUpdatePaused[outer.i] = false;
                EditorSlowUpdateHeld[outer.i] = false;
                if (_indexToHandle.ContainsKey(outer))
                {
                    RemoveGraffiti(_indexToHandle[outer]);

                    _handleToIndex.Remove(_indexToHandle[outer]);
                    _indexToHandle.Remove(outer);
                }
            }

            EditorSlowUpdateCoroutines = _nextEditorSlowUpdateProcessSlot = inner.i;
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, GameObject gameObj)
        {
            return coroutine == null ? new CoroutineHandle() : Instance.RunCoroutineInternal(coroutine, Segment.Update,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, int layer)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, GameObject gameObj, string tag)
        {
            return coroutine == null ? new CoroutineHandle() : Instance.RunCoroutineInternal(coroutine, Segment.Update,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, int layer, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, segment, 0, false, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, GameObject gameObj)
        {
            return coroutine == null ? new CoroutineHandle() : Instance.RunCoroutineInternal(coroutine, segment,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, int layer)
        {
            return coroutine == null ? new CoroutineHandle()
                 : Instance.RunCoroutineInternal(coroutine, segment, layer, true, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : Instance.RunCoroutineInternal(coroutine, segment, 0, false, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag)
        {
            return coroutine == null ? new CoroutineHandle() : Instance.RunCoroutineInternal(coroutine, segment,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, int layer, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : Instance.RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, 0, false, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, GameObject gameObj)
        {
            return coroutine == null ? new CoroutineHandle() : RunCoroutineInternal(coroutine, Segment.Update,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, int layer)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, layer, true, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, GameObject gameObj, string tag)
        {
            return coroutine == null ? new CoroutineHandle() : RunCoroutineInternal(coroutine, Segment.Update,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, int layer, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, segment, 0, false, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj)
        {
            return coroutine == null ? new CoroutineHandle() : RunCoroutineInternal(coroutine, segment,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, int layer)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, segment, layer, true, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, segment, 0, false, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag)
        {
            return coroutine == null ? new CoroutineHandle() : RunCoroutineInternal(coroutine, segment,
                gameObj == null ? 0 : gameObj.GetInstanceID(), gameObj != null, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                : RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment, but not while the coroutine with the supplied handle is running.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="handle">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, CoroutineHandle handle, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(handle);
            }
            else if (IsRunning(handle))
            {
                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                        return handle;
                    case SingletonBehavior.AbortAndUnpause:
                        ResumeCoroutines(handle);
                        return handle;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, null,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, handle, false);
                        return newCoroutineHandle;

                }
            }

            return Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, GameObject gameObj, SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutine(coroutine) : RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, int layer, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(layer);
            }
            else if (Instance._layeredProcesses.ContainsKey(layer))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    _instance.ResumeCoroutinesOnInstance(_instance._layeredProcesses[layer]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = Instance._layeredProcesses[layer].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, null,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _instance._layeredProcesses[layer], false);
                        return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(tag);
            }
            else if (Instance._taggedProcesses.ContainsKey(tag))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    _instance.ResumeCoroutinesOnInstance(_instance._taggedProcesses[tag]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = Instance._taggedProcesses[tag].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _instance._taggedProcesses[tag], false);
                        return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineSingleton(coroutine, tag, behaviorOnCollision)
                : RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, int layer, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(layer, tag);
                return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
            }

            if (!Instance._taggedProcesses.ContainsKey(tag) || !Instance._layeredProcesses.ContainsKey(layer))
                return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);

            if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                ResumeCoroutines(layer, tag);
            
            if (behaviorOnCollision == SingletonBehavior.Abort || behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
            {
                var matchesEnum = Instance._taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_instance._processLayers.ContainsKey(matchesEnum.Current) && _instance._processLayers[matchesEnum.Current] == layer)
                        return matchesEnum.Current;
            }

            if (behaviorOnCollision == SingletonBehavior.Wait)
            {
                List<CoroutineHandle> matches = new List<CoroutineHandle>();
                var matchesEnum = Instance._taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (Instance._processLayers.ContainsKey(matchesEnum.Current) && Instance._processLayers[matchesEnum.Current] == layer)
                        matches.Add(matchesEnum.Current);

                if (matches.Count > 0)
                {
                    CoroutineHandle newCoroutineHandle = _instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag,
                         new CoroutineHandle(_instance._instanceID), false);
                    WaitForOtherHandles(newCoroutineHandle, matches, false);
                    return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine, but not while the coroutine with the supplied handle is running.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="handle">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, CoroutineHandle handle, Segment segment,
            SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(handle);
            }
            else if (IsRunning(handle))
            {
                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                        return handle;
                    case SingletonBehavior.AbortAndUnpause:
                        ResumeCoroutines(handle);
                        return handle;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, 0, false, null,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, handle, false);
                        return newCoroutineHandle;

                }
            }

            return Instance.RunCoroutineInternal(coroutine, segment, 0, false, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, GameObject gameObj,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutine(coroutine, segment) : RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, int layer, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(layer);
            }
            else if (Instance._layeredProcesses.ContainsKey(layer))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    _instance.ResumeCoroutinesOnInstance(_instance._layeredProcesses[layer]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = Instance._layeredProcesses[layer].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, layer, true, null,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _instance._layeredProcesses[layer], false);
                        return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, segment, layer, true, null, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null)
                return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(tag);
            }
            else if (Instance._taggedProcesses.ContainsKey(tag))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    _instance.ResumeCoroutinesOnInstance(_instance._taggedProcesses[tag]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = Instance._taggedProcesses[tag].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = Instance.RunCoroutineInternal(coroutine, segment, 0, false, tag,
                            new CoroutineHandle(Instance._instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _instance._taggedProcesses[tag], false);
                        return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, segment, 0, false, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied graffitti unless there is already one or more coroutines running with both that tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineSingleton(coroutine, segment, tag, behaviorOnCollision)
                : RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied graffitti unless there is already one or more coroutines running with both that tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(IEnumerator<float> coroutine, Segment segment, int layer, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutines(layer, tag);
                return Instance.RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
            }

            if (!Instance._taggedProcesses.ContainsKey(tag) || !Instance._layeredProcesses.ContainsKey(layer))
                return Instance.RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);

            if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                ResumeCoroutines(layer, tag);

            if (behaviorOnCollision == SingletonBehavior.Abort || behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
            {
                var matchesEnum = Instance._taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_instance._processLayers.ContainsKey(matchesEnum.Current) && _instance._processLayers[matchesEnum.Current] == layer)
                        return matchesEnum.Current;
            }
            else if (behaviorOnCollision == SingletonBehavior.Wait)
            {
                List<CoroutineHandle> matches = new List<CoroutineHandle>();
                var matchesEnum = Instance._taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_instance._processLayers.ContainsKey(matchesEnum.Current) && _instance._processLayers[matchesEnum.Current] == layer)
                        matches.Add(matchesEnum.Current);

                if (matches.Count > 0)
                {
                    CoroutineHandle newCoroutineHandle = _instance.RunCoroutineInternal(coroutine, segment, layer, true, tag,
                        new CoroutineHandle(_instance._instanceID), false);
                    WaitForOtherHandles(newCoroutineHandle, matches, false);
                    return newCoroutineHandle;
                }
            }

            return Instance.RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(Instance._instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment, but not while the coroutine with the supplied handle is running.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="handle">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, CoroutineHandle handle, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(handle);
            }
            else if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]))
            {
                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                        return handle;
                    case SingletonBehavior.AbortAndUnpause:
                        ResumeCoroutinesOnInstance(handle);
                        return handle;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, 0, false, null,
                            new CoroutineHandle(_instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, handle, false);
                        return newCoroutineHandle;

                }
            }

            return RunCoroutineInternal(coroutine, Segment.Update, 0, false, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, GameObject gameObj, SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineOnInstance(coroutine)
                : RunCoroutineSingletonOnInstance(coroutine, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, int layer, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(layer);
            }
            else if (_layeredProcesses.ContainsKey(layer))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    ResumeCoroutinesOnInstance(_layeredProcesses[layer]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = _layeredProcesses[layer].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, layer, true, null,
                            new CoroutineHandle(_instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _layeredProcesses[layer], false);
                        return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, Segment.Update, layer, true, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(tag);
            }
            else if (_taggedProcesses.ContainsKey(tag))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    ResumeCoroutinesOnInstance(_taggedProcesses[tag]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = _taggedProcesses[tag].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag,
                            new CoroutineHandle(_instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _taggedProcesses[tag], false);
                        return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, Segment.Update, 0, false, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, GameObject gameObj, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineSingletonOnInstance(coroutine, tag, behaviorOnCollision)
                : RunCoroutineSingletonOnInstance(coroutine, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, int layer, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(layer, tag);
                return RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(_instanceID), true);
            }

            if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
                return RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(_instanceID), true);

            if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                ResumeCoroutinesOnInstance(layer, tag);

            if (behaviorOnCollision == SingletonBehavior.Abort || behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
            {
                var matchesEnum = _taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_processLayers.ContainsKey(matchesEnum.Current) && _processLayers[matchesEnum.Current] == layer)
                        return matchesEnum.Current;
            }

            if (behaviorOnCollision == SingletonBehavior.Wait)
            {
                List<CoroutineHandle> matches = new List<CoroutineHandle>();
                var matchesEnum = _taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_processLayers.ContainsKey(matchesEnum.Current) && _processLayers[matchesEnum.Current] == layer)
                        matches.Add(matchesEnum.Current);

                if (matches.Count > 0)
                {
                    CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(_instanceID), false);
                    WaitForOtherHandles(newCoroutineHandle, matches, false);
                    return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, Segment.Update, layer, true, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineOnInstance(coroutine, segment)
                : RunCoroutineSingletonOnInstance(coroutine, segment, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(layer);
            }
            else if (_layeredProcesses.ContainsKey(layer))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    ResumeCoroutinesOnInstance(_layeredProcesses[layer]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = _layeredProcesses[layer].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, segment, layer, true, null,
                            new CoroutineHandle(_instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _layeredProcesses[layer], false);
                        return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, segment, layer, true, null, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, string tag, SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null)
                return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(tag);
            }
            else if (_taggedProcesses.ContainsKey(tag))
            {
                if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                    ResumeCoroutinesOnInstance(_taggedProcesses[tag]);

                switch (behaviorOnCollision)
                {
                    case SingletonBehavior.Abort:
                    case SingletonBehavior.AbortAndUnpause:
                        var indexEnum = _taggedProcesses[tag].GetEnumerator();

                        while (indexEnum.MoveNext())
                            if (IsRunning(indexEnum.Current))
                                return indexEnum.Current;

                        break;
                    case SingletonBehavior.Wait:
                        CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, segment, 0, false, tag,
                            new CoroutineHandle(_instanceID), false);
                        WaitForOtherHandles(newCoroutineHandle, _taggedProcesses[tag], false);
                        return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, segment, 0, false, tag, new CoroutineHandle(_instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? RunCoroutineSingletonOnInstance(coroutine, segment, tag, behaviorOnCollision)
                : RunCoroutineSingletonOnInstance(coroutine, segment, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public CoroutineHandle RunCoroutineSingletonOnInstance(IEnumerator<float> coroutine, Segment segment, int layer, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            if (coroutine == null) return new CoroutineHandle();

            if (behaviorOnCollision == SingletonBehavior.Overwrite)
            {
                KillCoroutinesOnInstance(layer, tag);
                return RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(_instanceID), true);
            }

            if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
                return RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(_instanceID), true);

            if (behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
                ResumeCoroutinesOnInstance(layer, tag);

            if (behaviorOnCollision == SingletonBehavior.Abort || behaviorOnCollision == SingletonBehavior.AbortAndUnpause)
            {
                var matchesEnum = _taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_processLayers.ContainsKey(matchesEnum.Current) && _processLayers[matchesEnum.Current] == layer)
                        return matchesEnum.Current;
            }
            else if (behaviorOnCollision == SingletonBehavior.Wait)
            {
                List<CoroutineHandle> matches = new List<CoroutineHandle>();
                var matchesEnum = _taggedProcesses[tag].GetEnumerator();
                while (matchesEnum.MoveNext())
                    if (_processLayers.ContainsKey(matchesEnum.Current) && _processLayers[matchesEnum.Current] == layer)
                        matches.Add(matchesEnum.Current);

                if (matches.Count > 0)
                {
                    CoroutineHandle newCoroutineHandle = RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(_instanceID), false);
                    WaitForOtherHandles(newCoroutineHandle, matches, false);
                    return newCoroutineHandle;
                }
            }

            return RunCoroutineInternal(coroutine, segment, layer, true, tag, new CoroutineHandle(_instanceID), true);
        }


        private CoroutineHandle RunCoroutineInternal(IEnumerator<float> coroutine, Segment segment, int layer, bool layerHasValue, string tag, CoroutineHandle handle, bool prewarm)
        {
            ProcessIndex slot = new ProcessIndex { seg = segment };

            if (_handleToIndex.ContainsKey(handle))
            {
                _indexToHandle.Remove(_handleToIndex[handle]);
                _handleToIndex.Remove(handle);
            }

            float currentLocalTime = localTime;
            float currentDeltaTime = deltaTime;
            CoroutineHandle cashedHandle = currentCoroutine;
            currentCoroutine = handle;

            try
            {
                switch (segment)
                {
                    case Segment.Update:

                        if (_nextUpdateProcessSlot >= UpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = UpdateProcesses;
                            bool[] oldPausedArray = UpdatePaused;
                            bool[] oldHeldArray = UpdateHeld;

                            UpdateProcesses = new IEnumerator<float>[UpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            UpdatePaused = new bool[UpdateProcesses.Length];
                            UpdateHeld = new bool[UpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                UpdateProcesses[i] = oldProcArray[i];
                                UpdatePaused[i] = oldPausedArray[i];
                                UpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastUpdateProcessSlot = _nextUpdateProcessSlot;

                        slot.i = _nextUpdateProcessSlot++;
                        UpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!UpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (UpdateProcesses[slot.i] != null && float.IsNaN(UpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    UpdateProcesses[slot.i] = ReplacementFunction(UpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !UpdatePaused[slot.i] && !UpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;

                    case Segment.FixedUpdate:

                        if (_nextFixedUpdateProcessSlot >= FixedUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = FixedUpdateProcesses;
                            bool[] oldPausedArray = FixedUpdatePaused;
                            bool[] oldHeldArray = FixedUpdateHeld;

                            FixedUpdateProcesses = new IEnumerator<float>[FixedUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            FixedUpdatePaused = new bool[FixedUpdateProcesses.Length];
                            FixedUpdateHeld = new bool[FixedUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                FixedUpdateProcesses[i] = oldProcArray[i];
                                FixedUpdatePaused[i] = oldPausedArray[i];
                                FixedUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;

                        slot.i = _nextFixedUpdateProcessSlot++;
                        FixedUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!FixedUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (FixedUpdateProcesses[slot.i] != null && float.IsNaN(FixedUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    FixedUpdateProcesses[slot.i] = ReplacementFunction(FixedUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !FixedUpdatePaused[slot.i] && !FixedUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;

                    case Segment.LateUpdate:

                        if (_nextLateUpdateProcessSlot >= LateUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = LateUpdateProcesses;
                            bool[] oldPausedArray = LateUpdatePaused;
                            bool[] oldHeldArray = LateUpdateHeld;

                            LateUpdateProcesses = new IEnumerator<float>[LateUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            LateUpdatePaused = new bool[LateUpdateProcesses.Length];
                            LateUpdateHeld = new bool[LateUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                LateUpdateProcesses[i] = oldProcArray[i];
                                LateUpdatePaused[i] = oldPausedArray[i];
                                LateUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;

                        slot.i = _nextLateUpdateProcessSlot++;
                        LateUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!LateUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (LateUpdateProcesses[slot.i] != null && float.IsNaN(LateUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    LateUpdateProcesses[slot.i] = ReplacementFunction(LateUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !LateUpdatePaused[slot.i] && !LateUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;

                    case Segment.SlowUpdate:

                        if (_nextSlowUpdateProcessSlot >= SlowUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = SlowUpdateProcesses;
                            bool[] oldPausedArray = SlowUpdatePaused;
                            bool[] oldHeldArray = SlowUpdateHeld;

                            SlowUpdateProcesses = new IEnumerator<float>[SlowUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            SlowUpdatePaused = new bool[SlowUpdateProcesses.Length];
                            SlowUpdateHeld = new bool[SlowUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                SlowUpdateProcesses[i] = oldProcArray[i];
                                SlowUpdatePaused[i] = oldPausedArray[i];
                                SlowUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;

                        slot.i = _nextSlowUpdateProcessSlot++;
                        SlowUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!SlowUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (SlowUpdateProcesses[slot.i] != null && float.IsNaN(SlowUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    SlowUpdateProcesses[slot.i] = ReplacementFunction(SlowUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !SlowUpdatePaused[slot.i] && !SlowUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;

                    case Segment.RealtimeUpdate:

                        if (_nextRealtimeUpdateProcessSlot >= RealtimeUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = RealtimeUpdateProcesses;
                            bool[] oldPausedArray = RealtimeUpdatePaused;
                            bool[] oldHeldArray = RealtimeUpdateHeld;

                            RealtimeUpdateProcesses = new IEnumerator<float>[RealtimeUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            RealtimeUpdatePaused = new bool[RealtimeUpdateProcesses.Length];
                            RealtimeUpdateHeld = new bool[RealtimeUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                RealtimeUpdateProcesses[i] = oldProcArray[i];
                                RealtimeUpdatePaused[i] = oldPausedArray[i];
                                RealtimeUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastRealtimeUpdateProcessSlot = _nextRealtimeUpdateProcessSlot;

                        slot.i = _nextRealtimeUpdateProcessSlot++;
                        RealtimeUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!RealtimeUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (RealtimeUpdateProcesses[slot.i] != null && float.IsNaN(RealtimeUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    RealtimeUpdateProcesses[slot.i] = ReplacementFunction(RealtimeUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !RealtimeUpdatePaused[slot.i] && !RealtimeUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;
#if UNITY_EDITOR
                    case Segment.EditorUpdate:

                        if (!OnEditorStart())
                            return new CoroutineHandle();

                        if (handle.Key == 0)
                            handle = new CoroutineHandle(_instanceID);

                        if (_nextEditorUpdateProcessSlot >= EditorUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = EditorUpdateProcesses;
                            bool[] oldPausedArray = EditorUpdatePaused;
                            bool[] oldHeldArray = EditorUpdateHeld;

                            EditorUpdateProcesses = new IEnumerator<float>[EditorUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            EditorUpdatePaused = new bool[EditorUpdateProcesses.Length];
                            EditorUpdateHeld = new bool[EditorUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                EditorUpdateProcesses[i] = oldProcArray[i];
                                EditorUpdatePaused[i] = oldPausedArray[i];
                                EditorUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastEditorUpdateProcessSlot = _nextEditorUpdateProcessSlot;

                        slot.i = _nextEditorUpdateProcessSlot++;
                        EditorUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!EditorUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (EditorUpdateProcesses[slot.i] != null && float.IsNaN(EditorUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    EditorUpdateProcesses[slot.i] = ReplacementFunction(EditorUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !EditorUpdatePaused[slot.i] && !EditorUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;

                    case Segment.EditorSlowUpdate:

                        if (!OnEditorStart())
                            return new CoroutineHandle();

                        if (handle.Key == 0)
                            handle = new CoroutineHandle(_instanceID);

                        if (_nextEditorSlowUpdateProcessSlot >= EditorSlowUpdateProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = EditorSlowUpdateProcesses;
                            bool[] oldPausedArray = EditorSlowUpdatePaused;
                            bool[] oldHeldArray = EditorSlowUpdateHeld;

                            EditorSlowUpdateProcesses = new IEnumerator<float>[EditorSlowUpdateProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            EditorSlowUpdatePaused = new bool[EditorSlowUpdateProcesses.Length];
                            EditorSlowUpdateHeld = new bool[EditorSlowUpdateProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                EditorSlowUpdateProcesses[i] = oldProcArray[i];
                                EditorSlowUpdatePaused[i] = oldPausedArray[i];
                                EditorSlowUpdateHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastEditorSlowUpdateProcessSlot = _nextEditorSlowUpdateProcessSlot;

                        slot.i = _nextEditorSlowUpdateProcessSlot++;
                        EditorSlowUpdateProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        while (prewarm)
                        {
                            if (!EditorSlowUpdateProcesses[slot.i].MoveNext())
                            {
                                if (_indexToHandle.ContainsKey(slot))
                                    KillCoroutinesOnInstance(_indexToHandle[slot]);

                                prewarm = false;
                            }
                            else if (EditorSlowUpdateProcesses[slot.i] != null && float.IsNaN(EditorSlowUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction != null)
                                {
                                    EditorSlowUpdateProcesses[slot.i] = ReplacementFunction(EditorSlowUpdateProcesses[slot.i], _indexToHandle[slot]);
                                    ReplacementFunction = null;
                                }
                                prewarm = !EditorSlowUpdatePaused[slot.i] && !EditorSlowUpdateHeld[slot.i];
                            }
                            else
                            {
                                prewarm = false;
                            }
                        }

                        break;
#endif
                    case Segment.EndOfFrame:

                        if (_nextEndOfFrameProcessSlot >= EndOfFrameProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = EndOfFrameProcesses;
                            bool[] oldPausedArray = EndOfFramePaused;
                            bool[] oldHeldArray = EndOfFrameHeld;

                            EndOfFrameProcesses = new IEnumerator<float>[EndOfFrameProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            EndOfFramePaused = new bool[EndOfFrameProcesses.Length];
                            EndOfFrameHeld = new bool[EndOfFrameProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                EndOfFrameProcesses[i] = oldProcArray[i];
                                EndOfFramePaused[i] = oldPausedArray[i];
                                EndOfFrameHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastEndOfFrameProcessSlot = _nextEndOfFrameProcessSlot;

                        slot.i = _nextEndOfFrameProcessSlot++;
                        EndOfFrameProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        _eofWatcherHandle = RunCoroutineSingletonOnInstance(_EOFPumpWatcher(), _eofWatcherHandle, SingletonBehavior.Abort);

                        break;

                    case Segment.ManualTimeframe:

                        if (_nextManualTimeframeProcessSlot >= ManualTimeframeProcesses.Length)
                        {
                            IEnumerator<float>[] oldProcArray = ManualTimeframeProcesses;
                            bool[] oldPausedArray = ManualTimeframePaused;
                            bool[] oldHeldArray = ManualTimeframeHeld;

                            ManualTimeframeProcesses = new IEnumerator<float>[ManualTimeframeProcesses.Length + (ProcessArrayChunkSize * _expansions++)];
                            ManualTimeframePaused = new bool[ManualTimeframeProcesses.Length];
                            ManualTimeframeHeld = new bool[ManualTimeframeProcesses.Length];

                            for (int i = 0; i < oldProcArray.Length; i++)
                            {
                                ManualTimeframeProcesses[i] = oldProcArray[i];
                                ManualTimeframePaused[i] = oldPausedArray[i];
                                ManualTimeframeHeld[i] = oldHeldArray[i];
                            }
                        }

                        if (UpdateTimeValues(slot.seg))
                            _lastManualTimeframeProcessSlot = _nextManualTimeframeProcessSlot;

                        slot.i = _nextManualTimeframeProcessSlot++;
                        ManualTimeframeProcesses[slot.i] = coroutine;

                        if (null != tag)
                            AddTagOnInstance(tag, handle);

                        if (layerHasValue)
                            AddLayerOnInstance(layer, handle);

                        _indexToHandle.Add(slot, handle);
                        _handleToIndex.Add(handle, slot);

                        break;

                    default:
                        handle = new CoroutineHandle();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            localTime = currentLocalTime;
            deltaTime = currentDeltaTime;
            currentCoroutine = cashedHandle;

            return handle;
        }
        /// <summary>
        /// This will kill all coroutines running on the main MEC instance and reset the context.
        /// NOTE: If you call this function from within a running coroutine then you MUST end the current
        /// coroutine. If the running coroutine has more work to do you may run a new "part 2" coroutine 
        /// function to complete the task before ending the current one.
        /// </summary>
        /// <returns>The number of coroutines that were killed.</returns>
        public static int KillCoroutines()
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance();
        }

        /// <summary>
        /// This will kill all coroutines running on the current MEC instance and reset the context. 
        /// NOTE: If you call this function from within a running coroutine then you MUST end the current
        /// coroutine. If the running coroutine has more work to do you may run a new "part 2" coroutine 
        /// function to complete the task before ending the current one.
        /// </summary>
        /// <returns>The number of coroutines that were killed.</returns>
        public int KillCoroutinesOnInstance()
        {
            int retVal = _nextUpdateProcessSlot + _nextLateUpdateProcessSlot + _nextFixedUpdateProcessSlot + _nextSlowUpdateProcessSlot +
                         _nextRealtimeUpdateProcessSlot + _nextEditorUpdateProcessSlot + _nextEditorSlowUpdateProcessSlot +
                         _nextEndOfFrameProcessSlot + _nextManualTimeframeProcessSlot;

            UpdateProcesses = new IEnumerator<float>[InitialBufferSizeLarge];
            UpdatePaused = new bool[InitialBufferSizeLarge];
            UpdateHeld = new bool[InitialBufferSizeLarge];
            UpdateCoroutines = 0;
            _nextUpdateProcessSlot = 0;

            LateUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            LateUpdatePaused = new bool[InitialBufferSizeSmall];
            LateUpdateHeld = new bool[InitialBufferSizeSmall];
            LateUpdateCoroutines = 0;
            _nextLateUpdateProcessSlot = 0;

            FixedUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
            FixedUpdatePaused = new bool[InitialBufferSizeMedium];
            FixedUpdateHeld = new bool[InitialBufferSizeMedium];
            FixedUpdateCoroutines = 0;
            _nextFixedUpdateProcessSlot = 0;

            SlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
            SlowUpdatePaused = new bool[InitialBufferSizeMedium];
            SlowUpdateHeld = new bool[InitialBufferSizeMedium];
            SlowUpdateCoroutines = 0;
            _nextSlowUpdateProcessSlot = 0;

            RealtimeUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            RealtimeUpdatePaused = new bool[InitialBufferSizeSmall];
            RealtimeUpdateHeld = new bool[InitialBufferSizeSmall];
            RealtimeUpdateCoroutines = 0;
            _nextRealtimeUpdateProcessSlot = 0;

            EditorUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            EditorUpdatePaused = new bool[InitialBufferSizeSmall];
            EditorUpdateHeld = new bool[InitialBufferSizeSmall];
            EditorUpdateCoroutines = 0;
            _nextEditorUpdateProcessSlot = 0;

            EditorSlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            EditorSlowUpdatePaused = new bool[InitialBufferSizeSmall];
            EditorSlowUpdateHeld = new bool[InitialBufferSizeSmall];
            EditorSlowUpdateCoroutines = 0;
            _nextEditorSlowUpdateProcessSlot = 0;

            EndOfFrameProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            EndOfFramePaused = new bool[InitialBufferSizeSmall];
            EndOfFrameHeld = new bool[InitialBufferSizeSmall];
            EndOfFrameCoroutines = 0;
            _nextEndOfFrameProcessSlot = 0;

            ManualTimeframeProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            ManualTimeframePaused = new bool[InitialBufferSizeSmall];
            ManualTimeframeHeld = new bool[InitialBufferSizeSmall];
            ManualTimeframeCoroutines = 0;
            _nextManualTimeframeProcessSlot = 0;

            _processTags.Clear();
            _taggedProcesses.Clear();
            _processLayers.Clear();
            _layeredProcesses.Clear();
            _handleToIndex.Clear();
            _indexToHandle.Clear();
            _waitingTriggers.Clear();
            _expansions = (ushort)((_expansions / 2) + 1);
            Links.Clear();

#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
            return retVal;
        }

        /// <summary>
        /// Kills the coroutine that you pass in (ignores it if the handle is invalid or isn't running).
        /// </summary>
        /// <param name="handles">A list of handles to be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(CoroutineHandle handle)
        {
            return ActiveInstances[handle.Key] != null
                ? GetInstance(handle.Key).KillCoroutinesOnInstance(handle) : 0;
        }

        /// <summary>
        /// Kills all the coroutines that you pass in (ignores any that aren't running).
        /// </summary>
        /// <param name="handles">A list of handles to be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(params CoroutineHandle[] handles)
        {
            int count = 0;
            for (int i = 0;i < handles.Length;i++)
                count += ActiveInstances[handles[i].Key] != null 
                    ? GetInstance(handles[i].Key).KillCoroutinesOnInstance(handles[i]) : 0;

            return count;
        }

        /// <summary>
        /// Kills the instance of the coroutine handle on this Timing instance if it exists.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to kill.</param>
        /// <returns>The number of coroutines that were found and killed (Normally 0 or 1).</returns>
        public int KillCoroutinesOnInstance(CoroutineHandle handle)
        {
            int count = 0;

            if (_handleToIndex.ContainsKey(handle))
            {
                if (_waitingTriggers.ContainsKey(handle))
                    CloseWaitingProcess(handle);

                if (Nullify(handle))
                    count++;
                RemoveGraffiti(handle);
            }

            if (Links.ContainsKey(handle))
            {
                var linksEnum = Links[handle].GetEnumerator();
                Links.Remove(handle);
                while (linksEnum.MoveNext())
                    count += KillCoroutines(linksEnum.Current);
            }

            return count;
        }

        /// <summary>
        /// Kills all coroutines on the given layer.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(GameObject gameObj)
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance(gameObj.GetInstanceID());
        }

        /// <summary> 
        /// Kills all coroutines on the given layer.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(GameObject gameObj)
        {
            return KillCoroutinesOnInstance(gameObj.GetInstanceID());
        }

        /// <summary>
        /// Kills all coroutines on the given layer.
        /// </summary>
        /// <param name="layer">All coroutines on this layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(int layer)
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance(layer);
        }

        /// <summary> 
        /// Kills all coroutines on the given layer.
        /// </summary>
        /// <param name="layer">All coroutines on this layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(int layer)
        {
            int numberFound = 0;

            while (_layeredProcesses.ContainsKey(layer))
            {
                var matchEnum = _layeredProcesses[layer].GetEnumerator();
                matchEnum.MoveNext();

                if (Nullify(matchEnum.Current))
                {
                    if (_waitingTriggers.ContainsKey(matchEnum.Current))
                        CloseWaitingProcess(matchEnum.Current);

                    numberFound++;
                }

                RemoveGraffiti(matchEnum.Current);

                if (Links.ContainsKey(matchEnum.Current))
                {
                    var linksEnum = Links[matchEnum.Current].GetEnumerator();
                    Links.Remove(matchEnum.Current);
                    while (linksEnum.MoveNext())
                        numberFound += KillCoroutines(linksEnum.Current);
                }
            }

            return numberFound;
        }

        /// <summary>
        /// Kills all coroutines that have the given tag.
        /// </summary>
        /// <param name="tag">All coroutines with this tag will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(string tag)
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance(tag);
        }

        /// <summary> 
        /// Kills all coroutines that have the given tag.
        /// </summary>
        /// <param name="tag">All coroutines with this tag will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(string tag)
        {
            if (tag == null) return 0;
            int numberFound = 0;

            while (_taggedProcesses.ContainsKey(tag))
            {
                var matchEnum = _taggedProcesses[tag].GetEnumerator();
                matchEnum.MoveNext();

                if (Nullify(_handleToIndex[matchEnum.Current]))
                {
                    if (_waitingTriggers.ContainsKey(matchEnum.Current))
                        CloseWaitingProcess(matchEnum.Current);

                    numberFound++;
                }

                RemoveGraffiti(matchEnum.Current);

                if (Links.ContainsKey(matchEnum.Current))
                {
                    var linksEnum = Links[matchEnum.Current].GetEnumerator();
                    Links.Remove(matchEnum.Current);
                    while (linksEnum.MoveNext())
                        numberFound += KillCoroutines(linksEnum.Current);
                }
            }

            return numberFound;
        }

        /// <summary>
        /// Kills all coroutines with the given tag on the given layer.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be killed.</param>
        /// <param name="tag">All coroutines with this tag on the given layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(GameObject gameObj, string tag)
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary> 
        /// Kills all coroutines with the given tag on the given layer.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be killed.</param>
        /// <param name="tag">All coroutines with this tag on the given layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(GameObject gameObj, string tag)
        {
            return KillCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary>
        /// Kills all coroutines with the given tag on the given layer.
        /// </summary>
        /// <param name="layer">All coroutines on this layer with the given tag will be killed.</param>
        /// <param name="tag">All coroutines with this tag on the given layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(int layer, string tag)
        {
            return _instance == null ? 0 : _instance.KillCoroutinesOnInstance(layer, tag);
        }

        /// <summary> 
        /// Kills all coroutines with the given tag on the given layer.
        /// </summary>
        /// <param name="layer">All coroutines on this layer with the given tag will be killed.</param>
        /// <param name="tag">All coroutines with this tag on the given layer will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(int layer, string tag)
        {
            if (tag == null)
                return KillCoroutinesOnInstance(layer);
            if (!_layeredProcesses.ContainsKey(layer) || !_taggedProcesses.ContainsKey(tag))
                return 0;
            int count = 0;

            var indexesEnum = _taggedProcesses[tag].GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (CoindexIsNull(_handleToIndex[indexesEnum.Current]) || !_layeredProcesses[layer].Contains(indexesEnum.Current) ||
                    !Nullify(indexesEnum.Current))
                    continue;

                if (_waitingTriggers.ContainsKey(indexesEnum.Current))
                    CloseWaitingProcess(indexesEnum.Current);

                count++;
                RemoveGraffiti(indexesEnum.Current);

                if (Links.ContainsKey(indexesEnum.Current))
                {
                    var linksEnum = Links[indexesEnum.Current].GetEnumerator();
                    Links.Remove(indexesEnum.Current);
                    while (linksEnum.MoveNext())
                        KillCoroutines(linksEnum.Current);
                }

                if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
                    break;

                indexesEnum = _taggedProcesses[tag].GetEnumerator();
            }

            return count;
        }

        /// <summary>
        /// Retrieves the MEC manager that corresponds to the supplied instance id.
        /// </summary>
        /// <param name="ID">The instance ID.</param>
        /// <returns>The manager, or null if not found.</returns>
        public static Timing GetInstance(byte ID)
        {
            if (ID >= 0x10)
                return null;
            return ActiveInstances[ID];
        }

        /// <summary>
        /// Use "yield return Timing.WaitForSeconds(time);" to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public static float WaitForSeconds(float waitTime)
        {
            if (float.IsNaN(waitTime)) waitTime = 0f;
            return LocalTime + waitTime;
        }

        /// <summary>
        /// Use "yield return timingInstance.WaitForSecondsOnInstance(time);" to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public float WaitForSecondsOnInstance(float waitTime)
        {
            if (float.IsNaN(waitTime)) waitTime = 0f;
            return localTime + waitTime;
        }

        private bool UpdateTimeValues(Segment segment)
        {
            switch (segment)
            {
                case Segment.Update:
                    if (_currentUpdateFrame != Time.frameCount)
                    {
                        deltaTime = Time.deltaTime;
                        _lastUpdateTime += deltaTime;
                        localTime = _lastUpdateTime;
                        _currentUpdateFrame = Time.frameCount;
                        return true;
                    }
                    else
                    {
                        deltaTime = Time.deltaTime;
                        localTime = _lastUpdateTime;
                        return false;
                    }
                case Segment.LateUpdate:
                    if (_currentLateUpdateFrame != Time.frameCount)
                    {
                        deltaTime = Time.deltaTime;
                        _lastLateUpdateTime += deltaTime;
                        localTime = _lastLateUpdateTime;
                        _currentLateUpdateFrame = Time.frameCount;
                        return true;
                    }
                    else
                    {
                        deltaTime = Time.deltaTime;
                        localTime = _lastLateUpdateTime;
                        return false;
                    }
                case Segment.FixedUpdate:
                    deltaTime = Time.fixedDeltaTime;
                    localTime = Time.fixedTime;

                    if (_lastFixedUpdateTime + 0.0001f < Time.fixedTime)
                    {
                        _lastFixedUpdateTime = Time.fixedTime;
                        return true;
                    }

                    return false;
                case Segment.SlowUpdate:
                    if (_currentSlowUpdateFrame != Time.frameCount)
                    {
                        deltaTime = _lastSlowUpdateDeltaTime = Time.realtimeSinceStartup - _lastSlowUpdateTime;
                        localTime = _lastSlowUpdateTime = Time.realtimeSinceStartup;
                        _currentSlowUpdateFrame = Time.frameCount;
                        return true;
                    }
                    else
                    {
                        localTime = _lastSlowUpdateTime;
                        deltaTime = _lastSlowUpdateDeltaTime;
                        return false;
                    }
                case Segment.RealtimeUpdate:
                    if (_currentRealtimeUpdateFrame != Time.frameCount)
                    {
                        deltaTime = Time.unscaledDeltaTime;
                        _lastRealtimeUpdateTime += deltaTime;
                        localTime = _lastRealtimeUpdateTime;
                        _currentRealtimeUpdateFrame = Time.frameCount;
                        return true;
                    }
                    else
                    {
                        deltaTime = Time.unscaledDeltaTime;
                        localTime = _lastRealtimeUpdateTime;
                        return false;
                    }
#if UNITY_EDITOR
                case Segment.EditorUpdate:
                    if (_lastEditorUpdateTime + 0.0001 < EditorApplication.timeSinceStartup)
                    {
                        _lastEditorUpdateDeltaTime = (float)EditorApplication.timeSinceStartup - _lastEditorUpdateTime;
                        if (_lastEditorUpdateDeltaTime > Time.maximumDeltaTime)
                            _lastEditorUpdateDeltaTime = Time.maximumDeltaTime;

                        deltaTime = _lastEditorUpdateDeltaTime;
                        localTime = _lastEditorUpdateTime = (float)EditorApplication.timeSinceStartup;
                        return true;
                    }
                    else
                    {
                        deltaTime = _lastEditorUpdateDeltaTime;
                        localTime = _lastEditorUpdateTime;
                        return false;
                    }
                case Segment.EditorSlowUpdate:
                    if (_lastEditorSlowUpdateTime + 0.0001 < EditorApplication.timeSinceStartup)
                    {
                        _lastEditorSlowUpdateDeltaTime = (float)EditorApplication.timeSinceStartup - _lastEditorSlowUpdateTime;
                        deltaTime = _lastEditorSlowUpdateDeltaTime;
                        localTime = _lastEditorSlowUpdateTime = (float)EditorApplication.timeSinceStartup;
                        return true;
                    }
                    else
                    {
                        deltaTime = _lastEditorSlowUpdateDeltaTime;
                        localTime = _lastEditorSlowUpdateTime;
                        return false;
                    }
#endif
                case Segment.EndOfFrame:
                    if (_currentEndOfFrameFrame != Time.frameCount)
                    {
                        deltaTime = Time.deltaTime;
                        _lastEndOfFrameTime += deltaTime;
                        localTime = _lastEndOfFrameTime;
                        _currentEndOfFrameFrame = Time.frameCount;
                        return true;
                    }
                    else
                    {
                        deltaTime = Time.deltaTime;
                        localTime = _lastEndOfFrameTime;
                        return false;
                    }
                case Segment.ManualTimeframe:
                    float timeCalculated = SetManualTimeframeTime == null ? Time.time : SetManualTimeframeTime(_lastManualTimeframeTime);
                    if (_lastManualTimeframeTime + 0.0001 < timeCalculated && _lastManualTimeframeTime - 0.0001 > timeCalculated)
                    {
                        localTime = timeCalculated;
                        deltaTime = localTime - _lastManualTimeframeTime;

                        if (deltaTime > Time.maximumDeltaTime)
                            deltaTime = Time.maximumDeltaTime;

                        _lastManualTimeframeDeltaTime = deltaTime;
                        _lastManualTimeframeTime = timeCalculated;
                        return true;
                    }
                    else
                    {
                        deltaTime = _lastManualTimeframeDeltaTime;
                        localTime = _lastManualTimeframeTime;
                        return false;
                    }
            }
            return true;
        }

        private float GetSegmentTime(Segment segment)
        {
            switch (segment)
            {
                case Segment.Update:
                    if (_currentUpdateFrame == Time.frameCount)
                        return _lastUpdateTime;
                    else
                        return _lastUpdateTime + Time.deltaTime;
                case Segment.LateUpdate:
                    if (_currentUpdateFrame == Time.frameCount)
                        return _lastLateUpdateTime;
                    else
                        return _lastLateUpdateTime + Time.deltaTime;
                case Segment.FixedUpdate:
                    return Time.fixedTime;
                case Segment.SlowUpdate:
                    return Time.realtimeSinceStartup;
                case Segment.RealtimeUpdate:
                    if (_currentRealtimeUpdateFrame == Time.frameCount)
                        return _lastRealtimeUpdateTime;
                    else
                        return _lastRealtimeUpdateTime + Time.unscaledDeltaTime;
#if UNITY_EDITOR
                case Segment.EditorUpdate:
                case Segment.EditorSlowUpdate:
                    return (float)EditorApplication.timeSinceStartup;
#endif
                case Segment.EndOfFrame:
                    if (_currentUpdateFrame == Time.frameCount)
                        return _lastEndOfFrameTime;
                    else
                        return _lastEndOfFrameTime + Time.deltaTime;
                case Segment.ManualTimeframe:
                    return _lastManualTimeframeTime;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// This will pause all coroutines running on the main MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines()
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance();
        }

        /// <summary>
        /// This will pause all coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance()
        {
            int count = 0;
            int i;
            for (i = 0; i < _nextUpdateProcessSlot; i++)
            {
                if (!UpdatePaused[i] && UpdateProcesses[i] != null)
                {
                    count++;
                    UpdatePaused[i] = true;

                    if (UpdateProcesses[i].Current > GetSegmentTime(Segment.Update))
                        UpdateProcesses[i] = _InjectDelay(UpdateProcesses[i],
                            UpdateProcesses[i].Current - GetSegmentTime(Segment.Update));
                }
            }

            for (i = 0; i < _nextLateUpdateProcessSlot; i++)
            {
                if (!LateUpdatePaused[i] && LateUpdateProcesses[i] != null)
                {
                    count++;
                    LateUpdatePaused[i] = true;

                    if (LateUpdateProcesses[i].Current > GetSegmentTime(Segment.LateUpdate))
                        LateUpdateProcesses[i] = _InjectDelay(LateUpdateProcesses[i],
                            LateUpdateProcesses[i].Current - GetSegmentTime(Segment.LateUpdate));
                }
            }

            for (i = 0; i < _nextFixedUpdateProcessSlot; i++)
            {
                if (!FixedUpdatePaused[i] && FixedUpdateProcesses[i] != null)
                {
                    count++;
                    FixedUpdatePaused[i] = true;

                    if (FixedUpdateProcesses[i].Current > GetSegmentTime(Segment.FixedUpdate))
                        FixedUpdateProcesses[i] = _InjectDelay(FixedUpdateProcesses[i],
                            FixedUpdateProcesses[i].Current - GetSegmentTime(Segment.FixedUpdate));
                }
            }

            for (i = 0; i < _nextSlowUpdateProcessSlot; i++)
            {
                if (!SlowUpdatePaused[i] && SlowUpdateProcesses[i] != null)
                {
                    count++;
                    SlowUpdatePaused[i] = true;

                    if (SlowUpdateProcesses[i].Current > GetSegmentTime(Segment.SlowUpdate))
                        SlowUpdateProcesses[i] = _InjectDelay(SlowUpdateProcesses[i],
                            SlowUpdateProcesses[i].Current - GetSegmentTime(Segment.SlowUpdate));
                }
            }

            for (i = 0; i < _nextRealtimeUpdateProcessSlot; i++)
            {
                if (!RealtimeUpdatePaused[i] && RealtimeUpdateProcesses[i] != null)
                {
                    count++;
                    RealtimeUpdatePaused[i] = true;

                    if (RealtimeUpdateProcesses[i].Current > GetSegmentTime(Segment.RealtimeUpdate))
                        RealtimeUpdateProcesses[i] = _InjectDelay(RealtimeUpdateProcesses[i],
                            RealtimeUpdateProcesses[i].Current - GetSegmentTime(Segment.RealtimeUpdate));
                }
            }

            for (i = 0; i < _nextEditorUpdateProcessSlot; i++)
            {
                if (!EditorUpdatePaused[i] && EditorUpdateProcesses[i] != null)
                {
                    count++;
                    EditorUpdatePaused[i] = true;

                    if (EditorUpdateProcesses[i].Current > GetSegmentTime(Segment.EditorUpdate))
                        EditorUpdateProcesses[i] = _InjectDelay(EditorUpdateProcesses[i],
                            EditorUpdateProcesses[i].Current - GetSegmentTime(Segment.EditorUpdate));
                }
            }

            for (i = 0; i < _nextEditorSlowUpdateProcessSlot; i++)
            {
                if (!EditorSlowUpdatePaused[i] && EditorSlowUpdateProcesses[i] != null)
                {
                    count++;
                    EditorSlowUpdatePaused[i] = true;

                    if (EditorSlowUpdateProcesses[i].Current > GetSegmentTime(Segment.EditorSlowUpdate))
                        EditorSlowUpdateProcesses[i] = _InjectDelay(EditorSlowUpdateProcesses[i],
                            EditorSlowUpdateProcesses[i].Current - GetSegmentTime(Segment.EditorSlowUpdate));
                }
            }

            for (i = 0; i < _nextEndOfFrameProcessSlot; i++)
            {
                if (!EndOfFramePaused[i] && EndOfFrameProcesses[i] != null)
                {
                    count++;
                    EndOfFramePaused[i] = true;

                    if (EndOfFrameProcesses[i].Current > GetSegmentTime(Segment.EndOfFrame))
                        EndOfFrameProcesses[i] = _InjectDelay(EndOfFrameProcesses[i],
                            EndOfFrameProcesses[i].Current - GetSegmentTime(Segment.EndOfFrame));
                }
            }

            for (i = 0; i < _nextManualTimeframeProcessSlot; i++)
            {
                if (!ManualTimeframePaused[i] && ManualTimeframeProcesses[i] != null)
                {
                    count++;
                    ManualTimeframePaused[i] = true;

                    if (ManualTimeframeProcesses[i].Current > GetSegmentTime(Segment.ManualTimeframe))
                        ManualTimeframeProcesses[i] = _InjectDelay(ManualTimeframeProcesses[i],
                            ManualTimeframeProcesses[i].Current - GetSegmentTime(Segment.ManualTimeframe));
                }
            }

            var indexesEnum = Links.GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (!_handleToIndex.ContainsKey(indexesEnum.Current.Key)) continue;

                var linksEnum = indexesEnum.Current.Value.GetEnumerator();
                while (linksEnum.MoveNext())
                    count += PauseCoroutines(linksEnum.Current);
            }

            return count;
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to pause.</param>
        /// <returns>The number of coroutines that were paused (Normally 0 or 1).</returns>
        public int PauseCoroutinesOnInstance(CoroutineHandle handle)
        {
            int count = 0;

            if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]) && !SetPause(_handleToIndex[handle], true))
                count++;

            if (Links.ContainsKey(handle))
            {
                var links = Links[handle];
                Links.Remove(handle);
                var linksEnum = links.GetEnumerator();
                while (linksEnum.MoveNext())
                    count += PauseCoroutines(linksEnum.Current);
                Links.Add(handle, links);
            }

            return count;
        }

        /// <summary>
        /// This will pause any matching coroutines until ResumeCoroutines is called.
        /// </summary>
        /// <param name="handles">A list of handles to coroutines you want to pause.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(CoroutineHandle handle)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(handle);
        }

        /// <summary>
        /// This will pause any matching coroutines until ResumeCoroutines is called.
        /// </summary>
        /// <param name="handles">A list of handles to coroutines you want to pause.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(params CoroutineHandle[] handles)
        {
            int total = 0;
            for (int i = 0;i < handles.Length;i++)
                total += ActiveInstances[handles[i].Key] != null 
                    ? GetInstance(handles[i].Key).PauseCoroutinesOnInstance(handles[i]) : 0;
            return total;
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(GameObject gameObj)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(gameObj);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(GameObject gameObj)
        {
            return gameObj == null ? 0 : PauseCoroutinesOnInstance(gameObj.GetInstanceID());
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="layer">Any coroutines on the matching layer will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(int layer)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(layer);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="layer">Any coroutines on the matching layer will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(int layer)
        {
            if (!_layeredProcesses.ContainsKey(layer))
                return 0;

            int count = 0;
            var matchesEnum = _layeredProcesses[layer].GetEnumerator();

            while (matchesEnum.MoveNext())
            {
                if (!CoindexIsNull(_handleToIndex[matchesEnum.Current]) && !SetPause(_handleToIndex[matchesEnum.Current], true))
                    count++;

                if (Links.ContainsKey(matchesEnum.Current))
                {
                    var links = Links[matchesEnum.Current];
                    Links.Remove(matchesEnum.Current);
                    var linksEnum = links.GetEnumerator();
                    while (linksEnum.MoveNext())
                        count += PauseCoroutines(linksEnum.Current);
                    Links.Add(matchesEnum.Current, links);
                }
            }

            return count;
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(string tag)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(tag);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(string tag)
        {
            if (tag == null || !_taggedProcesses.ContainsKey(tag))
                return 0;

            int count = 0;
            var matchesEnum = _taggedProcesses[tag].GetEnumerator();

            while (matchesEnum.MoveNext())
            {
                if (!CoindexIsNull(_handleToIndex[matchesEnum.Current]) && !SetPause(_handleToIndex[matchesEnum.Current], true))
                    count++;

                if (Links.ContainsKey(matchesEnum.Current))
                {
                    var links = Links[matchesEnum.Current];
                    Links.Remove(matchesEnum.Current);
                    var linksEnum = links.GetEnumerator();
                    while (linksEnum.MoveNext())
                        count += PauseCoroutines(linksEnum.Current);
                    Links.Add(matchesEnum.Current, links);
                }
            }

            return count;
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be paused.</param>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(GameObject gameObj, string tag)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be paused.</param>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(GameObject gameObj, string tag)
        {
            return gameObj == null ? 0 : PauseCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="layer">Any coroutines on the matching layer will be paused.</param>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(int layer, string tag)
        {
            return _instance == null ? 0 : _instance.PauseCoroutinesOnInstance(layer, tag);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="layer">Any coroutines on the matching layer will be paused.</param>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(int layer, string tag)
        {
            if (tag == null)
                return PauseCoroutinesOnInstance(layer);

            if (!_taggedProcesses.ContainsKey(tag) || !_layeredProcesses.ContainsKey(layer))
                return 0;

            int count = 0;
            var matchesEnum = _taggedProcesses[tag].GetEnumerator();

            while (matchesEnum.MoveNext())
            {
                if (_processLayers.ContainsKey(matchesEnum.Current) && _processLayers[matchesEnum.Current] == layer
                    && !CoindexIsNull(_handleToIndex[matchesEnum.Current]))
                {
                    if (!SetPause(_handleToIndex[matchesEnum.Current], true))
                        count++;

                    if (Links.ContainsKey(matchesEnum.Current))
                    {
                        var links = Links[matchesEnum.Current];
                        Links.Remove(matchesEnum.Current);
                        var linksEnum = links.GetEnumerator();
                        while (linksEnum.MoveNext())
                            count += PauseCoroutines(linksEnum.Current);
                        Links.Add(matchesEnum.Current, links);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// This resumes all coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines()
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance();
        }

        /// <summary>
        /// This resumes all coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance()
        {
            int count = 0;
            ProcessIndex coindex;
            for (coindex.i = 0, coindex.seg = Segment.Update; coindex.i < _nextUpdateProcessSlot; coindex.i++)
            {
                if (UpdatePaused[coindex.i] && UpdateProcesses[coindex.i] != null)
                {
                    UpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.LateUpdate; coindex.i < _nextLateUpdateProcessSlot; coindex.i++)
            {
                if (LateUpdatePaused[coindex.i] && LateUpdateProcesses[coindex.i] != null)
                {
                    LateUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.FixedUpdate; coindex.i < _nextFixedUpdateProcessSlot; coindex.i++)
            {
                if (FixedUpdatePaused[coindex.i] && FixedUpdateProcesses[coindex.i] != null)
                {
                    FixedUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.SlowUpdate; coindex.i < _nextSlowUpdateProcessSlot; coindex.i++)
            {
                if (SlowUpdatePaused[coindex.i] && SlowUpdateProcesses[coindex.i] != null)
                {
                    SlowUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.RealtimeUpdate; coindex.i < _nextRealtimeUpdateProcessSlot; coindex.i++)
            {
                if (RealtimeUpdatePaused[coindex.i] && RealtimeUpdateProcesses[coindex.i] != null)
                {
                    RealtimeUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.EditorUpdate; coindex.i < _nextEditorUpdateProcessSlot; coindex.i++)
            {
                if (EditorUpdatePaused[coindex.i] && EditorUpdateProcesses[coindex.i] != null)
                {
                    EditorUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.EditorSlowUpdate; coindex.i < _nextEditorSlowUpdateProcessSlot; coindex.i++)
            {
                if (EditorSlowUpdatePaused[coindex.i] && EditorSlowUpdateProcesses[coindex.i] != null)
                {
                    EditorSlowUpdatePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.EndOfFrame; coindex.i < _nextEndOfFrameProcessSlot; coindex.i++)
            {
                if (EndOfFramePaused[coindex.i] && EndOfFrameProcesses[coindex.i] != null)
                {
                    EndOfFramePaused[coindex.i] = false;
                    count++;
                }
            }

            for (coindex.i = 0, coindex.seg = Segment.ManualTimeframe; coindex.i < _nextManualTimeframeProcessSlot; coindex.i++)
            {
                if (ManualTimeframePaused[coindex.i] && ManualTimeframeProcesses[coindex.i] != null)
                {
                    ManualTimeframePaused[coindex.i] = false;
                    count++;
                }
            }

            var indexesEnum = Links.GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (!_handleToIndex.ContainsKey(indexesEnum.Current.Key)) continue;

                var linksEnum = indexesEnum.Current.Value.GetEnumerator();
                while (linksEnum.MoveNext())
                    count += ResumeCoroutines(linksEnum.Current);
            }

            return count;
        }

        /// <summary>
        /// This will resume any matching coroutines that are paused.
        /// </summary>
        /// <param name="handles">A list of handles to coroutines you want to resume.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(CoroutineHandle handle)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(handle);
        }
        
        /// <summary>
        /// This will resume any matching coroutines that are paused.
        /// </summary>
        /// <param name="handles">A list of handles to coroutines you want to resume.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(params CoroutineHandle[] handles)
        {
            int count = 0;
            for (int i = 0; i < handles.Length; i++)
                count += ActiveInstances[handles[i].Key] != null
                    ? GetInstance(handles[i].Key).ResumeCoroutinesOnInstance(handles[i]) : 0;
            return count;
        }

        /// <summary>
        /// This will resume any matching coroutines that are paused and running on this MEC instance.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to resume.</param>
        /// <returns>The number of coroutines that were resumed. (Normally 0 or 1)</returns>
        public int ResumeCoroutinesOnInstance(CoroutineHandle handle)
        {
            int count = 0;

            if (_handleToIndex.ContainsKey(handle) && !CoindexIsNull(_handleToIndex[handle]) && SetPause(_handleToIndex[handle], false))
                count++;

            if (Links.ContainsKey(handle))
            {
                var links = Links[handle];
                Links.Remove(handle);
                var linksEnum = links.GetEnumerator();
                while (linksEnum.MoveNext())
                    count += ResumeCoroutines(linksEnum.Current);
                Links.Add(handle, links);
            }

            return count;
        }

        /// <summary>
        /// This will resume any matching coroutines running on this MEC instance.
        /// </summary>
        /// <param name="handles">A list of handles to coroutines you want to resume.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(IEnumerable<CoroutineHandle> handles)
        {
            int count = 0;
            var handlesEnum = handles.GetEnumerator();
            while (!handlesEnum.MoveNext())
                ResumeCoroutinesOnInstance(handlesEnum.Current);
            return count;
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be resumed.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(GameObject gameObj)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(gameObj.GetInstanceID());
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be resumed.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(GameObject gameObj)
        {
            return gameObj == null ? 0 : ResumeCoroutinesOnInstance(gameObj.GetInstanceID());
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <param name="layer">Any coroutines previously paused on the matching layer will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(int layer)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(layer);
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="layer">Any coroutines previously paused on the matching layer will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(int layer)
        {
            if (!_layeredProcesses.ContainsKey(layer))
                return 0;
            int count = 0;

            var indexesEnum = _layeredProcesses[layer].GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (!CoindexIsNull(_handleToIndex[indexesEnum.Current]) && SetPause(_handleToIndex[indexesEnum.Current], false))
                {
                    count++;
                }

                if (Links.ContainsKey(indexesEnum.Current))
                {
                    var links = Links[indexesEnum.Current];
                    Links.Remove(indexesEnum.Current);
                    var linksEnum = links.GetEnumerator();
                    while (linksEnum.MoveNext())
                        count += ResumeCoroutines(linksEnum.Current);
                    Links.Add(indexesEnum.Current, links);
                }
            }

            return count;
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(string tag)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(tag);
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(string tag)
        {
            if (tag == null || !_taggedProcesses.ContainsKey(tag))
                return 0;
            int count = 0;

            var indexesEnum = _taggedProcesses[tag].GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (!CoindexIsNull(_handleToIndex[indexesEnum.Current]) && SetPause(_handleToIndex[indexesEnum.Current], false))
                {
                    count++;
                }

                if (Links.ContainsKey(indexesEnum.Current))
                {
                    var links = Links[indexesEnum.Current];
                    Links.Remove(indexesEnum.Current);
                    var linksEnum = links.GetEnumerator();
                    while (linksEnum.MoveNext())
                        count += ResumeCoroutines(linksEnum.Current);
                    Links.Add(indexesEnum.Current, links);
                }
            }

            return count;
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be resumed.</param>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(GameObject gameObj, string tag)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="gameObj">All coroutines on the layer corresponding with this GameObject will be resumed.</param>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(GameObject gameObj, string tag)
        {
            return gameObj == null ? 0 : ResumeCoroutinesOnInstance(gameObj.GetInstanceID(), tag);
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <param name="layer">Any coroutines previously paused on the matching layer will be resumend.</param>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(int layer, string tag)
        {
            return _instance == null ? 0 : _instance.ResumeCoroutinesOnInstance(layer, tag);
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="layer">Any coroutines previously paused on the matching layer will be resumend.</param>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(int layer, string tag)
        {
            if (tag == null)
                return ResumeCoroutinesOnInstance(layer);
            if (!_layeredProcesses.ContainsKey(layer) || !_taggedProcesses.ContainsKey(tag))
                return 0;
            int count = 0;

            var indexesEnum = _taggedProcesses[tag].GetEnumerator();
            while (indexesEnum.MoveNext())
            {
                if (!CoindexIsNull(_handleToIndex[indexesEnum.Current]) && _layeredProcesses[layer].Contains(indexesEnum.Current))
                {
                    if (SetPause(_handleToIndex[indexesEnum.Current], false))
                        count++;

                    if (Links.ContainsKey(indexesEnum.Current))
                    {
                        var links = Links[indexesEnum.Current];
                        Links.Remove(indexesEnum.Current);
                        var linksEnum = links.GetEnumerator();
                        while (linksEnum.MoveNext())
                            count += ResumeCoroutines(linksEnum.Current);
                        Links.Add(indexesEnum.Current, links);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the tag associated with the coroutine that the given handle points to, if it is running.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>The coroutine's tag, or null if there is no matching tag.</returns>
        public static string GetTag(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            return inst != null && inst._handleToIndex.ContainsKey(handle) && inst._processTags.ContainsKey(handle)
                 ? inst._processTags[handle] : null;
        }

        /// <summary>
        /// Returns the layer associated with the coroutine that the given handle points to, if it is running.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>The coroutine's layer as a nullable integer, or null if there is no matching layer.</returns>
        public static int? GetLayer(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            return inst != null && inst._handleToIndex.ContainsKey(handle) && inst._processLayers.ContainsKey(handle)
                  ? inst._processLayers[handle] : (int?)null;
        }

        /// <summary>
        /// Returns >NET's name for the coroutine that the handle points to.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>The underlying debug name of the coroutine, or info about the state of the coroutine.</returns>
        public static string GetDebugName(CoroutineHandle handle)
        {
            if (handle.Key == 0)
                return "Uninitialized handle";
            Timing inst = GetInstance(handle.Key);
            if (inst == null)
                return "Invalid handle";
            if (!inst._handleToIndex.ContainsKey(handle))
                return "Expired coroutine";

            return inst.CoindexPeek(inst._handleToIndex[handle]).ToString();
        }

        /// <summary>
        /// Returns the segment that the coroutine with the given handle is running on.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>The coroutine's segment, or Segment.Invalid if it's not found.</returns>
        public static Segment GetSegment(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            return inst != null && inst._handleToIndex.ContainsKey(handle) ? inst._handleToIndex[handle].seg : Segment.Invalid;
        }

        /// <summary>
        /// Sets the coroutine that the handle points to to have the given tag.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <param name="newTag">The new tag to assign, or null to clear the tag.</param>
        /// <param name="overwriteExisting">If set to false then the tag will not be changed if the coroutine has an existing tag.</param>
        /// <returns>Whether the tag was set successfully.</returns>
        public static bool SetTag(CoroutineHandle handle, string newTag, bool overwriteExisting = true)
        {
            Timing inst = GetInstance(handle.Key);
            if (inst == null || !inst._handleToIndex.ContainsKey(handle) || inst.CoindexIsNull(inst._handleToIndex[handle])
                || (!overwriteExisting && inst._processTags.ContainsKey(handle)))
                return false;

            inst.RemoveTagOnInstance(handle);
            inst.AddTagOnInstance(newTag, handle);

            return true;
        }

        /// <summary>
        /// Sets the coroutine that the handle points to to have the given layer.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <param name="newLayer">The new tag to assign.</param>
        /// <param name="overwriteExisting">If set to false then the tag will not be changed if the coroutine has an existing tag.</param>
        /// <returns>Whether the layer was set successfully.</returns>
        public static bool SetLayer(CoroutineHandle handle, int newLayer, bool overwriteExisting = true)
        {
            Timing inst = GetInstance(handle.Key);
            if (inst == null || !inst._handleToIndex.ContainsKey(handle) || inst.CoindexIsNull(inst._handleToIndex[handle])
                || (!overwriteExisting && inst._processLayers.ContainsKey(handle)))
                return false;

            inst.RemoveLayerOnInstance(handle);
            inst.AddLayerOnInstance(newLayer, handle);

            return true;
        }

        /// <summary>
        /// Sets the segment for the coroutine with the given handle.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <param name="newSegment">The new segment to run the coroutine in.</param>
        /// <returns>Whether the segment was set successfully.</returns>
        public static bool SetSegment(CoroutineHandle handle, Segment newSegment)
        {
            Timing inst = GetInstance(handle.Key);
            if (inst == null || !inst._handleToIndex.ContainsKey(handle) || inst.CoindexIsNull(inst._handleToIndex[handle]))
                return false;

            ProcessIndex coindex = inst._handleToIndex[handle];
            IEnumerator<float> procPtr = inst.CoindexExtract(coindex);
            bool coroutineHeld = inst.CoindexIsHeld(coindex);
            bool coroutinePaused = inst.CoindexIsPaused(coindex);

            if (procPtr.Current > inst.GetSegmentTime(coindex.seg))
                procPtr = inst._InjectDelay(procPtr, procPtr.Current - inst.GetSegmentTime(coindex.seg));

            inst.RunCoroutineInternal(procPtr, newSegment, 0, false, null, handle, false);

            coindex = inst._handleToIndex[handle];
            inst.SetHeld(coindex, coroutineHeld);
            inst.SetPause(coindex, coroutinePaused);

            return true;
        }

        /// <summary>
        /// Sets the coroutine that the handle points to to have the given tag.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>Whether the tag was removed successfully.</returns>
        public static bool RemoveTag(CoroutineHandle handle)
        {
            return SetTag(handle, null);
        }

        /// <summary>
        /// Sets the coroutine that the handle points to to have the given layer.
        /// </summary>
        /// <param name="handle">The handle to the coroutine.</param>
        /// <returns>Whether the layer was removed successfully.</returns>
        public static bool RemoveLayer(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            if (inst == null || !inst._handleToIndex.ContainsKey(handle) || inst.CoindexIsNull(inst._handleToIndex[handle]))
                return false;

            inst.RemoveLayerOnInstance(handle);

            return true;
        }

        /// <summary>
        /// Tests to see if the handle you have points to a valid coroutine that is currently running. Paused or held coroutines count as running.
        /// </summary>
        /// <param name="handle">The handle to test.</param>
        /// <returns>Whether it's a valid coroutine.</returns>
        public static bool IsRunning(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            return inst != null && inst._handleToIndex.ContainsKey(handle) && !inst.CoindexIsNull(inst._handleToIndex[handle]);
        }

        /// <summary>
        /// Tests to see if the handle you have points to a coroutine that has not ended but is paused.
        /// </summary>
        /// <param name="handle">The handle to test.</param>
        /// <returns>Whether it's a paused coroutine.</returns>
        public static bool IsAliveAndPaused(CoroutineHandle handle)
        {
            Timing inst = GetInstance(handle.Key);
            return inst != null && inst._handleToIndex.ContainsKey(handle) && !inst.CoindexIsNull(inst._handleToIndex[handle]) &&
                inst.CoindexIsPaused(inst._handleToIndex[handle]);
        }

        private void AddTagOnInstance(string tag, CoroutineHandle handle)
        {
            _processTags.Add(handle, tag);

            if (_taggedProcesses.ContainsKey(tag))
                _taggedProcesses[tag].Add(handle);
            else
                _taggedProcesses.Add(tag, new HashSet<CoroutineHandle> { handle });
        }

        private void AddLayerOnInstance(int layer, CoroutineHandle handle)
        {
            _processLayers.Add(handle, layer);

            if (_layeredProcesses.ContainsKey(layer))
                _layeredProcesses[layer].Add(handle);
            else
                _layeredProcesses.Add(layer, new HashSet<CoroutineHandle> { handle });
        }

        private void RemoveTagOnInstance(CoroutineHandle handle)
        {
            if (_processTags.ContainsKey(handle))
            {
                if (_taggedProcesses[_processTags[handle]].Count > 1)
                    _taggedProcesses[_processTags[handle]].Remove(handle);
                else
                    _taggedProcesses.Remove(_processTags[handle]);

                _processTags.Remove(handle);
            }
        }

        private void RemoveLayerOnInstance(CoroutineHandle handle)
        {
            if (_processLayers.ContainsKey(handle))
            {
                if (_layeredProcesses[_processLayers[handle]].Count > 1)
                    _layeredProcesses[_processLayers[handle]].Remove(handle);
                else
                    _layeredProcesses.Remove(_processLayers[handle]);

                _processLayers.Remove(handle);
            }
        }

        private void RemoveGraffiti(CoroutineHandle handle)
        {
            if (_processLayers.ContainsKey(handle))
            {
                if (_layeredProcesses[_processLayers[handle]].Count > 1)
                    _layeredProcesses[_processLayers[handle]].Remove(handle);
                else
                    _layeredProcesses.Remove(_processLayers[handle]);

                _processLayers.Remove(handle);
            }

            if (_processTags.ContainsKey(handle))
            {
                if (_taggedProcesses[_processTags[handle]].Count > 1)
                    _taggedProcesses[_processTags[handle]].Remove(handle);
                else
                    _taggedProcesses.Remove(_processTags[handle]);

                _processTags.Remove(handle);
            }
        }

        private IEnumerator<float> CoindexExtract(ProcessIndex coindex)
        {
            IEnumerator<float> retVal;

            switch (coindex.seg)
            {
                case Segment.Update:
                    retVal = UpdateProcesses[coindex.i];
                    UpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.FixedUpdate:
                    retVal = FixedUpdateProcesses[coindex.i];
                    FixedUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.LateUpdate:
                    retVal = LateUpdateProcesses[coindex.i];
                    LateUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.SlowUpdate:
                    retVal = SlowUpdateProcesses[coindex.i];
                    SlowUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.RealtimeUpdate:
                    retVal = RealtimeUpdateProcesses[coindex.i];
                    RealtimeUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EditorUpdate:
                    retVal = EditorUpdateProcesses[coindex.i];
                    EditorUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EditorSlowUpdate:
                    retVal = EditorSlowUpdateProcesses[coindex.i];
                    EditorSlowUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EndOfFrame:
                    retVal = EndOfFrameProcesses[coindex.i];
                    EndOfFrameProcesses[coindex.i] = null;
                    return retVal;
                case Segment.ManualTimeframe:
                    retVal = ManualTimeframeProcesses[coindex.i];
                    ManualTimeframeProcesses[coindex.i] = null;
                    return retVal;
                default:
                    return null;
            }
        }

        private bool CoindexIsNull(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdateProcesses[coindex.i] == null;
                case Segment.FixedUpdate:
                    return FixedUpdateProcesses[coindex.i] == null;
                case Segment.LateUpdate:
                    return LateUpdateProcesses[coindex.i] == null;
                case Segment.SlowUpdate:
                    return SlowUpdateProcesses[coindex.i] == null;
                case Segment.RealtimeUpdate:
                    return RealtimeUpdateProcesses[coindex.i] == null;
                case Segment.EditorUpdate:
                    return EditorUpdateProcesses[coindex.i] == null;
                case Segment.EditorSlowUpdate:
                    return EditorSlowUpdateProcesses[coindex.i] == null;
                case Segment.EndOfFrame:
                    return EndOfFrameProcesses[coindex.i] == null;
                case Segment.ManualTimeframe:
                    return ManualTimeframeProcesses[coindex.i] == null;
                default:
                    return true;
            }
        }

        private IEnumerator<float> CoindexPeek(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdateProcesses[coindex.i];
                case Segment.FixedUpdate:
                    return FixedUpdateProcesses[coindex.i];
                case Segment.LateUpdate:
                    return LateUpdateProcesses[coindex.i];
                case Segment.SlowUpdate:
                    return SlowUpdateProcesses[coindex.i];
                case Segment.RealtimeUpdate:
                    return RealtimeUpdateProcesses[coindex.i];
                case Segment.EditorUpdate:
                    return EditorUpdateProcesses[coindex.i];
                case Segment.EditorSlowUpdate:
                    return EditorSlowUpdateProcesses[coindex.i];
                case Segment.EndOfFrame:
                    return EndOfFrameProcesses[coindex.i];
                case Segment.ManualTimeframe:
                    return ManualTimeframeProcesses[coindex.i];
                default:
                    return null;
            }
        }

        /// <returns>Whether it was already null.</returns>
        private bool Nullify(CoroutineHandle handle)
        {
            return Nullify(_handleToIndex[handle]);
        }

        /// <returns>Whether it was already null.</returns>
        private bool Nullify(ProcessIndex coindex)
        {
            bool retVal;

            switch (coindex.seg)
            {
                case Segment.Update:
                    retVal = UpdateProcesses[coindex.i] != null;
                    UpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.FixedUpdate:
                    retVal = FixedUpdateProcesses[coindex.i] != null;
                    FixedUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.LateUpdate:
                    retVal = LateUpdateProcesses[coindex.i] != null;
                    LateUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.SlowUpdate:
                    retVal = SlowUpdateProcesses[coindex.i] != null;
                    SlowUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.RealtimeUpdate:
                    retVal = RealtimeUpdateProcesses[coindex.i] != null;
                    RealtimeUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EditorUpdate:
                    retVal = UpdateProcesses[coindex.i] != null;
                    EditorUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EditorSlowUpdate:
                    retVal = EditorSlowUpdateProcesses[coindex.i] != null;
                    EditorSlowUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.EndOfFrame:
                    retVal = EndOfFrameProcesses[coindex.i] != null;
                    EndOfFrameProcesses[coindex.i] = null;
                    return retVal;
                case Segment.ManualTimeframe:
                    retVal = ManualTimeframeProcesses[coindex.i] != null;
                    ManualTimeframeProcesses[coindex.i] = null;
                    return retVal;
                default:
                    return false;
            }
        }

        private bool SetPause(ProcessIndex coindex, bool newPausedState)
        {
            if (CoindexPeek(coindex) == null)
                return false;

            bool isPaused;

            switch (coindex.seg)
            {
                case Segment.Update:
                    isPaused = UpdatePaused[coindex.i];
                    UpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        UpdateProcesses[coindex.i] = _InjectDelay(UpdateProcesses[coindex.i],
                            UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.FixedUpdate:
                    isPaused = FixedUpdatePaused[coindex.i];
                    FixedUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        FixedUpdateProcesses[coindex.i] = _InjectDelay(FixedUpdateProcesses[coindex.i],
                            FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.LateUpdate:
                    isPaused = LateUpdatePaused[coindex.i];
                    LateUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        LateUpdateProcesses[coindex.i] = _InjectDelay(LateUpdateProcesses[coindex.i],
                            LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.SlowUpdate:
                    isPaused = SlowUpdatePaused[coindex.i];
                    SlowUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        SlowUpdateProcesses[coindex.i] = _InjectDelay(SlowUpdateProcesses[coindex.i],
                            SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.RealtimeUpdate:
                    isPaused = RealtimeUpdatePaused[coindex.i];
                    RealtimeUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && RealtimeUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        RealtimeUpdateProcesses[coindex.i] = _InjectDelay(RealtimeUpdateProcesses[coindex.i],
                            RealtimeUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.EditorUpdate:
                    isPaused = EditorUpdatePaused[coindex.i];
                    EditorUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && EditorUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EditorUpdateProcesses[coindex.i] = _InjectDelay(EditorUpdateProcesses[coindex.i],
                            EditorUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.EditorSlowUpdate:
                    isPaused = EditorSlowUpdatePaused[coindex.i];
                    EditorSlowUpdatePaused[coindex.i] = newPausedState;

                    if (newPausedState && EditorSlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EditorSlowUpdateProcesses[coindex.i] = _InjectDelay(EditorSlowUpdateProcesses[coindex.i],
                            EditorSlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.EndOfFrame:
                    isPaused = EndOfFramePaused[coindex.i];
                    EndOfFramePaused[coindex.i] = newPausedState;

                    if (newPausedState && EndOfFrameProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EndOfFrameProcesses[coindex.i] = _InjectDelay(EndOfFrameProcesses[coindex.i],
                            EndOfFrameProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                case Segment.ManualTimeframe:
                    isPaused = ManualTimeframePaused[coindex.i];
                    ManualTimeframePaused[coindex.i] = newPausedState;

                    if (newPausedState && ManualTimeframeProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        ManualTimeframeProcesses[coindex.i] = _InjectDelay(ManualTimeframeProcesses[coindex.i],
                            ManualTimeframeProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isPaused;
                default:
                    return false;
            }
        }

        private bool SetHeld(ProcessIndex coindex, bool newHeldState)
        {
            if (CoindexPeek(coindex) == null)
                return false;

            bool isHeld;

            switch (coindex.seg)
            {
                case Segment.Update:
                    isHeld = UpdateHeld[coindex.i];
                    UpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        UpdateProcesses[coindex.i] = _InjectDelay(UpdateProcesses[coindex.i],
                            UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.FixedUpdate:
                    isHeld = FixedUpdateHeld[coindex.i];
                    FixedUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        FixedUpdateProcesses[coindex.i] = _InjectDelay(FixedUpdateProcesses[coindex.i],
                            FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.LateUpdate:
                    isHeld = LateUpdateHeld[coindex.i];
                    LateUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        LateUpdateProcesses[coindex.i] = _InjectDelay(LateUpdateProcesses[coindex.i],
                            LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.SlowUpdate:
                    isHeld = SlowUpdateHeld[coindex.i];
                    SlowUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        SlowUpdateProcesses[coindex.i] = _InjectDelay(SlowUpdateProcesses[coindex.i],
                            SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.RealtimeUpdate:
                    isHeld = RealtimeUpdateHeld[coindex.i];
                    RealtimeUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && RealtimeUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        RealtimeUpdateProcesses[coindex.i] = _InjectDelay(RealtimeUpdateProcesses[coindex.i],
                            RealtimeUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.EditorUpdate:
                    isHeld = EditorUpdateHeld[coindex.i];
                    EditorUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && EditorUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EditorUpdateProcesses[coindex.i] = _InjectDelay(EditorUpdateProcesses[coindex.i],
                            EditorUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.EditorSlowUpdate:
                    isHeld = EditorSlowUpdateHeld[coindex.i];
                    EditorSlowUpdateHeld[coindex.i] = newHeldState;

                    if (newHeldState && EditorSlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EditorSlowUpdateProcesses[coindex.i] = _InjectDelay(EditorSlowUpdateProcesses[coindex.i],
                            EditorSlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.EndOfFrame:
                    isHeld = EndOfFrameHeld[coindex.i];
                    EndOfFrameHeld[coindex.i] = newHeldState;

                    if (newHeldState && EndOfFrameProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        EndOfFrameProcesses[coindex.i] = _InjectDelay(EndOfFrameProcesses[coindex.i],
                            EndOfFrameProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                case Segment.ManualTimeframe:
                    isHeld = ManualTimeframeHeld[coindex.i];
                    ManualTimeframeHeld[coindex.i] = newHeldState;

                    if (newHeldState && ManualTimeframeProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        ManualTimeframeProcesses[coindex.i] = _InjectDelay(ManualTimeframeProcesses[coindex.i],
                            ManualTimeframeProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return isHeld;
                default:
                    return false;
            }
        }

        private IEnumerator<float> CreateHold(ProcessIndex coindex, IEnumerator<float> coptr)
        {
            if (CoindexPeek(coindex) == null)
                return null;

            switch (coindex.seg)
            {
                case Segment.Update:
                    UpdateHeld[coindex.i] = true;

                    if (UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(UpdateProcesses[coindex.i], UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.FixedUpdate:
                    FixedUpdateHeld[coindex.i] = true;

                    if (FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(FixedUpdateProcesses[coindex.i],
                            FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.LateUpdate:
                    LateUpdateHeld[coindex.i] = true;

                    if (LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(LateUpdateProcesses[coindex.i],
                            LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.SlowUpdate:
                    SlowUpdateHeld[coindex.i] = true;

                    if (SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(SlowUpdateProcesses[coindex.i],
                            SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.RealtimeUpdate:
                    RealtimeUpdateHeld[coindex.i] = true;

                    if (RealtimeUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(RealtimeUpdateProcesses[coindex.i],
                            RealtimeUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.EditorUpdate:
                    EditorUpdateHeld[coindex.i] = true;

                    if (EditorUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(EditorUpdateProcesses[coindex.i],
                            EditorUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.EditorSlowUpdate:
                    EditorSlowUpdateHeld[coindex.i] = true;

                    if (EditorSlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(EditorSlowUpdateProcesses[coindex.i],
                            EditorSlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.EndOfFrame:
                    EndOfFrameHeld[coindex.i] = true;

                    if (EndOfFrameProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(EndOfFrameProcesses[coindex.i],
                            EndOfFrameProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                case Segment.ManualTimeframe:
                    ManualTimeframeHeld[coindex.i] = true;

                    if (ManualTimeframeProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
                        coptr = _InjectDelay(ManualTimeframeProcesses[coindex.i],
                            ManualTimeframeProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));

                    return coptr;
                default:
                    return coptr;
            }
        }

        private bool CoindexIsPaused(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdatePaused[coindex.i];
                case Segment.FixedUpdate:
                    return FixedUpdatePaused[coindex.i];
                case Segment.LateUpdate:
                    return LateUpdatePaused[coindex.i];
                case Segment.SlowUpdate:
                    return SlowUpdatePaused[coindex.i];
                case Segment.RealtimeUpdate:
                    return RealtimeUpdatePaused[coindex.i];
                case Segment.EditorUpdate:
                    return EditorUpdatePaused[coindex.i];
                case Segment.EditorSlowUpdate:
                    return EditorSlowUpdatePaused[coindex.i];
                case Segment.EndOfFrame:
                    return EndOfFramePaused[coindex.i];
                case Segment.ManualTimeframe:
                    return ManualTimeframePaused[coindex.i];
                default:
                    return false;
            }
        }

        private bool CoindexIsHeld(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdateHeld[coindex.i];
                case Segment.FixedUpdate:
                    return FixedUpdateHeld[coindex.i];
                case Segment.LateUpdate:
                    return LateUpdateHeld[coindex.i];
                case Segment.SlowUpdate:
                    return SlowUpdateHeld[coindex.i];
                case Segment.RealtimeUpdate:
                    return RealtimeUpdateHeld[coindex.i];
                case Segment.EditorUpdate:
                    return EditorUpdateHeld[coindex.i];
                case Segment.EditorSlowUpdate:
                    return EditorSlowUpdateHeld[coindex.i];
                case Segment.EndOfFrame:
                    return EndOfFrameHeld[coindex.i];
                case Segment.ManualTimeframe:
                    return ManualTimeframeHeld[coindex.i];
                default:
                    return false;
            }
        }

        private void CoindexReplace(ProcessIndex coindex, IEnumerator<float> replacement)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    UpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.FixedUpdate:
                    FixedUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.LateUpdate:
                    LateUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.SlowUpdate:
                    SlowUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.RealtimeUpdate:
                    RealtimeUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.EditorUpdate:
                    EditorUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.EditorSlowUpdate:
                    EditorSlowUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.EndOfFrame:
                    EndOfFrameProcesses[coindex.i] = replacement;
                    return;
                case Segment.ManualTimeframe:
                    ManualTimeframeProcesses[coindex.i] = replacement;
                    return;
            }
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, CurrentCoroutine.Segment), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, string tag)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, CurrentCoroutine.Segment, tag), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, int layer)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, CurrentCoroutine.Segment, layer), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, int layer, string tag)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, CurrentCoroutine.Segment, layer, tag), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, segment), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, string tag)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, segment, tag), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, int layer)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, segment, layer), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(newCoroutine);" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(IEnumerator<float> newCoroutine, Segment segment, int layer, string tag)
        {
            return WaitUntilDone(RunCoroutine(newCoroutine, segment, layer, tag), true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        public static float WaitUntilDone(CoroutineHandle otherCoroutine)
        {
            return WaitUntilDone(otherCoroutine, true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine, false);" to pause the current 
        /// coroutine until otherCoroutine is done, supressing warnings.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        /// <param name="warnOnIssue">Post a warning if no hold action was actually performed.</param>
        public static float WaitUntilDone(CoroutineHandle otherCoroutine, bool warnOnIssue)
        {
            Timing inst = GetInstance(otherCoroutine.Key);

            if (inst != null && inst._handleToIndex.ContainsKey(otherCoroutine))
            {
                if (inst.CoindexIsNull(inst._handleToIndex[otherCoroutine]))
                    return 0f;

                if (!inst._waitingTriggers.ContainsKey(otherCoroutine))
                {
                    inst.CoindexReplace(inst._handleToIndex[otherCoroutine],
                        inst._StartWhenDone(otherCoroutine, inst.CoindexPeek(inst._handleToIndex[otherCoroutine])));
                    inst._waitingTriggers.Add(otherCoroutine, new HashSet<CoroutineHandle>());
                }

                if (inst.currentCoroutine == otherCoroutine)
                {
                    Assert.IsFalse(warnOnIssue, "A coroutine cannot wait for itself.");
                    return WaitForOneFrame;
                }
                if (!inst.currentCoroutine.IsValid)
                {
                    Assert.IsFalse(warnOnIssue, "The two coroutines are not running on the same MEC instance.");
                    return WaitForOneFrame;
                }

                inst._waitingTriggers[otherCoroutine].Add(inst.currentCoroutine);
                if (!inst._allWaiting.Contains(inst.currentCoroutine))
                    inst._allWaiting.Add(inst.currentCoroutine);
                inst.SetHeld(inst._handleToIndex[inst.currentCoroutine], true);
                inst.SwapToLast(otherCoroutine, inst.currentCoroutine);

                return float.NaN;
            }

            Assert.IsFalse(warnOnIssue, "WaitUntilDone cannot hold, the coroutine handle that was passed in is invalid: " + otherCoroutine);
            return 0f;
        }

        /// <summary>
        /// This will pause one coroutine until another coroutine finishes running. Note: This is NOT used with a yield return statement.
        /// </summary>
        /// <param name="handle">The coroutine that should be paused.</param>
        /// <param name="otherHandle">The coroutine that will be waited for.</param>
        /// <param name="warnOnIssue">Whether a warning should be logged if there is a problem.</param>
        public static void WaitForOtherHandles(CoroutineHandle handle, CoroutineHandle otherHandle, bool warnOnIssue = true)
        {
            if (!IsRunning(handle) || !IsRunning(otherHandle))
                return;

            if (handle == otherHandle)
            {
                Assert.IsFalse(warnOnIssue, "A coroutine cannot wait for itself.");
                return;
            }

            if (handle.Key != otherHandle.Key)
            {
                Assert.IsFalse(warnOnIssue, "A coroutine cannot wait for another coroutine on a different MEC instance.");
                return;
            }

            Timing inst = GetInstance(handle.Key);

            if (inst != null && inst._handleToIndex.ContainsKey(handle) && inst._handleToIndex.ContainsKey(otherHandle) &&
                !inst.CoindexIsNull(inst._handleToIndex[otherHandle]))
            {
                if (!inst._waitingTriggers.ContainsKey(otherHandle))
                {
                    inst.CoindexReplace(inst._handleToIndex[otherHandle],
                        inst._StartWhenDone(otherHandle, inst.CoindexPeek(inst._handleToIndex[otherHandle])));
                    inst._waitingTriggers.Add(otherHandle, new HashSet<CoroutineHandle>());
                }

                inst._waitingTriggers[otherHandle].Add(handle);
                if (!inst._allWaiting.Contains(handle))
                    inst._allWaiting.Add(handle);
                inst.SetHeld(inst._handleToIndex[handle], true);
                inst.SwapToLast(otherHandle, handle);
            }
        }

        /// <summary>
        /// This will pause one coroutine until the other coroutines finish running. Note: This is NOT used with a yield return statement.
        /// </summary>
        /// <param name="handle">The coroutine that should be paused.</param>
        /// <param name="otherHandles">A list of coroutines to be waited for.</param>
        /// <param name="warnOnIssue">Whether a warning should be logged if there is a problem.</param>
        public static void WaitForOtherHandles(CoroutineHandle handle, IEnumerable<CoroutineHandle> otherHandles, bool warnOnIssue = true)
        {
            if (!IsRunning(handle))
                return;

            Timing inst = GetInstance(handle.Key);

            var othersEnum = otherHandles.GetEnumerator();
            while (othersEnum.MoveNext())
            {
                if (!IsRunning(othersEnum.Current))
                    continue;

                if (handle == othersEnum.Current)
                {
                    Assert.IsFalse(warnOnIssue, "A coroutine cannot wait for itself.");
                    continue;
                }

                if (handle.Key != othersEnum.Current.Key)
                {
                    Assert.IsFalse(warnOnIssue, "A coroutine cannot wait for another coroutine on a different MEC instance.");
                    continue;
                }

                if (!inst._waitingTriggers.ContainsKey(othersEnum.Current))
                {
                    inst.CoindexReplace(inst._handleToIndex[othersEnum.Current],
                        inst._StartWhenDone(othersEnum.Current, inst.CoindexPeek(inst._handleToIndex[othersEnum.Current])));
                    inst._waitingTriggers.Add(othersEnum.Current, new HashSet<CoroutineHandle>());
                }

                inst._waitingTriggers[othersEnum.Current].Add(handle);
                if (!inst._allWaiting.Contains(handle))
                    inst._allWaiting.Add(handle);
                inst.SetHeld(inst._handleToIndex[handle], true);
                inst.SwapToLast(othersEnum.Current, handle);
            }
        }

        private void SwapToLast(CoroutineHandle firstHandle, CoroutineHandle lastHandle)
        {
            if (firstHandle.Key != lastHandle.Key)
                return;

            ProcessIndex firstIndex = _handleToIndex[firstHandle];
            ProcessIndex lastIndex = _handleToIndex[lastHandle];

            if (firstIndex.seg != lastIndex.seg || firstIndex.i <= lastIndex.i)
                return;

            IEnumerator<float> tempCoptr = CoindexPeek(firstIndex);
            CoindexReplace(firstIndex, CoindexPeek(lastIndex));
            CoindexReplace(lastIndex, tempCoptr);

            _indexToHandle[firstIndex] = lastHandle;
            _indexToHandle[lastIndex] = firstHandle;
            _handleToIndex[firstHandle] = lastIndex;
            _handleToIndex[lastHandle] = firstIndex;
            bool tmpB = SetPause(firstIndex, CoindexIsPaused(lastIndex));
            SetPause(lastIndex, tmpB);
            tmpB = SetHeld(firstIndex, CoindexIsHeld(lastIndex));
            SetHeld(lastIndex, tmpB);

            if (_waitingTriggers.ContainsKey(lastHandle))
            {
                var trigsEnum = _waitingTriggers[lastHandle].GetEnumerator();
                while (trigsEnum.MoveNext())
                    SwapToLast(lastHandle, trigsEnum.Current);
            }

            if (_allWaiting.Contains(firstHandle))
            {
                var keyEnum = _waitingTriggers.GetEnumerator();
                while (keyEnum.MoveNext())
                {
                    var valueEnum = keyEnum.Current.Value.GetEnumerator();
                    while (valueEnum.MoveNext())
                        if (valueEnum.Current == firstHandle)
                            SwapToLast(keyEnum.Current.Key, firstHandle);
                }
            }
        }

        private IEnumerator<float> _StartWhenDone(CoroutineHandle handle, IEnumerator<float> proc)
        {
            if (!_waitingTriggers.ContainsKey(handle)) yield break;

            try
            {
                if (proc.Current > localTime)
                    yield return proc.Current;

                while (proc.MoveNext())
                    yield return proc.Current;
            }
            finally
            {
                CloseWaitingProcess(handle);
            }
        }

        private void CloseWaitingProcess(CoroutineHandle handle)
        {
            if (!_waitingTriggers.ContainsKey(handle)) return;

            var tasksEnum = _waitingTriggers[handle].GetEnumerator();
            _waitingTriggers.Remove(handle);

            while (tasksEnum.MoveNext())
            {
                if (_handleToIndex.ContainsKey(tasksEnum.Current) && !HandleIsInWaitingList(tasksEnum.Current))
                {
                    SetHeld(_handleToIndex[tasksEnum.Current], false);
                    _allWaiting.Remove(tasksEnum.Current);
                }
            }
        }

        private bool HandleIsInWaitingList(CoroutineHandle handle)
        {
            var triggersEnum = _waitingTriggers.GetEnumerator();
            while (triggersEnum.MoveNext())
                if (triggersEnum.Current.Value.Contains(handle))
                    return true;

            return false;
        }

        private static IEnumerator<float> ReturnTmpRefForRepFunc(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            return _tmpRef as IEnumerator<float>;
        }

#if !UNITY_2018_3_OR_NEWER
        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(wwwObject);" to pause the current 
        /// coroutine until the wwwObject is done.
        /// </summary>
        /// <param name="wwwObject">The www object to pause for.</param>
        public static float WaitUntilDone(WWW wwwObject)
        {
            if (wwwObject == null || wwwObject.isDone) return 0f;

            _tmpRef = wwwObject;
            ReplacementFunction = WaitUntilDoneWwwHelper;
            return float.NaN;
        }

        private static IEnumerator<float> WaitUntilDoneWwwHelper(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            return _StartWhenDone(_tmpRef as WWW, coptr);
        }

        private static IEnumerator<float> _StartWhenDone(WWW wwwObject, IEnumerator<float> pausedProc)
        {
            while (!wwwObject.isDone)
                yield return WaitForOneFrame;

            _tmpRef = pausedProc;
            ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return float.NaN;
        }
#endif

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(operation);" to pause the current 
        /// coroutine until the operation is done.
        /// </summary>
        /// <param name="operation">The operation variable returned.</param>
        public static float WaitUntilDone(AsyncOperation operation)
        {
            if (operation == null || operation.isDone) return float.NaN;

            CoroutineHandle handle = CurrentCoroutine;
            Timing inst = GetInstance(CurrentCoroutine.Key);
            if (inst == null) return float.NaN;

            _tmpRef = _StartWhenDone(operation, inst.CoindexPeek(inst._handleToIndex[handle]));
            ReplacementFunction = ReturnTmpRefForRepFunc;
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(AsyncOperation operation, IEnumerator<float> pausedProc)
        {
            while (!operation.isDone)
                yield return WaitForOneFrame;

            _tmpRef = pausedProc;
            ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return float.NaN;
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(operation);" to pause the current 
        /// coroutine until the operation is done.
        /// </summary>
        /// <param name="operation">The operation variable returned.</param>
        public static float WaitUntilDone(CustomYieldInstruction operation)
        {
            if (operation == null || !operation.keepWaiting) return float.NaN;

            CoroutineHandle handle = CurrentCoroutine;
            Timing inst = GetInstance(CurrentCoroutine.Key);
            if (inst == null) return float.NaN;

            _tmpRef = _StartWhenDone(operation, inst.CoindexPeek(inst._handleToIndex[handle]));
            ReplacementFunction = ReturnTmpRefForRepFunc;
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(CustomYieldInstruction operation, IEnumerator<float> pausedProc)
        {
            while (operation.keepWaiting)
                yield return WaitForOneFrame;

            _tmpRef = pausedProc;
            ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return float.NaN;
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilTrue(evaluatorFunc);" to pause the current 
        /// coroutine until the evaluator function returns true.
        /// </summary>
        /// <param name="evaluatorFunc">The evaluator function.</param>
        public static float WaitUntilTrue(System.Func<bool> evaluatorFunc)
        {
            if (evaluatorFunc == null || evaluatorFunc()) return float.NaN;
            _tmpRef = evaluatorFunc;
            ReplacementFunction = WaitUntilTrueHelper;
            return float.NaN;
        }

        private static IEnumerator<float> WaitUntilTrueHelper(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            return _StartWhenDone(_tmpRef as System.Func<bool>, false, coptr);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilFalse(evaluatorFunc);" to pause the current 
        /// coroutine until the evaluator function returns false.
        /// </summary>
        /// <param name="evaluatorFunc">The evaluator function.</param>
        public static float WaitUntilFalse(System.Func<bool> evaluatorFunc)
        {
            if (evaluatorFunc == null || !evaluatorFunc()) return float.NaN;
            _tmpRef = evaluatorFunc;
            ReplacementFunction = WaitUntilFalseHelper;
            return float.NaN;
        }

        private static IEnumerator<float> WaitUntilFalseHelper(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            return _StartWhenDone(_tmpRef as System.Func<bool>, true, coptr);
        }

        private static IEnumerator<float> _StartWhenDone(System.Func<bool> evaluatorFunc, bool continueOn, IEnumerator<float> pausedProc)
        {
            while (evaluatorFunc() == continueOn)
                yield return WaitForOneFrame;

            _tmpRef = pausedProc;
            ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return float.NaN;
        }

        private IEnumerator<float> _InjectDelay(IEnumerator<float> proc, float waitTime)
        {
            yield return WaitForSecondsOnInstance(waitTime);

            _tmpRef = proc;
            ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return float.NaN;
        }

        /// <summary>
        /// Keeps this coroutine from executing until UnlockCoroutine is called with a matching key.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine to be locked.</param>
        /// <param name="key">The key to use. A new key can be generated by calling "new CoroutineHandle(0)".</param>
        /// <returns>Whether the lock was successful.</returns>
        public bool LockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
        {
            if (coroutine.Key != _instanceID || key == new CoroutineHandle() || key.Key != 0)
                return false;

            if (!_waitingTriggers.ContainsKey(key))
                _waitingTriggers.Add(key, new HashSet<CoroutineHandle> { coroutine });
            else
                _waitingTriggers[key].Add(coroutine);

            _allWaiting.Add(coroutine);

            SetHeld(_handleToIndex[coroutine], true);

            return true;
        }

        /// <summary>
        /// Unlocks a coroutine that has been locked, so long as the key matches.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine to be unlocked.</param>
        /// <param name="key">The key that the coroutine was previously locked with.</param>
        /// <returns>Whether the coroutine was successfully unlocked.</returns>
        public bool UnlockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
        {
            if (coroutine.Key != _instanceID || key == new CoroutineHandle() ||
                !_handleToIndex.ContainsKey(coroutine) || !_waitingTriggers.ContainsKey(key))
                return false;

            if (_waitingTriggers[key].Count == 1)
                _waitingTriggers.Remove(key);
            else
                _waitingTriggers[key].Remove(coroutine);

            if (!HandleIsInWaitingList(coroutine))
            {
                SetHeld(_handleToIndex[coroutine], false);
                _allWaiting.Remove(coroutine);
            }

            return true;
        }

        /// <summary>
        /// This will create a one way link between two handles. If the master coroutine ends for any reason or is paused or resumed
        /// that will also happen to the slave coroutine. If a two way link is desired then just call this funciton twice with 
        /// parameters reversed.
        /// </summary>
        /// <param name="master">The coroutine that generates the link events</param>
        /// <param name="slave">The coroutine that recieves the link events</param>
        /// <returns>The number of coroutines that were linked.</returns>
        public static int LinkCoroutines(CoroutineHandle master, CoroutineHandle slave)
        {
            if (!IsRunning(slave) || !master.IsValid)
            {
                return 0;
            }
            else if (!IsRunning(master))
            {
                KillCoroutines(slave);
                return 1;
            }

            if (Links.ContainsKey(master))
            {
                if (!Links[master].Contains(slave))
                {
                    Links[master].Add(slave);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                Links.Add(master, new HashSet<CoroutineHandle> { slave });
                return 1;
            }
        }

        /// <summary>
        /// This will remove the link between two coroutine handles if one exists.
        /// </summary>
        /// <param name="master">The coroutine that generates the link events</param>
        /// <param name="slave">The coroutine that recieves the link events</param>
        /// <param name="twoWay">if true this will also remove any links that exist from the slave to the master</param>
        /// <returns>The number of coroutines that were unlinked.</returns>
        public static int UnlinkCoroutines(CoroutineHandle master, CoroutineHandle slave, bool twoWay = false)
        {
            int count = 0;
            if (Links.ContainsKey(master) && Links[master].Contains(slave))
            {
                if (Links[master].Count <= 1)
                    Links.Remove(master);
                else
                    Links[master].Remove(slave);

                count++;
            }

            if (twoWay && Links.ContainsKey(slave) && Links[slave].Contains(master))
            {
                if (Links[slave].Count <= 1)
                    Links.Remove(slave);
                else
                    Links[slave].Remove(master);

                count++;
            }

            return count;
        }

        [System.Obsolete("Use Timing.CurrentCoroutine instead.", false)]
        public static float GetMyHandle(System.Action<CoroutineHandle> reciever)
        {
            _tmpRef = reciever;
            ReplacementFunction = GetHandleHelper;
            return float.NaN;
        }

        private static IEnumerator<float> GetHandleHelper(IEnumerator<float> input, CoroutineHandle handle)
        {
            System.Action<CoroutineHandle> reciever = _tmpRef as System.Action<CoroutineHandle>;
            if (reciever != null)
                reciever(handle);
            return input;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(segment);" to switch this coroutine to
        /// the given segment on the default instance.
        /// </summary>
        /// <param name="newSegment">The new segment to run in.</param>
        public static float SwitchCoroutine(Segment newSegment)
        {
            _tmpSegment = newSegment;
            ReplacementFunction = SwitchCoroutineRepS;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepS(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            instance.RunCoroutineInternal(coptr, _tmpSegment, 0, false, null, handle, false);
            return null;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(segment, tag);" to switch this coroutine to
        /// the given values.
        /// </summary>
        /// <param name="newSegment">The new segment to run in.</param>
        /// <param name="newTag">The new tag to apply, or null to remove this coroutine's tag.</param>
        public static float SwitchCoroutine(Segment newSegment, string newTag)
        {
            _tmpSegment = newSegment;
            _tmpRef = newTag;
            ReplacementFunction = SwitchCoroutineRepST;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepST(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            instance.RemoveTagOnInstance(handle);
            if ((_tmpRef as string) != null)
                instance.AddTagOnInstance((string)_tmpRef, handle);
            instance.RunCoroutineInternal(coptr, _tmpSegment, 0, false, null, handle, false);
            return null;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(segment, layer);" to switch this coroutine to
        /// the given values.
        /// </summary>
        /// <param name="newSegment">The new segment to run in.</param>
        /// <param name="newLayer">The new layer to apply.</param>
        public static float SwitchCoroutine(Segment newSegment, int newLayer)
        {
            _tmpSegment = newSegment;
            _tmpInt = newLayer;
            ReplacementFunction = SwitchCoroutineRepSL;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepSL(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            RemoveLayer(handle);
            instance.AddLayerOnInstance(_tmpInt, handle);
            instance.RunCoroutineInternal(coptr, _tmpSegment, _tmpInt, false, null, handle, false);
            return null;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(segment, layer, tag);" to switch this coroutine to
        /// the given values.
        /// </summary>
        /// <param name="newSegment">The new segment to run in.</param>
        /// <param name="newLayer">The new layer to apply.</param>
        /// <param name="newTag">The new tag to apply, or null to remove this coroutine's tag.</param>
        public static float SwitchCoroutine(Segment newSegment, int newLayer, string newTag)
        {
            _tmpSegment = newSegment;
            _tmpInt = newLayer;
            _tmpRef = newTag;
            ReplacementFunction = SwitchCoroutineRepSLT;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepSLT(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            instance.RemoveTagOnInstance(handle);
            if ((_tmpRef as string) != null)
                instance.AddTagOnInstance((string)_tmpRef, handle);
            RemoveLayer(handle);
            instance.AddLayerOnInstance(_tmpInt, handle);
            instance.RunCoroutineInternal(coptr, _tmpSegment, _tmpInt, false, null, handle, false);
            return null;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(tag);" to switch this coroutine to
        /// the given tag.
        /// </summary>
        /// <param name="newTag">The new tag to apply, or null to remove this coroutine's tag.</param>
        public static float SwitchCoroutine(string newTag)
        {
            _tmpRef = newTag;
            ReplacementFunction = SwitchCoroutineRepT;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepT(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            instance.RemoveTagOnInstance(handle);
            if ((_tmpRef as string) != null)
                instance.AddTagOnInstance((string)_tmpRef, handle);
            return coptr;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(layer);" to switch this coroutine to
        /// the given layer.
        /// </summary>
        /// <param name="newLayer">The new layer to apply.</param>
        public static float SwitchCoroutine(int newLayer)
        {
            _tmpInt = newLayer;
            ReplacementFunction = SwitchCoroutineRepL;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepL(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            RemoveLayer(handle);
            GetInstance(handle.Key).AddLayerOnInstance(_tmpInt, handle);
            return coptr;
        }

        /// <summary>
        /// Use the command "yield return Timing.SwitchCoroutine(layer, tag);" to switch this coroutine to
        /// the given tag.
        /// </summary>
        /// <param name="newLayer">The new layer to apply.</param>
        /// <param name="newTag">The new tag to apply, or null to remove this coroutine's tag.</param>
        public static float SwitchCoroutine(int newLayer, string newTag)
        {
            _tmpInt = newLayer;
            _tmpRef = newTag;
            ReplacementFunction = SwitchCoroutineRepLT;

            return float.NaN;
        }

        private static IEnumerator<float> SwitchCoroutineRepLT(IEnumerator<float> coptr, CoroutineHandle handle)
        {
            Timing instance = GetInstance(handle.Key);
            instance.RemoveLayerOnInstance(handle);
            instance.AddLayerOnInstance(_tmpInt, handle);
            instance.RemoveTagOnInstance(handle);
            if ((_tmpRef as string) != null)
                instance.AddTagOnInstance((string)_tmpRef, handle);

            return coptr;
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallDelayed(float delay, System.Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._DelayedCall(delay, action, null));
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallDelayedOnInstance(float delay, System.Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_DelayedCall(delay, action, null));
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed 
        /// before calling the action.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallDelayed(float delay, System.Action action, GameObject gameObject)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._DelayedCall(delay, action, gameObject), gameObject);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed 
        /// before calling the action.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallDelayedOnInstance(float delay, System.Action action, GameObject gameObject)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_DelayedCall(delay, action, gameObject), gameObject);
        }

        private IEnumerator<float> _DelayedCall(float delay, System.Action action, GameObject cancelWith)
        {
            yield return WaitForSecondsOnInstance(delay);

            if (ReferenceEquals(cancelWith, null) || cancelWith != null)
                action();
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="segment">The timing segment that the call should be made in.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallDelayed(float delay, Segment segment, System.Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._DelayedCall(delay, action, null), segment);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="segment">The timing segment that the call should be made in.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallDelayedOnInstance(float delay, Segment segment, System.Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_DelayedCall(delay, action, null), segment);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed 
        /// before calling the action.</param>
        /// <param name="segment">The timing segment that the call should be made in.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallDelayed(float delay, Segment segment, System.Action action, GameObject gameObject)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._DelayedCall(delay, action, gameObject), segment, gameObject);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed 
        /// before calling the action.</param>
        /// <param name="segment">The timing segment that the call should be made in.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallDelayedOnInstance(float delay, Segment segment, System.Action action, GameObject gameObject)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_DelayedCall(delay, action, gameObject), segment, gameObject);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, System.Action action, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(period, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                RunCoroutine(Instance._WatchCall(timeframe, handle, null, onDone));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, System.Action action, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(period, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                RunCoroutineOnInstance(_WatchCall(timeframe, handle, null, onDone));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, System.Action action,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(period, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, System.Action action,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(period, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, System.Action action, Segment timing, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(period, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                RunCoroutine(Instance._WatchCall(timeframe, handle, null, onDone), timing);
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, System.Action action, Segment timing, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(period, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, System.Action action, Segment timing,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(period, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, System.Action action, Segment timing,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(period, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, System.Action action, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(0f, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, System.Action action, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(0f, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, System.Action action, GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(0f, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, System.Action action, GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(0f, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, System.Action action, Segment timing, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(0f, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, System.Action action, Segment timing, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(0f, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, System.Action action, Segment timing,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(0f, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, System.Action action, Segment timing,
            GameObject gameObject, System.Action onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(0f, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        private IEnumerator<float> _WatchCall(float timeframe, CoroutineHandle handle, GameObject gObject, System.Action onDone)
        {
            yield return WaitForSecondsOnInstance(timeframe);

            KillCoroutinesOnInstance(handle);

            if (onDone != null && (ReferenceEquals(gObject, null) || gObject != null))
                onDone();
        }

        private IEnumerator<float> _CallContinuously(float period, System.Action action, GameObject gObject)
        {
            while (ReferenceEquals(gObject, null) || gObject != null)
            {
                yield return WaitForSecondsOnInstance(period);

                if (ReferenceEquals(gObject, null) || (gObject != null && gObject.activeInHierarchy))
                    action();
            }
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period,
            System.Action<T> action, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(reference, period, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period,
            System.Action<T> action, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(reference, period, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period,
            System.Action<T> action, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, period, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period,
            System.Action<T> action, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, period, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, System.Action<T> action,
            Segment timing, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, period, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, System.Action<T> action,
            Segment timing, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, period, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, System.Action<T> action,
            Segment timing, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, period, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, System.Action<T> action,
            Segment timing, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, period, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(reference, 0f, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, null));
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, null, onDone)));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, System.Action<T> action,
            GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, 0f, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, System.Action<T> action,
            GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, gameObject), gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, gameObject, onDone), gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, System.Action<T> action,
            Segment timing, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, 0f, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, System.Action<T> action,
            Segment timing, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, null), timing);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, null, onDone), timing));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, System.Action<T> action,
            Segment timing, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutine(Instance._CallContinuously(reference, 0f, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutine(Instance._WatchCall(reference, timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run. Use float.PositiveInfinity to run indefinitely.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="gameObject">A GameObject that will be tagged onto the coroutine and checked to make sure it hasn't been destroyed or disabled
        /// before calling the action.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, System.Action<T> action,
            Segment timing, GameObject gameObject, System.Action<T> onDone = null)
        {
            CoroutineHandle handle = action == null ? new CoroutineHandle()
                : RunCoroutineOnInstance(_CallContinuously(reference, 0f, action, gameObject), timing, gameObject);
            if (!float.IsPositiveInfinity(timeframe))
                LinkCoroutines(handle, RunCoroutineOnInstance(_WatchCall(reference, timeframe, handle, gameObject, onDone), timing, gameObject));
            return handle;
        }

        private IEnumerator<float> _WatchCall<T>(T reference, float timeframe, CoroutineHandle handle, GameObject gObject, System.Action<T> onDone)
        {
            yield return WaitForSecondsOnInstance(timeframe);

            KillCoroutinesOnInstance(handle);

            if (onDone != null && (ReferenceEquals(gObject, null) || gObject != null))
                onDone(reference);
        }

        private IEnumerator<float> _CallContinuously<T>(T reference, float period, System.Action<T> action, GameObject gObject)
        {
            while ((ReferenceEquals(gObject, null) || gObject != null))
            {
                yield return WaitForSecondsOnInstance(period);

                if (ReferenceEquals(gObject, null) || (gObject != null && gObject.activeInHierarchy))
                    action(reference);
            }
        }

        private struct ProcessIndex : System.IEquatable<ProcessIndex>
        {
            public Segment seg;
            public int i;

            public bool Equals(ProcessIndex other)
            {
                return seg == other.seg && i == other.i;
            }

            public override bool Equals(object other)
            {
                if (other is ProcessIndex)
                    return Equals((ProcessIndex)other);
                return false;
            }

            public static bool operator ==(ProcessIndex a, ProcessIndex b)
            {
                return a.seg == b.seg && a.i == b.i;
            }

            public static bool operator !=(ProcessIndex a, ProcessIndex b)
            {
                return a.seg != b.seg || a.i != b.i;
            }

            public override int GetHashCode()
            {
                return (((int)seg - 4) * (int.MaxValue / 7)) + i;
            }
        }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(System.Collections.IEnumerator routine) { return null; }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(string methodName) { return null; }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine_Auto(System.Collections.IEnumerator routine) { return null; }

        [System.Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
        public new void StopCoroutine(string methodName) { }

        [System.Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
        public new void StopCoroutine(System.Collections.IEnumerator routine) { }

        [System.Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
        public new void StopCoroutine(Coroutine routine) { }

        [System.Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
        public new void StopAllCoroutines() { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void Destroy(Object obj) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void Destroy(Object obj, float f) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyObject(Object obj) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyObject(Object obj, float f) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyImmediate(Object obj) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyImmediate(Object obj, bool b) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void Instantiate(Object obj) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void Instantiate(Object original, Vector3 position, Quaternion rotation) { }

        [System.Obsolete("Use your own GameObject for this.", true)]
        public new static void Instantiate<T>(T original) where T : Object { }

        [System.Obsolete("Just.. no.", true)]
        public new static T FindObjectOfType<T>() where T : Object { return null; }

        [System.Obsolete("Just.. no.", true)]
        public new static Object FindObjectOfType(System.Type t) { return null; }

        [System.Obsolete("Just.. no.", true)]
        public new static T[] FindObjectsOfType<T>() where T : Object { return null; }

        [System.Obsolete("Just.. no.", true)]
        public new static Object[] FindObjectsOfType(System.Type t) { return null; }

        [System.Obsolete("Just.. no.", true)]
        public new static void print(object message) { }
    }

    /// <summary>
    /// The timing segment that a coroutine is running in or should be run in.
    /// </summary>
    public enum Segment
    {
        /// <summary>
        /// Sometimes returned as an error state
        /// </summary>
        Invalid = -1,
        /// <summary>
        /// This is the default timing segment
        /// </summary>
        Update,
        /// <summary>
        /// This is primarily used for physics calculations
        /// </summary>
        FixedUpdate,
        /// <summary>
        /// This is run immediately after update
        /// </summary>
        LateUpdate,
        /// <summary>
        /// This executes, by default, about as quickly as the eye can detect changes in a text field
        /// </summary>
        SlowUpdate,
        /// <summary>
        /// This is the same as update, but it ignores Unity's timescale
        /// </summary>
        RealtimeUpdate,
        /// <summary>
        /// This is a coroutine that runs in the unity editor while your app is not in play mode
        /// </summary>
        EditorUpdate,
        /// <summary>
        /// This executes in the unity editor about as quickly as the eye can detect changes in a text field
        /// </summary>
        EditorSlowUpdate,
        /// <summary>
        /// This segment executes as the very last action before the frame is done
        /// </summary>
        EndOfFrame,
        /// <summary>
        /// This segment can be configured to execute and/or define its notion of time in custom ways
        /// </summary>
        ManualTimeframe
    }

    /// <summary>
    /// How much debug info should be sent to the Unity profiler. NOTE: Setting this to anything above none shows up in the profiler as a 
    /// decrease in performance and a memory alloc. Those effects do not translate onto device.
    /// </summary>
    public enum DebugInfoType
    {
        /// <summary>
        /// None coroutines will be separated in the Unity profiler
        /// </summary>
        None,
        /// <summary>
        /// The Unity profiler will identify each coroutine individually
        /// </summary>
        SeperateCoroutines,
        /// <summary>
        /// Coroutines will be separated and any tags or layers will be identified
        /// </summary>
        SeperateTags
    }

    /// <summary>
    /// How the new coroutine should act if there are any existing coroutines running.
    /// </summary>
    public enum SingletonBehavior
    {
        /// <summary>
        /// Don't run this corutine if there are any matches
        /// </summary>
        Abort,
        /// <summary>
        /// Kill any matching coroutines when this one runs
        /// </summary>
        Overwrite,
        /// <summary>
        /// Run this coroutine once all matches finish running
        /// </summary>
        Wait,
        /// <summary>
        /// Don't run this coroutine if there are any matches, but unpause the matches if they're paused. 
        /// (Does not resume any coroutines in a WaitUntilDone state.)
        /// </summary>
        AbortAndUnpause
    }

    /// <summary>
    /// A handle for a MEC coroutine.
    /// </summary>
    public struct CoroutineHandle : System.IEquatable<CoroutineHandle>
    {
        private const byte ReservedSpace = 0x0F;
        private readonly static int[] NextIndex = { ReservedSpace + 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int _id;

        public byte Key { get { return (byte)(_id & ReservedSpace); } }

        public CoroutineHandle(byte ind)
        {
            if (ind > ReservedSpace)
                ind -= ReservedSpace;

            _id = NextIndex[ind] + ind;
            NextIndex[ind] += ReservedSpace + 1;
        }

        public CoroutineHandle(CoroutineHandle other)
        {
            _id = other._id;
        }

        public bool Equals(CoroutineHandle other)
        {
            return _id == other._id;
        }

        public override bool Equals(object other)
        {
            if (other is CoroutineHandle)
                return Equals((CoroutineHandle)other);
            return false;
        }

        public static bool operator ==(CoroutineHandle a, CoroutineHandle b)
        {
            return a._id == b._id;
        }

        public static bool operator !=(CoroutineHandle a, CoroutineHandle b)
        {
            return a._id != b._id;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            if (Timing.GetTag(this) == null)
            {
                if (Timing.GetLayer(this) == null)
                    return Timing.GetDebugName(this);
                else
                    return Timing.GetDebugName(this) + " Layer: " + Timing.GetLayer(this);
            }
            else
            {
                if (Timing.GetLayer(this) == null)
                    return Timing.GetDebugName(this) + " Tag: " + Timing.GetTag(this);
                else
                    return Timing.GetDebugName(this) + " Tag: " + Timing.GetTag(this) + " Layer: " + Timing.GetLayer(this);
            }
        }

        /// <summary>
        /// Get or set the corrosponding coroutine's tag. Null removes the tag or represents no tag assigned.
        /// </summary>
        public string Tag
        {
            get { return Timing.GetTag(this); }
            set { Timing.SetTag(this, value); }
        }

        /// <summary>
        /// Get or set the corrosponding coroutine's layer. Null removes the layer or represents no layer assigned.
        /// </summary>
        public int? Layer
        {
            get { return Timing.GetLayer(this); }
            set
            {
                if (value == null)
                    Timing.RemoveLayer(this);
                else
                    Timing.SetLayer(this, (int)value);
            }
        }

        /// <summary>
        /// Get or set the coorsponding coroutine's segment.
        /// </summary>
        public Segment Segment
        {
            get { return Timing.GetSegment(this); }
            set { Timing.SetSegment(this, value); }
        }

        /// <summary>
        /// Is true until the coroutine function ends or is killed. Paused or waiting coroutines count as running. 
        /// Setting this to false will kill the coroutine.
        /// </summary>
        public bool IsRunning
        {
            get { return Timing.IsRunning(this); }
            set { if (!value) Timing.KillCoroutines(this); }
        }

        /// <summary>
        /// Is true while the coroutine is paused. Setting this value will pause or resume the coroutine. 
        /// </summary>
        public bool IsAliveAndPaused
        {
            get { return Timing.IsAliveAndPaused(this); }
            set { if (value) Timing.PauseCoroutines(this); else Timing.ResumeCoroutines(this); }
        }

        /// <summary>
        /// Is true if this handle may have been a valid handle at some point. (i.e. is not an uninitialized handle, error handle, or a key to a coroutine lock)
        /// </summary>
        public bool IsValid
        {
            get { return Key != 0; }
        }

        /// <summary>
        /// This will execute the function you pass in once the coroutine this handle is pointing to is ended. This works whether this 
        /// coroutine gets to the end of its function, throws an exception, or is the target of a KillCoroutines command. NOTE: It is 
        /// generally a bad idea to use this function on any coroutine that you would use a CancelWith command on, because that will
        /// typically lead to exceptions when the gameObject is destroyed.
        /// </summary>
        /// <param name="action">The function that you want to execute after this coroutine ends.</param>
        /// <param name="segment">The timing segment that the OnDestroy action should be executed in.</param>
        /// <returns></returns>
        public CoroutineHandle OnDestroy(System.Action action, Segment segment = Segment.Update)
        {
            Timing inst = Timing.GetInstance(Key);
            if (action == null || inst == null)
                return new CoroutineHandle();

            return inst.RunCoroutineOnInstance(_OnDestroy(this, action), segment);
        }

        /// <summary>
        /// This will execute the coroutine you pass in once the coroutine this handle is pointing to is ended. This works whether this 
        /// coroutine gets to the end of its function, throws an exception, or is the target of a KillCoroutines command. NOTE: It is 
        /// generally a bad idea to use this function on any coroutine that you would use a CancelWith command on, because that will
        /// typically lead to exceptions when the gameObject is destroyed.
        /// </summary>
        /// <param name="action">The coroutine that you want to execute after this coroutine ends.</param>
        /// <param name="segment">The timing segment that the OnDestroy coroutine should be executed in.</param>
        /// <returns></returns>
        public CoroutineHandle OnDestroy(IEnumerator<float> action, Segment segment = Segment.Update)
        {
            Timing inst = Timing.GetInstance(Key);
            if (action == null || inst == null)
                return new CoroutineHandle();

            return inst.RunCoroutineOnInstance(_OnDestroy(this, action), segment);
        }

        private static IEnumerator<float> _OnDestroy(CoroutineHandle watched, System.Action action)
        {
            while (watched.IsRunning)
                yield return Timing.WaitForOneFrame;

            action();
        }

        private static IEnumerator<float> _OnDestroy(CoroutineHandle watched, IEnumerator<float> action)
        {
            while (watched.IsRunning)
                yield return Timing.WaitForOneFrame;

            while (action.MoveNext())
                yield return action.Current;
        }
    }

    public static class MECExtensionMethods1
    {
        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine)
        {
            return Timing.RunCoroutine(coroutine);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, GameObject gameObj)
        {
            return Timing.RunCoroutine(coroutine, gameObj);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, int layer)
        {
            return Timing.RunCoroutine(coroutine, layer);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, string tag)
        {
            return Timing.RunCoroutine(coroutine, tag);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, GameObject gameObj, string tag)
        {
            return Timing.RunCoroutine(coroutine, gameObj, tag);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, int layer, string tag)
        {
            return Timing.RunCoroutine(coroutine, layer, tag);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment)
        {
            return Timing.RunCoroutine(coroutine, segment);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment, GameObject gameObj)
        {
            return Timing.RunCoroutine(coroutine, segment, gameObj);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment, int layer)
        {
            return Timing.RunCoroutine(coroutine, segment, layer);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment, string tag)
        {
            return Timing.RunCoroutine(coroutine, segment, tag);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag)
        {
            return Timing.RunCoroutine(coroutine, segment, gameObj, tag);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(this IEnumerator<float> coroutine, Segment segment, int layer, string tag)
        {
            return Timing.RunCoroutine(coroutine, segment, layer, tag);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment, but not while the coroutine with the supplied handle is running.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="handle">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, CoroutineHandle handle, SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, handle, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, GameObject gameObj, SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? Timing.RunCoroutine(coroutine) :
                Timing.RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, int layer, SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, layer, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, string tag, SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, GameObject gameObj, string tag, SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? Timing.RunCoroutineSingleton(coroutine, tag, behaviorOnCollision)
                : Timing.RunCoroutineSingleton(coroutine, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment with the supplied graffitti unless there is already one or more coroutines running with both that 
        /// tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, int layer, string tag, SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, layer, tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine, but not while the coroutine with the supplied handle is running.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="handle">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, CoroutineHandle handle, Segment segment,
            SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, handle, segment, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, Segment segment, GameObject gameObj,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? Timing.RunCoroutine(coroutine, segment) :
                Timing.RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied layer unless there is already one or more coroutines running with that layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, Segment segment, int layer,
            SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, segment, layer, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied tag unless there is already one or more coroutines running with that tag.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, Segment segment, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, segment, tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied graffitti unless there is already one or more coroutines running with both that tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="gameObj">The new coroutine will be put on a layer corresponding to this gameObject.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, Segment segment, GameObject gameObj, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return gameObj == null ? Timing.RunCoroutineSingleton(coroutine, segment, tag, behaviorOnCollision)
                : Timing.RunCoroutineSingleton(coroutine, segment, gameObj.GetInstanceID(), tag, behaviorOnCollision);
        }

        /// <summary>
        /// Run a new coroutine with the supplied graffitti unless there is already one or more coroutines running with both that tag and layer.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="segment">The segment that the coroutine should run in.</param>
        /// <param name="layer">A layer to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="tag">A tag to attach to the coroutine, and to check for existing instances.</param>
        /// <param name="behaviorOnCollision">Should this coroutine fail to start, overwrite, or wait for any coroutines to finish if any matches are 
        /// currently running.</param>
        /// <returns>The newly created or existing handle.</returns>
        public static CoroutineHandle RunCoroutineSingleton(this IEnumerator<float> coroutine, Segment segment, int layer, string tag,
            SingletonBehavior behaviorOnCollision)
        {
            return Timing.RunCoroutineSingleton(coroutine, segment, layer, tag, behaviorOnCollision);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine)
        {
            return Timing.WaitUntilDone(newCoroutine);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, string tag)
        {
            return Timing.WaitUntilDone(newCoroutine, tag);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, int layer)
        {
            return Timing.WaitUntilDone(newCoroutine, layer);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, int layer, string tag)
        {
            return Timing.WaitUntilDone(newCoroutine, layer, tag);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, Segment segment)
        {
            return Timing.WaitUntilDone(newCoroutine, segment);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, Segment segment, string tag)
        {
            return Timing.WaitUntilDone(newCoroutine, segment, tag);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, Segment segment, int layer)
        {
            return Timing.WaitUntilDone(newCoroutine, segment, layer);
        }

        /// <summary>
        /// Use the command "yield return newCoroutine().WaitUntilDone();" to start a new coroutine and pause the
        /// current one until it finishes.
        /// </summary>
        /// <param name="newCoroutine">The coroutine to pause for.</param>
        /// <param name="segment">The segment that the new coroutine should run in.</param>
        /// <param name="layer">An optional layer to attach to the coroutine which can later be used to identify this coroutine.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used to identify this coroutine.</param>
        public static float WaitUntilDone(this IEnumerator<float> newCoroutine, Segment segment, int layer, string tag)
        {
            return Timing.WaitUntilDone(newCoroutine, segment, layer, tag);
        }
    }
}

public static class MECExtensionMethods2
{
    /// <summary>
    /// Adds a delay to the beginning of this coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="timeToDelay">The number of seconds to delay this coroutine.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Delay(this IEnumerator<float> coroutine, float timeToDelay)
    {
        yield return Pancake.Timing.WaitForSeconds(timeToDelay);

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Adds a delay to the beginning of this coroutine until a function returns true.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="condition">The coroutine will be paused until this function returns true.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Delay(this IEnumerator<float> coroutine, System.Func<bool> condition)
    {
        while (!condition())
            yield return 0f;

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Adds a delay to the beginning of this coroutine until a function returns true.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="data">A variable that will be passed into the condition function each time it is tested.</param>
    /// <param name="condition">The coroutine will be paused until this function returns true.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Delay<T>(this IEnumerator<float> coroutine, T data, System.Func<T, bool> condition)
    {
        while (!condition(data))
            yield return 0f;

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Adds a delay to the beginning of this coroutine in frames.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="framesToDelay">The number of frames to delay this coroutine.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> DelayFrames(this IEnumerator<float> coroutine, int framesToDelay)
    {
        while (framesToDelay-- > 0)
            yield return 0f;

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Cancels this coroutine when the supplied game object is destroyed or made inactive.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject">The GameObject to test.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject)
    {
        while (Pancake.Timing.MainThread != System.Threading.Thread.CurrentThread ||
                (gameObject && gameObject.activeInHierarchy && coroutine.MoveNext()))
            yield return coroutine.Current;
    }

    /// <summary>
    /// Cancels this coroutine when either of the supplied game objects are destroyed or made inactive.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject1">The first GameObject to test.</param>
    /// <param name="gameObject2">The second GameObject to test</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
    {
        while (Pancake.Timing.MainThread != System.Threading.Thread.CurrentThread || (gameObject1 && gameObject1.activeInHierarchy &&
                gameObject2 && gameObject2.activeInHierarchy && coroutine.MoveNext()))
            yield return coroutine.Current;
    }

    /// <summary>
    /// Cancels this coroutine when the supplied monobehavior is removed from its game object, or the game object is made inactive or destroyed.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject">The GameObject to test.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> CancelWith<T>(this IEnumerator<float> coroutine, T script) where T : MonoBehaviour
    {
        GameObject myGO = script.gameObject;

        while (Pancake.Timing.MainThread != System.Threading.Thread.CurrentThread ||
                (myGO && myGO.activeInHierarchy && script != null && coroutine.MoveNext()))
            yield return coroutine.Current;
    }

    /// <summary>
    /// Cancels this coroutine when the supplied function returns false.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="condition">The test function. True for continue, false to stop.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, System.Func<bool> condition)
    {
        if (condition == null) yield break;

        while (Pancake.Timing.MainThread != System.Threading.Thread.CurrentThread || (condition() && coroutine.MoveNext()))
            yield return coroutine.Current;
    }

    /// <summary>
    /// Cancels this coroutine when the supplied game object is destroyed, but only pauses it while it's inactive.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject">The GameObject to test.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, GameObject gameObject)
    {
        while (Pancake.Timing.MainThread == System.Threading.Thread.CurrentThread && gameObject)
        {
            if (gameObject.activeInHierarchy)
            {
                if (coroutine.MoveNext())
                    yield return coroutine.Current;
                else
                    yield break;
            }
            else
            {
                yield return Pancake.Timing.WaitForOneFrame;
            }
        }
    }

    /// <summary>
    /// Cancels this coroutine when either of the supplied game objects are destroyed, but only pauses them while they're inactive.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject1">The first GameObject to test.</param>
    /// <param name="gameObject2">The second GameObject to test</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
    {
        while (Pancake.Timing.MainThread == System.Threading.Thread.CurrentThread && gameObject1 && gameObject2)
        {
            if (gameObject1.activeInHierarchy && gameObject2.activeInHierarchy)
            {
                if (coroutine.MoveNext())
                    yield return coroutine.Current;
                else
                    yield break;
            }
            else
            {
                yield return Pancake.Timing.WaitForOneFrame;
            }
        }
    }

    /// <summary>
    /// Cancels this coroutine when the supplied monobehavior is removed from its game object, or the game object is destroyed. Pauses the coroutine 
    /// if the game object or script is disabled.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="gameObject">The GameObject to test.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> PauseWith<T>(this IEnumerator<float> coroutine, T script) where T : MonoBehaviour
    {
        GameObject myGO = script.gameObject;

        while (Pancake.Timing.MainThread == System.Threading.Thread.CurrentThread && myGO && myGO.GetComponent<T>() != null)
        {
            if (myGO.activeInHierarchy && script.enabled)
            {
                if (coroutine.MoveNext())
                    yield return coroutine.Current;
                else
                    yield break;
            }
            else
            {
                yield return Pancake.Timing.WaitForOneFrame;
            }
        }
    }

    /// <summary>
    /// Pauses this coroutine whenever the supplied function returns false.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="condition">The test function. True for continue, false to stop.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, System.Func<bool> condition)
    {
        if (condition == null) yield break;

        while (Pancake.Timing.MainThread != System.Threading.Thread.CurrentThread || (condition() && coroutine.MoveNext()))
            yield return coroutine.Current;
    }

    /// <summary>
    /// Watches the supplied handle and ends this coroutine when the other coroutine ends.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="otherCoroutine">A handle to the coroutine that should be watched.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> KillWith(this IEnumerator<float> coroutine, Pancake.CoroutineHandle otherCoroutine)
    {
        while (otherCoroutine.IsRunning && coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Runs the supplied coroutine immediately after this one.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="nextCoroutine">The coroutine to run next.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Append(this IEnumerator<float> coroutine, IEnumerator<float> nextCoroutine)
    {
        while (coroutine.MoveNext())
            yield return coroutine.Current;

        if (nextCoroutine == null) yield break;

        while (nextCoroutine.MoveNext())
            yield return nextCoroutine.Current;
    }

    /// <summary>
    /// Runs the supplied function immediately after this coroutine finishes.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="onDone">The action to run after this coroutine finishes.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Append(this IEnumerator<float> coroutine, System.Action onDone)
    {
        while (coroutine.MoveNext())
            yield return coroutine.Current;

        if (onDone != null)
            onDone();
    }

    /// <summary>
    /// Runs the supplied coroutine immediately before this one.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="lastCoroutine">The coroutine to run first.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Prepend(this IEnumerator<float> coroutine, IEnumerator<float> lastCoroutine)
    {
        if (lastCoroutine != null)
            while (lastCoroutine.MoveNext())
                yield return lastCoroutine.Current;

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Runs the supplied function immediately before this coroutine starts.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="onStart">The action to run before this coroutine starts.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Prepend(this IEnumerator<float> coroutine, System.Action onStart)
    {
        if (onStart != null)
            onStart();

        while (coroutine.MoveNext())
            yield return coroutine.Current;
    }

    /// <summary>
    /// Combines the this coroutine with another and runs them in a combined handle.
    /// </summary>
    /// <param name="coroutineA">The coroutine handle to act upon.</param>
    /// <param name="coroutineB">The coroutine handle to combine.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Superimpose(this IEnumerator<float> coroutineA, IEnumerator<float> coroutineB)
    {
        return Superimpose(coroutineA, coroutineB, Pancake.Timing.Instance);
    }

    /// <summary>
    /// Combines the this coroutine with another and runs them in a combined handle.
    /// </summary>
    /// <param name="coroutineA">The coroutine handle to act upon.</param>
    /// <param name="coroutineB">The coroutine handle to combine.</param>
    /// <param name="instance">The timing instance that this will be run in, if not the default instance.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Superimpose(this IEnumerator<float> coroutineA, IEnumerator<float> coroutineB, Pancake.Timing instance)
    {
        while (coroutineA != null || coroutineB != null)
        {
            if (coroutineA != null && !(instance.localTime < coroutineA.Current) && !coroutineA.MoveNext())
                coroutineA = null;

            if (coroutineB != null && !(instance.localTime < coroutineB.Current) && !coroutineB.MoveNext())
                coroutineB = null;

            if ((coroutineA != null && float.IsNaN(coroutineA.Current)) || (coroutineB != null && float.IsNaN(coroutineB.Current)))
                yield return float.NaN;
            else if (coroutineA != null && coroutineB != null)
                yield return coroutineA.Current < coroutineB.Current ? coroutineA.Current : coroutineB.Current;
            else if (coroutineA == null && coroutineB != null)
                yield return coroutineB.Current;
            else if (coroutineA != null)
                yield return coroutineA.Current;
        }
    }

    /// <summary>
    /// Uses the passed in function to change the return values of this coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="newReturn">A function that takes the current return value and returns the new return.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> Hijack(this IEnumerator<float> coroutine, System.Func<float, float> newReturn)
    {
        if (newReturn == null) yield break;

        while (coroutine.MoveNext())
            yield return newReturn(coroutine.Current);
    }

    /// <summary>
    /// This will send any exceptions thrown in this coroutine to the exception handler you define. If you pass in null then your exceptions 
    /// will go unreported. NOTE: Any exceptions thrown will still terminate that coroutine function. The only way to avoid termination is
    /// to catch the exception inside your function (avoiding any yield return statements).
    /// </summary>
    /// <param name="coroutine">The coroutine handle to act upon.</param>
    /// <param name="exceptionHandler">The function to be called when an exception occurs.</param>
    /// <returns>The modified coroutine handle.</returns>
    public static IEnumerator<float> RerouteExceptions(this IEnumerator<float> coroutine, System.Action<System.Exception> exceptionHandler)
    {
        while(true)
        {
            try
            {
                if (!coroutine.MoveNext())
                    yield break;
            }
            catch (System.Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler(ex);
                yield break;
            }

            yield return coroutine.Current;
        }
    }
}
