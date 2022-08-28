using UnityEngine;

namespace Pancake.Core
{
    /// <summary>
    /// A component you can select UpdateMode
    /// </summary>
    public abstract class ConfigurableUpdateComponent : ScriptableComponent
    {
        [SerializeField] private UpdateMode updateMode = UpdateMode.Update;


        private bool _registered = false;


        /// <summary>
        /// Update mode
        /// </summary>
        public UpdateMode UpdateMode
        {
            get { return updateMode; }
            set
            {
                if (updateMode != value)
                {
                    if (_registered)
                    {
                        RuntimeUtilities.RemoveUpdate(updateMode, OnUpdate);
                        RuntimeUtilities.AddUpdate(value, OnUpdate);

#if UNITY_EDITOR
                        _addedUpdateMode = value;
#endif
                    }

                    updateMode = value;
                }
            }
        }


        protected abstract void OnUpdate();


        protected virtual void OnEnable()
        {
            RuntimeUtilities.AddUpdate(updateMode, OnUpdate);
            _registered = true;

#if UNITY_EDITOR
            _addedUpdateMode = updateMode;
#endif
        }


        protected virtual void OnDisable()
        {
            RuntimeUtilities.RemoveUpdate(updateMode, OnUpdate);
            _registered = false;
        }


#if UNITY_EDITOR

        private UpdateMode _addedUpdateMode;


        protected virtual void OnValidate()
        {
            if (_registered && _addedUpdateMode != updateMode)
            {
                RuntimeUtilities.RemoveUpdate(_addedUpdateMode, OnUpdate);
                RuntimeUtilities.AddUpdate(updateMode, OnUpdate);
                _addedUpdateMode = updateMode;
            }
        }

#endif
    } // class ConfigurableUpdateComponent
} // namespace Pancake