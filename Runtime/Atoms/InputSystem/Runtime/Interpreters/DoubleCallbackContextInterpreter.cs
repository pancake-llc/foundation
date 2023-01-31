#if PANCAKE_ATOM
#if PANCAKE_INPUTSYSTEM
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace UnityAtoms.InputSystem
{
    [CreateAssetMenu(menuName = "Unity Atoms/Input System/Interpreters/Double")]
    public sealed class DoubleCallbackContextInterpreter : CallbackContextInterpreter<double, DoublePair, DoubleConstant, DoubleVariable, DoubleEvent, DoublePairEvent, DoubleDoubleFunction, DoubleVariableInstancer>
    {
    }
}

#endif
#endif