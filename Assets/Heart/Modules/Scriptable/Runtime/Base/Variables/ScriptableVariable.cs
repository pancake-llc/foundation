using System;
using System.Collections.Generic;
using System.Text;
using Pancake.Apex;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_variable")]
    public abstract class ScriptableVariable<T> : ScriptableBase, ISave, IReset, IResetOn, IDrawObjectsInInspector, IGuid
    {
#if UNITY_EDITOR
        protected virtual bool IgnoreDraw => false;
#endif

        [SerializeField, DisableInPlayMode, HideIf("IgnoreDraw")] protected T initialValue;

        [Tooltip("The value of the variable. This will be reset on play mode exit to the value it had before entering play mode.")]
        [SerializeField, HideInEditorMode, IgnoreTypeMismatch]
        protected T value;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        private bool debugLogEnabled;

        [Tooltip("If true, saves the value to Player Prefs and loads it onEnable.")] [SerializeField, HideIf("IgnoreDraw")]
        private bool saved;

        [SerializeField, ShowIf(nameof(saved)), Label("GUID"), HorizontalGroup("guid"), Indent]
        private ECreationMode guidCreateMode;

        [SerializeField, ShowIf(nameof(saved)), DisableIf(nameof(guidCreateMode), ECreationMode.Auto), HorizontalGroup("guid"), HideLabel, Indent]
        private string guid;

        [Tooltip("Reset to initial value when :" + "\nScene Loaded : when the scene is loaded by LoadSceneMode.Single" +
                 "\nAdditive Scene Loaded : when the scene is loaded by LoadSceneMode.Additive" + "\nApplication Start : Once, when the application starts.")]
        [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

        private readonly List<Object> _listenersObjects = new List<Object>();

        /// <summary> Event raised when the variable value changes. </summary>
        private Action<T> _onValueChanged;

        /// <summary>
        /// Event raised when the variable value changes.
        /// </summary>
        public event Action<T> OnValueChanged
        {
            add
            {
                _onValueChanged += value;

                var listener = value.Target as Object;
                if (listener != null && !_listenersObjects.Contains(listener)) _listenersObjects.Add(listener);
            }
            remove
            {
                _onValueChanged -= value;

                var listener = value.Target as Object;
                if (_listenersObjects.Contains(listener)) _listenersObjects.Remove(listener);
            }
        }

        /// <summary>
        /// The previous value just after the value changed.
        /// </summary>
        public T PreviousValue { get; private set; }

        public string Guid { get => guid; set => guid = value; }
        public ECreationMode GuidCreateMode { get => guidCreateMode; set => guidCreateMode = value; }

        public override Type GetGenericType => typeof(T);
        public virtual T InitialValue { get => initialValue; internal set => initialValue = value; }

        /// <summary>
        /// Modify this to change the value of the variable.
        /// Triggers OnValueChanged event.
        /// </summary>
        public virtual T Value
        {
            get => value;
            set
            {
                if (Equals(this.value, value)) return;
                PreviousValue = this.value;
                this.value = value;
                ValueChanged();
            }
        }

        protected void ValueChanged()
        {
            _onValueChanged?.Invoke(value);

            if (debugLogEnabled)
            {
                string log = GetColorizedString();
                log += saved ? " <color=#f75369>[Saved]</color>" : "";
                Debug.Log(log);
            }

            if (saved) Save();
#if UNITY_EDITOR
            SetDirtyAndRepaint();
#endif
        }

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Init();
#endif
            if (resetOn is ResetType.SceneLoaded or ResetType.AdditiveSceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (resetOn is ResetType.SceneLoaded or ResetType.AdditiveSceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if ((resetOn == ResetType.SceneLoaded && mode == LoadSceneMode.Single) || (resetOn == ResetType.AdditiveSceneLoaded && mode == LoadSceneMode.Additive))
            {
                if (saved) Load();
                else ResetToInitialValue();
            }
        }

#if UNITY_EDITOR
        private void SetDirtyAndRepaint()
        {
            EditorUtility.SetDirty(this);
            repaintRequest?.Invoke();
        }

        public void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode) Init();
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (!saved) ResetToInitialValue();
            }
        }

        protected virtual void OnValidate()
        {
            //In non fast play mode, this get called before OnEnable(). Therefore a saved variable can get saved before loading. 
            //This check prevents the latter.
            if (Equals(value, PreviousValue)) return;
            ValueChanged();
        }
#endif
        /// <summary> Reset the SO to default.</summary>
        public override void Reset()
        {
            _listenersObjects.Clear();
            Value = default;
            InitialValue = default;
            PreviousValue = default;
            saved = false;
            resetOn = ResetType.SceneLoaded;
            debugLogEnabled = false;
        }

        private void Init()
        {
            value = InitialValue;
            PreviousValue = value;
            if (saved) Load();
            _listenersObjects.Clear();
        }

        /// <summary> Reset to initial value</summary>
        [Button]
        public void ResetToInitialValue()
        {
            Value = InitialValue;
            PreviousValue = InitialValue;
        }

        public virtual void Save()
        {
#if UNITY_EDITOR
            Data.SaveAll();
#endif
        }

        public virtual void Load()
        {
            PreviousValue = value;

            if (debugLogEnabled) Debug.Log(GetColorizedString() + " <color=#f75369>[Loaded].</color>");
        }

        bool ISave.Saved => saved;
        ResetType IResetOn.ResetOn => resetOn;

        public override string ToString()
        {
            var sb = new StringBuilder(name);
            sb.Append(" : ");
            sb.Append(value);
            return sb.ToString();
        }

        private string GetColorizedString() => $"<color=#f75369>[Variable]</color> {ToString()}";

        public List<Object> GetAllObjects() => _listenersObjects;

        public static implicit operator T(ScriptableVariable<T> variable) => variable.Value;

#if UNITY_EDITOR
        private bool IsInPlayMode => Application.isPlaying;
        [Button, ButtonHeight(20), Color(1f, 0.47f, 0.78f, 0.66f), ShowIf("IsInPlayMode")]
        private void ShowListener()
        {
            VariableListenerWindow.Show();
            VariableListenerWindow.objects = _listenersObjects;
        }
#endif
    }
}