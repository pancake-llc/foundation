using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("icon_default")]
    public class UnityServiceInitialization : MonoBehaviour
    {
        [SerializeField] private Environment environment = Environment.Production;

        public void Awake() { Init(); }

        private async void Init()
        {
            Static.IsUnitySeriveReady = false;
            var options = new InitializationOptions();
            options.SetEnvironmentName(environment.ToString().ToLower());
            await UnityServices.InitializeAsync(options);
            Static.IsUnitySeriveReady = true;
        }

        private enum Environment
        {
            Production,
            Development,
        }
    }
}