using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_bind")]
    public class BindComparisonToUnityEvent : MonoBehaviour
    {
        public CustomVariableType type = CustomVariableType.None;

        [SerializeField] private BoolVariable boolVariable;
        [SerializeField] private BoolReference boolComparer;

        [SerializeField] private IntVariable intVariable;
        [SerializeField] private IntReference intComparer;

        [SerializeField] private FloatVariable floatVariable;
        [SerializeField] private FloatReference floatComparer;

        [SerializeField] private StringVariable stringVariable;
        [SerializeField] private StringReference stringComparer;

        public Comparator comparison = Comparator.Equal;

        [SerializeField] private UnityEvent unityEvent;

        private void Awake() { Subscribe(); }

        private void Start() { Evaluate(); }

        private void Evaluate()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    Evaluate(boolVariable.Value);
                    break;
                case CustomVariableType.Int:
                    Evaluate(intVariable.Value);
                    break;
                case CustomVariableType.Float:
                    Evaluate(floatVariable.Value);
                    break;
                case CustomVariableType.String:
                    Evaluate(stringVariable.Value);
                    break;
            }
        }

        private void Evaluate(bool value)
        {
            if (value == boolComparer) unityEvent.Invoke();
        }

        private void Evaluate(int value)
        {
            switch (comparison)
            {
                case Comparator.Equal:
                    if (value == intComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.Smaller:
                    if (value < intComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.Bigger:
                    if (value > intComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.BiggerOrEqual:
                    if (value >= intComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.SmallerOrEqual:
                    if (value <= intComparer.Value) unityEvent.Invoke();
                    break;
            }
        }

        private void Evaluate(float value)
        {
            switch (comparison)
            {
                case Comparator.Equal:
                    if (Mathf.Approximately(value, floatComparer.Value)) unityEvent.Invoke();
                    break;
                case Comparator.Smaller:
                    if (value < floatComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.Bigger:
                    if (value > floatComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.BiggerOrEqual:
                    if (value >= floatComparer.Value) unityEvent.Invoke();
                    break;
                case Comparator.SmallerOrEqual:
                    if (value <= floatComparer.Value) unityEvent.Invoke();
                    break;
            }
        }

        private void Evaluate(string value)
        {
            if (value.Equals(stringComparer.Value)) unityEvent.Invoke();
        }

        private void Subscribe()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    boolVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.Int:
                    intVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.Float:
                    floatVariable.OnValueChanged += Evaluate;
                    break;
                case CustomVariableType.String:
                    stringVariable.OnValueChanged += Evaluate;
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    boolVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.Int:
                    intVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.Float:
                    floatVariable.OnValueChanged -= Evaluate;
                    break;
                case CustomVariableType.String:
                    stringVariable.OnValueChanged -= Evaluate;
                    break;
            }
        }

        public enum Comparator
        {
            Equal,
            Smaller,
            Bigger,
            BiggerOrEqual,
            SmallerOrEqual
        }
    }
}