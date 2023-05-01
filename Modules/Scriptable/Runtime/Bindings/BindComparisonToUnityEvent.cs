using UnityEngine.Events;
using UnityEngine;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindComparisonToUnityEvent")]
    public class BindComparisonToUnityEvent : MonoBehaviour
    {
        public CustomVariableType Type = CustomVariableType.None;

        [SerializeField] private BoolVariable _boolVariable = null;
        [SerializeField] private BoolReference _boolComparer = null;

        [SerializeField] private IntVariable _intVariable = null;
        [SerializeField] private IntReference _intComparer = null;
  
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private FloatReference _floatComparer = null;

        [SerializeField] private StringVariable _stringVariable = null;
        [SerializeField] private StringReference _stringComparer = null;
  
        public Comparator Comparison = Comparator.EQUAL;

        [SerializeField] private UnityEvent _unityEvent = null;

        private void Awake()
        {
            Subscribe();
        }

        private void Start()
        {
            Evaluate();
        }

        private void Evaluate()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    Evaluate(_boolVariable.Value);
                    break;
                case CustomVariableType.Int:
                    Evaluate(_intVariable.Value);
                    break;
                case CustomVariableType.Float:
                    Evaluate(_floatVariable.Value);
                    break;
                case CustomVariableType.String:
                    Evaluate(_stringVariable.Value);
                    break;
            }
        }

        private void Evaluate(bool value)
        {
            if (value == _boolComparer)
                _unityEvent.Invoke();
        }

        private void Evaluate(int value)
        {
            switch (Comparison)
            {
                case Comparator.EQUAL:
                    if (value == _intComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.SMALLER:
                    if (value < _intComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.BIGGER:
                    if (value > _intComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.BIGGER_OR_EQUAL:
                    if (value >= _intComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.SMALLER_OR_EQUAL:
                    if (value <= _intComparer.Value)
                        _unityEvent.Invoke();
                    break;
            }
        }

        private void Evaluate(float value)
        {
            switch (Comparison)
            {
                case Comparator.EQUAL:
                    if (Mathf.Approximately(value, _floatComparer.Value))
                        _unityEvent.Invoke();
                    break;
                case Comparator.SMALLER:
                    if (value < _floatComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.BIGGER:
                    if (value > _floatComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.BIGGER_OR_EQUAL:
                    if (value >= _floatComparer.Value)
                        _unityEvent.Invoke();
                    break;
                case Comparator.SMALLER_OR_EQUAL:
                    if (value <= _floatComparer.Value)
                        _unityEvent.Invoke();
                    break;
            }
        }

        private void Evaluate(string value)
        {
            if (value.Equals(_stringComparer.Value))
                _unityEvent.Invoke();
        }

        private void Subscribe()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    _boolVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.Int:
                    _intVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.Float:
                    _floatVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.String:
                    _stringVariable.OnValueChanged += Evaluate;
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    _boolVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.Int:
                    _intVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.Float:
                    _floatVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.String:
                    _stringVariable.OnValueChanged -= Evaluate;
                    break;
            }
        }

        public enum Comparator
        {
            EQUAL,
            SMALLER,
            BIGGER,
            BIGGER_OR_EQUAL,
            SMALLER_OR_EQUAL
        }
    }
}