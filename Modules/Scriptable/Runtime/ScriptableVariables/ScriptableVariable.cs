using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public abstract class ScriptableVariable<T> : ScriptableVariableBase, ISave, IReset, IDrawObjectsInInspector
    {
        [Tooltip("The value of the variable. This will be reset on play mode exit to the value it had before entering play mode.")] [SerializeField]
        protected T value;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        private bool debugLogEnabled;

        [Tooltip("If true, saves the value to Player Prefs and loads it onEnable.")] [SerializeField]
        private bool saved;

        [Tooltip("The default value of this variable. When loading from Data the first time, it will be set to this value.")] [SerializeField] [ShowIf(nameof(saved))]
        private T defaultValue;

        [Tooltip("Reset to initial value." + " Scene Loaded : when the scene is loaded." + " Application Start : Once, when the application starts.")] [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

        private readonly List<Object> _listenersObjects = new List<Object>();

        /// <summary> Event raised when the variable value changes. </summary>
        private Action<T> _onValueChanged;

        /// <summary> This caches the value when play mode starts. </summary>
        private T _initialValue;

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

        public T PreviousValue { get; private set; }

        public T DefaultValue { get => defaultValue; private set => defaultValue = value; }

        public override Type GetGenericType => typeof(T);

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
                this.value = value;
                ValueChanged();
            }
        }

        private void ValueChanged()
        {
            _onValueChanged?.Invoke(value);

            if (debugLogEnabled)
            {
                string log = GetColorizedString();
                log += saved ? " <color=#f75369>[Saved]</color>" : "";
                Debug.Log(log);
            }

            if (saved) Save();

            PreviousValue = value;
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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (resetOn != ResetType.SceneLoaded) return;

            if (mode == LoadSceneMode.Single)
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
            _initialValue = default;
            PreviousValue = default;
            saved = false;
            resetOn = ResetType.SceneLoaded;
            debugLogEnabled = false;
        }

        public void Init()
        {
            _initialValue = value;
            PreviousValue = value;
            if (saved) Load();
            _listenersObjects.Clear();
        }

        /// <summary> Reset to initial value</summary>
        public void ResetToInitialValue()
        {
            Value = _initialValue;
            PreviousValue = _initialValue;
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
    }

    public enum CustomVariableType
    {
        None,
        Bool,
        Int,
        Float,
        String
    }
}