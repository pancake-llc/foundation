/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@martinTayx)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            15-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using System;
using UnityEngine;
using Tayx.Graphy.Fps;
using Tayx.Graphy.Ram;
using Tayx.Graphy.Utils;
using Tayx.Graphy.Utils.NumString;

#if GRAPHY_NEW_INPUT
using UnityEngine.InputSystem;
#endif

namespace Tayx.Graphy
{
    /// <summary>
    /// Main class to access the Graphy API.
    /// </summary>
    public class GraphyManager : MonoBehaviour
    {
        private static GraphyManager _instance;
        private static object _lock = new object();

        public static GraphyManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Debug.Log("[Singleton] An instance of " + typeof(GraphyManager) + " is trying to be accessed, but it wasn't initialized first. " +
                                  "Make sure to add an instance of " + typeof(GraphyManager) + " in the scene before " + " trying to access it.");
                    }

                    return _instance;
                }
            }
        }

        private void Awake()
        {
            if (_instance != null) Destroy(gameObject);
            else _instance = GetComponent<GraphyManager>();
        }

        protected GraphyManager() { }

        #region Enums -> Public

        public enum Mode
        {
            FULL = 0,
            LIGHT = 1
        }

        public enum ModuleType
        {
            FPS = 0,
            RAM = 1,
        }

        public enum ModuleState
        {
            FULL = 0,
            TEXT = 1,
            BASIC = 2,
            BACKGROUND = 3,
            OFF = 4
        }

        public enum ModulePosition
        {
            TOP_RIGHT = 0,
            TOP_LEFT = 1,
            BOTTOM_RIGHT = 2,
            BOTTOM_LEFT = 3,
            FREE = 4
        }

        public enum ModulePreset
        {
            FPS_BASIC = 0,
            FPS_TEXT = 1,
            FPS_FULL = 2,

            FPS_TEXT_RAM_TEXT = 3,
            FPS_FULL_RAM_TEXT = 4,
            FPS_FULL_RAM_FULL = 5,

            FPS_TEXT_RAM_TEXT_AUDIO_TEXT = 6,
            FPS_FULL_RAM_TEXT_AUDIO_TEXT = 7,
            FPS_FULL_RAM_FULL_AUDIO_TEXT = 8,
            FPS_FULL_RAM_FULL_AUDIO_FULL = 9,

            FPS_FULL_RAM_FULL_AUDIO_FULL_ADVANCED_FULL = 10,
            FPS_BASIC_ADVANCED_FULL = 11
        }

        #endregion

        #region Variables -> Serialized Private

        [SerializeField] private Mode m_graphyMode = Mode.FULL;

        [SerializeField] private bool m_enableOnStartup = true;

        [SerializeField] private bool m_keepAlive = true;

        [SerializeField] private bool m_background = true;
        [SerializeField] private Color m_backgroundColor = new Color(0, 0, 0, 0.3f);

        [SerializeField] private ModulePosition m_graphModulePosition = ModulePosition.TOP_RIGHT;
        [SerializeField] private Vector2 m_graphModuleOffset = new Vector2(0, 0);

        // Fps ---------------------------------------------------------------------------

        [SerializeField] private ModuleState m_fpsModuleState = ModuleState.FULL;

        [SerializeField] private Color m_goodFpsColor = new Color32(118, 212, 58, 255);
        [SerializeField] private int m_goodFpsThreshold = 60;

        [SerializeField] private Color m_cautionFpsColor = new Color32(243, 232, 0, 255);
        [SerializeField] private int m_cautionFpsThreshold = 30;

        [SerializeField] private Color m_criticalFpsColor = new Color32(220, 41, 30, 255);

        [Range(10, 300)] [SerializeField] private int m_fpsGraphResolution = 150;

        [Range(1, 200)] [SerializeField] private int m_fpsTextUpdateRate = 3; // 3 updates per sec.

        // Ram ---------------------------------------------------------------------------

        [SerializeField] private ModuleState m_ramModuleState = ModuleState.FULL;

        [SerializeField] private Color m_allocatedRamColor = new Color32(255, 190, 60, 255);
        [SerializeField] private Color m_reservedRamColor = new Color32(205, 84, 229, 255);
        [SerializeField] private Color m_monoRamColor = new Color(0.3f, 0.65f, 1f, 1);

        [Range(10, 300)] [SerializeField] private int m_ramGraphResolution = 150;
        [Range(1, 200)] [SerializeField] private int m_ramTextUpdateRate = 3; // 3 updates per sec.

        #endregion

        #region Variables -> Private

        private bool m_initialized = false;
        private bool m_active = true;
        private bool m_focused = true;

        private G_FpsManager m_fpsManager = null;
        private G_RamManager m_ramManager = null;

        private G_FpsMonitor m_fpsMonitor = null;
        private G_RamMonitor m_ramMonitor = null;

        private ModulePreset m_modulePresetState = ModulePreset.FPS_BASIC_ADVANCED_FULL;

        #endregion

        public Mode GraphyMode
        {
            get => m_graphyMode;
            set
            {
                m_graphyMode = value;
                UpdateAllParameters();
            }
        }

        public bool EnableOnStartup => m_enableOnStartup;

        public bool KeepAlive => m_keepAlive;

        public bool Background
        {
            get => m_background;
            set
            {
                m_background = value;
                UpdateAllParameters();
            }
        }

        public Color BackgroundColor
        {
            get => m_backgroundColor;
            set
            {
                m_backgroundColor = value;
                UpdateAllParameters();
            }
        }

        public ModulePosition GraphModulePosition
        {
            get => m_graphModulePosition;
            set
            {
                m_graphModulePosition = value;
                m_fpsManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);
                m_ramManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);
            }
        }

        // Fps ---------------------------------------------------------------------------

        // Setters & Getters

        public ModuleState FpsModuleState
        {
            get => m_fpsModuleState;
            set
            {
                m_fpsModuleState = value;
                m_fpsManager.SetState(m_fpsModuleState);
            }
        }

        public Color GoodFPSColor
        {
            get => m_goodFpsColor;
            set
            {
                m_goodFpsColor = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public Color CautionFPSColor
        {
            get => m_cautionFpsColor;
            set
            {
                m_cautionFpsColor = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public Color CriticalFPSColor
        {
            get => m_criticalFpsColor;
            set
            {
                m_criticalFpsColor = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public int GoodFPSThreshold
        {
            get => m_goodFpsThreshold;
            set
            {
                m_goodFpsThreshold = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public int CautionFPSThreshold
        {
            get => m_cautionFpsThreshold;
            set
            {
                m_cautionFpsThreshold = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public int FpsGraphResolution
        {
            get => m_fpsGraphResolution;
            set
            {
                m_fpsGraphResolution = value;
                m_fpsManager.UpdateParameters();
            }
        }

        public int FpsTextUpdateRate
        {
            get => m_fpsTextUpdateRate;
            set
            {
                m_fpsTextUpdateRate = value;
                m_fpsManager.UpdateParameters();
            }
        }

        // Getters

        public float CurrentFPS => m_fpsMonitor.CurrentFPS;
        public float AverageFPS => m_fpsMonitor.AverageFPS;
        public float OnePercentFPS => m_fpsMonitor.OnePercentFPS;
        public float Zero1PercentFps => m_fpsMonitor.Zero1PercentFps;

        // Ram ---------------------------------------------------------------------------

        // Setters & Getters

        public ModuleState RamModuleState
        {
            get => m_ramModuleState;
            set
            {
                m_ramModuleState = value;
                m_ramManager.SetState(m_ramModuleState);
            }
        }


        public Color AllocatedRamColor
        {
            get => m_allocatedRamColor;
            set
            {
                m_allocatedRamColor = value;
                m_ramManager.UpdateParameters();
            }
        }

        public Color ReservedRamColor
        {
            get => m_reservedRamColor;
            set
            {
                m_reservedRamColor = value;
                m_ramManager.UpdateParameters();
            }
        }

        public Color MonoRamColor
        {
            get => m_monoRamColor;
            set
            {
                m_monoRamColor = value;
                m_ramManager.UpdateParameters();
            }
        }

        public int RamGraphResolution
        {
            get => m_ramGraphResolution;
            set
            {
                m_ramGraphResolution = value;
                m_ramManager.UpdateParameters();
            }
        }

        public int RamTextUpdateRate
        {
            get => m_ramTextUpdateRate;
            set
            {
                m_ramTextUpdateRate = value;
                m_ramManager.UpdateParameters();
            }
        }

        // Getters

        public float AllocatedRam => m_ramMonitor.AllocatedRam;
        public float ReservedRam => m_ramMonitor.ReservedRam;
        public float MonoRam => m_ramMonitor.MonoRam;

        #region Methods -> Unity Callbacks

        private void Start() { Init(); }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;

            G_IntString.Dispose();
            G_FloatString.Dispose();
        }

        private void OnApplicationFocus(bool isFocused)
        {
            m_focused = isFocused;

            if (m_initialized && isFocused)
            {
                RefreshAllParameters();
            }
        }

        #endregion

        #region Methods -> Public

        public void SetModulePosition(ModuleType moduleType, ModulePosition modulePosition)
        {
            switch (moduleType)
            {
                case ModuleType.FPS:
                case ModuleType.RAM:

                    m_graphModulePosition = modulePosition;

                    m_ramManager.SetPosition(modulePosition, m_graphModuleOffset);
                    m_fpsManager.SetPosition(modulePosition, m_graphModuleOffset);
                    break;
            }
        }

        public void SetModuleMode(ModuleType moduleType, ModuleState moduleState)
        {
            switch (moduleType)
            {
                case ModuleType.FPS:
                    m_fpsManager.SetState(moduleState);
                    break;

                case ModuleType.RAM:
                    m_ramManager.SetState(moduleState);
                    break;
            }
        }

        public void ToggleModes()
        {
            if ((int) m_modulePresetState >= Enum.GetNames(typeof(ModulePreset)).Length - 1)
            {
                m_modulePresetState = 0;
            }
            else
            {
                m_modulePresetState++;
            }

            SetPreset(m_modulePresetState);
        }

        public void SetPreset(ModulePreset modulePreset)
        {
            m_modulePresetState = modulePreset;

            switch (m_modulePresetState)
            {
                case ModulePreset.FPS_BASIC:
                    m_fpsManager.SetState(ModuleState.BASIC);
                    m_ramManager.SetState(ModuleState.OFF);
                    break;

                case ModulePreset.FPS_TEXT:
                    m_fpsManager.SetState(ModuleState.TEXT);
                    m_ramManager.SetState(ModuleState.OFF);
                    break;

                case ModulePreset.FPS_FULL:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.OFF);
                    break;

                case ModulePreset.FPS_TEXT_RAM_TEXT:
                    m_fpsManager.SetState(ModuleState.TEXT);
                    m_ramManager.SetState(ModuleState.TEXT);
                    break;

                case ModulePreset.FPS_FULL_RAM_TEXT:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.TEXT);
                    break;

                case ModulePreset.FPS_FULL_RAM_FULL:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.FULL);
                    break;

                case ModulePreset.FPS_TEXT_RAM_TEXT_AUDIO_TEXT:
                    m_fpsManager.SetState(ModuleState.TEXT);
                    m_ramManager.SetState(ModuleState.TEXT);

                    break;

                case ModulePreset.FPS_FULL_RAM_TEXT_AUDIO_TEXT:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.TEXT);

                    break;

                case ModulePreset.FPS_FULL_RAM_FULL_AUDIO_TEXT:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.FULL);

                    break;

                case ModulePreset.FPS_FULL_RAM_FULL_AUDIO_FULL:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.FULL);

                    break;

                case ModulePreset.FPS_FULL_RAM_FULL_AUDIO_FULL_ADVANCED_FULL:
                    m_fpsManager.SetState(ModuleState.FULL);
                    m_ramManager.SetState(ModuleState.FULL);

                    break;

                case ModulePreset.FPS_BASIC_ADVANCED_FULL:
                    m_fpsManager.SetState(ModuleState.BASIC);
                    m_ramManager.SetState(ModuleState.OFF);

                    break;

                default:
                    Debug.LogWarning("[GraphyManager]::SetPreset - Tried to set a preset that is not supported.");
                    break;
            }
        }

        public void ToggleActive()
        {
            if (!m_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        public void Enable()
        {
            if (!m_active)
            {
                if (m_initialized)
                {
                    m_fpsManager.RestorePreviousState();
                    m_ramManager.RestorePreviousState();

                    m_active = true;
                }
                else
                {
                    Init();
                }
            }
        }

        public void Disable()
        {
            if (m_active)
            {
                m_fpsManager.SetState(ModuleState.OFF);
                m_ramManager.SetState(ModuleState.OFF);

                m_active = false;
            }
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            if (m_keepAlive)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }

            m_fpsMonitor = GetComponentInChildren<G_FpsMonitor>(true);
            m_ramMonitor = GetComponentInChildren<G_RamMonitor>(true);

            m_fpsManager = GetComponentInChildren<G_FpsManager>(true);
            m_ramManager = GetComponentInChildren<G_RamManager>(true);

            m_fpsManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);
            m_ramManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);

            m_fpsManager.SetState(m_fpsModuleState);
            m_ramManager.SetState(m_ramModuleState);

            if (!m_enableOnStartup)
            {
                ToggleActive();

                // We need to enable this on startup because we disable it in GraphyManagerEditor
                GetComponent<Canvas>().enabled = true;
            }

            m_initialized = true;
        }

        // AMW
        public void OnValidate()
        {
            if (m_initialized)
            {
                m_fpsManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);
                m_ramManager.SetPosition(m_graphModulePosition, m_graphModuleOffset);

                m_fpsManager.SetState(m_fpsModuleState);
                m_ramManager.SetState(m_ramModuleState);
            }
        }

        private void UpdateAllParameters()
        {
            m_fpsManager.UpdateParameters();
            m_ramManager.UpdateParameters();
        }

        private void RefreshAllParameters()
        {
            m_fpsManager.RefreshParameters();
            m_ramManager.RefreshParameters();
        }

        #endregion
    }
}