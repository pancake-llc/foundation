using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_entry")]
    public class EntryPointScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) { }
    }
}