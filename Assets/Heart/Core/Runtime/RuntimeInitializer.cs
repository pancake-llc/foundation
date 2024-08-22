using UnityEngine;

namespace Pancake
{
    [EditorIcon("icon_default")]
    public class RuntimeInitializer : MonoBehaviour
    {
        [SerializeField] private BaseInitialization[] initializes;

        private void Start()
        {
            foreach (var i in initializes) i.Init();
        }
    }
}