using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class UnityServiceInitialization : Initialize
    {
        [SerializeField] private Environment environment = Environment.Production;

        public override async void Init()
        {
            Pancake.Static.IsUnitySeriveReady = false;
            var options = new InitializationOptions();
            options.SetEnvironmentName(environment.ToString().ToLower());
            await UnityServices.InitializeAsync(options);
            Pancake.Static.IsUnitySeriveReady = true;
        }

        private enum Environment
        {
            Production,
            Development,
        }
    }
}