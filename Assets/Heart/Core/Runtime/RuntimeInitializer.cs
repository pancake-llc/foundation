
using UnityEngine;

namespace Pancake
{

    [EditorIcon("icon_default")]
    public class RuntimeInitializer : GameComponent
    {
        [SerializeField] private Initialize[] initializes;

        private void Start()
        {
            foreach (var i in initializes) i.Init();
        }
    }
}