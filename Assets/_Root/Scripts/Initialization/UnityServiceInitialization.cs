using Pancake.Scriptable;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class UnityServiceInitialization : Initialize
    {
        [SerializeField] private BoolVariable isUnityServiceInitialized;
        [SerializeField] private Environment environment = Environment.Production;

        public override async void Init()
        {
            isUnityServiceInitialized.Value = false;
            var options = new InitializationOptions();
            options.SetEnvironmentName(environment.ToString().ToLower());
            await UnityServices.InitializeAsync(options);
            isUnityServiceInitialized.Value = true;
        }

        private enum Environment
        {
            Production,
            Development,
        }
    }
}