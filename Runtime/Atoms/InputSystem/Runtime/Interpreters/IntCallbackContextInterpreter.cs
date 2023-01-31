#if PANCAKE_ATOM
#if PANCAKE_INPUTSYSTEM
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace UnityAtoms.InputSystem
{
    [CreateAssetMenu(menuName = "Unity Atoms/Input System/Interpreters/Int")]
    public sealed class IntCallbackContextInterpreter : CallbackContextInterpreter<int, IntPair, IntConstant, IntVariable, IntEvent, IntPairEvent, IntIntFunction, IntVariableInstancer>
    {
    }
}

#endif
#endif