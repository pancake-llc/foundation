
using UnityEngine;

namespace Pancake
{

    [EditorIcon("csharp")]
    public class RuntimeInitializer : GameComponent
    {
        [SerializeField] private Initialize[] initializes;

        private void Start()
        {
            foreach (var i in initializes) i.Init();
        }
    }
}