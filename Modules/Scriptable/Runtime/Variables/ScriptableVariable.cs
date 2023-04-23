using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [Serializable]
    public class ScriptableVariable<T> : ScriptableVariableBase, ISave, IReset, IDrawObjectsInInspector
    {
        [Tooltip("The value of the variable, this is changed at runtime.")] [SerializeField]
        protected T value;

        [Tooltip("The initial value of this variable. When reset is called, it is set to this value")] [SerializeField]
        private T initialValue;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        private bool debugLogEnabled;

        [Tooltip("If true, saves the value via Pancake.Data API and loads it onEnable.")] [SerializeField]
        private bool saved;

        [Tooltip("Reset to initial value. Scene Loaded : when the scene is loaded. Application Start : Once, when the application starts.")] [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

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

        public T InitialValue
        {
            get => initialValue;
            set
            {
                initialValue = value;
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

            if (debugLogEnabled) Debug.Log(GetColorizedString());

            if (saved) Save();

            PreviousValue = value;
#if UNITY_EDITOR
            SetDirtyInPlayMode();
#endif
        }

        private void SetDirtyInPlayMode()
        {
            //When the SV is changed by code, make sure it is marked dirty to be saved and picked up by Version Control.
            if (Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
        }

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;

            ResetToInitialValue();
        }


        private void OnDisable()
        {
            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) ResetToInitialValue();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            //In non fast play mode, this get called before OnEnable(). Therefore a saved variable can get saved before loading. 
            //This check prevents the latter.
            if (Equals(value, PreviousValue)) return;
            ValueChanged();
        }
#endif

        public virtual void Save()
        {
            if (debugLogEnabled) Debug.Log(GetColorizedString() + " <color=#f75369>[Saved]</color>");
        }

        public virtual void Load()
        {
            PreviousValue = value;
            if (debugLogEnabled) Debug.Log(GetColorizedString() + " <color=#f75369>[Loaded].</color>");
        }

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

        /// <summary> Reset to initial value or loads and clears the list</summary>
        public void ResetToInitialValue()
        {
            _listenersObjects.Clear();

            if (saved) Load();
            else SetToInitialValue();
        }

        public void SetToInitialValue()
        {
            Value = initialValue;
            PreviousValue = initialValue;
        }

        public List<Object> GetAllObjects() { return _listenersObjects; }

        public override string ToString()
        {
            var builder = new StringBuilder(name);
            builder.Append(" : ");
            builder.Append(value);
            return builder.ToString();
        }

        private string GetColorizedString() { return $"<color=#f75369>[Variable]</color> {ToString()}"; }

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