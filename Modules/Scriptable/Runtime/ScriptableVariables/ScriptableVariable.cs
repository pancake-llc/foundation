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
        [Tooltip("The value of the variable, this is changed at runtime.")] [SerializeField]
        protected T _value;

        [Tooltip("The initial value of this variable. When reset is called, it is set to this value")] [SerializeField]
        private T _initialValue;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        private bool _debugLogEnabled = false;

        [Tooltip("If true, saves the value to Player Prefs and loads it onEnable.")] [SerializeField]
        private bool _saved = false;

        [Tooltip("Reset to initial value." + " Scene Loaded : when the scene is loaded." + " Application Start : Once, when the application starts.")] [SerializeField]
        private ResetType _resetOn = ResetType.SceneLoaded;

        private readonly List<Object> _listenersObjects = new List<Object>();

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
                if (listener != null && !_listenersObjects.Contains(listener))
                    _listenersObjects.Add(listener);
            }
            remove
            {
                _onValueChanged -= value;

                var listener = value.Target as Object;
                if (_listenersObjects.Contains(listener))
                    _listenersObjects.Remove(listener);
            }
        }

        public T PreviousValue { get; private set; }

        public T InitialValue
        {
            get => _initialValue;
            set
            {
                _initialValue = value;
#if UNITY_EDITOR
                SetDirtyInPlayMode();
#endif
            }
        }

        /// <summary>
        /// Modify this to change the value of the variable.
        /// Triggers OnValueChanged event.
        /// </summary>
        public virtual T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                ValueChanged();
            }
        }

        private void ValueChanged()
        {
            _onValueChanged?.Invoke(_value);

            if (_debugLogEnabled)
                Debug.Log(GetColorizedString());

            if (_saved)
                Save();

            PreviousValue = _value;
#if UNITY_EDITOR
            SetDirtyInPlayMode();
#endif
        }

        private void SetDirtyInPlayMode()
        {
#if UNITY_EDITOR
            //When the SV is changed by code, make sure it is marked dirty to be saved and picked up by Version Control.
            if (Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            if (_resetOn == ResetType.SceneLoaded)
                SceneManager.sceneLoaded += OnSceneLoaded;

            ResetToInitialValue();
        }

        private void OnDisable()
        {
            if (_resetOn == ResetType.SceneLoaded)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                ResetToInitialValue();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            //In non fast play mode, this get called before OnEnable(). Therefore a saved variable can get saved before loading. 
            //This check prevents the latter.
            if (Equals(_value, PreviousValue))
                return;
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
            _saved = false;
            _resetOn = ResetType.SceneLoaded;
            _debugLogEnabled = false;
        }

        /// <summary> Reset to initial value or loads and clears the list</summary>
        public void ResetToInitialValue()
        {
            _listenersObjects.Clear();

            if (_saved)
                Load();
            else
                SetToInitialValue();
        }

        public virtual void Save()
        {
            if (_debugLogEnabled)
                Debug.Log(GetColorizedString() + " <color=#f75369>[Saved]</color>");
        }

        public virtual void Load()
        {
            PreviousValue = _value;

            if (_debugLogEnabled)
                Debug.Log(GetColorizedString() + " <color=#f75369>[Loaded].</color>");
        }

        public void SetToInitialValue()
        {
            Value = _initialValue;
            PreviousValue = _initialValue;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(name);
            sb.Append(" : ");
            sb.Append(_value);
            return sb.ToString();
        }

        private string GetColorizedString() { return $"<color=#f75369>[Variable]</color> {ToString()}"; }

        public List<Object> GetAllObjects() { return _listenersObjects; }

        public static implicit operator T(ScriptableVariable<T> variable) { return variable.Value; }
    }

    public enum ResetType
    {
        SceneLoaded,
        ApplicationStarts,
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