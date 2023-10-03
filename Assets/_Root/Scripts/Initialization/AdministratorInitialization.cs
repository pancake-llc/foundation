using Pancake.Apex;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [HideMonoScript]
    public class AdministratorInitialization : Initialize
    {
        [SerializeField] private GameObject prefab;

        public override void Init()
        {
            if (!HeartSettings.EnableAdministrator) return;
            
            // init menu debug
            var instance = Instantiate(prefab, transform, false);
        }
    }
}