using System;
using Pancake.Monetization;
using UnityEngine;

namespace Pancake
{
    public static class Runtime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (App.IsRuntimeInitialized) return;

            if (Application.isPlaying)
            {
                var app = new GameObject("App");
                app.AddComponent<App.GlobalComponent>();
                UnityEngine.Object.DontDestroyOnLoad(app);

                Data.Init();
                var advertising = new GameObject("Advertising");
                advertising.AddComponent<Advertising>();
                advertising.transform.SetParent(app.transform);
                Advertising.Initialize();

                // Store the timestamp of the *first* init which can be used as a rough approximation of the installation time.
                if (!Data.HasKey(App.FIRST_INSTALL_TIMESTAMP_KEY)) Data.Save(App.FIRST_INSTALL_TIMESTAMP_KEY, DateTime.Now);

                App.IsRuntimeInitialized = true;
                Debug.Log("<color=#52D5F2>Runtime has been initialized!</color>");
            }
        }
    }
}