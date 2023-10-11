using Pancake.Apex;
using UnityEngine;

namespace Pancake
{
    [HideMonoScript]
    [EditorIcon("csharp")]
    public class RuntimeInitializer : GameComponent
    {
        [SerializeField, Array] private Initialize[] initializes;

        private void Start()
        {
            foreach (var i in initializes) i.Init();
        }
    }
}