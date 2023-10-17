using Pancake.Scriptable;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class UnityServiceInitialization : Initialize
    {
        [SerializeField] private BoolVariable isUnityServiceInitialized;

        public override async void Init()
        {
            isUnityServiceInitialized.Value = false;
            var options = new InitializationOptions();
            options.SetEnvironmentName(Application.isMobilePlatform ? "production" : "development");
            await UnityServices.InitializeAsync(options);
            isUnityServiceInitialized.Value = true;
        }
    }
}