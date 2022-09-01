using UnityEngine;

namespace Pancake.Debugging
{
    /// <summary>
    /// Class responsible for initializing the RuntimeDebugger class at runtime in builds and in the editor.
    /// 
    /// This class should not be compiled inside the Debug DLL because methods with the RuntimeInitializeOnLoadMethod
    /// won't get called for classes inside DLLs.
    /// </summary>
    internal class RuntimeInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeRuntimeDebugger()
        {
            Debug.Initialize();
            RuntimeDebugger.Initialize();
        }
    }
}