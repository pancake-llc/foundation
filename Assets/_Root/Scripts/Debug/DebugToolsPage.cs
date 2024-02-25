using System;
using System.Threading.Tasks;
using Pancake.DebugView;
using Tayx.Graphy;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public sealed class DebugToolsPage : DefaultDebugPageBase
    {
        private const string SETTING_ICON =
            "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";

        private const string FPS_ICON =
            "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";

        protected override string Title => "Debug Tools";

        public override Task Initialize()
        {
            var settingTex = new Texture2D(64, 64);
            settingTex.LoadImage(Convert.FromBase64String(SETTING_ICON));
            var settingIcon = Sprite.Create(settingTex, new Rect(0.0f, 0.0f, settingTex.width, settingTex.height), new Vector2(0.5f, 0.5f), 100.0f);

            var fpsTex = new Texture2D(64, 64);
            fpsTex.LoadImage(Convert.FromBase64String(FPS_ICON));
            var fpsIcon = Sprite.Create(fpsTex, new Rect(0.0f, 0.0f, fpsTex.width, fpsTex.height), new Vector2(0.5f, 0.5f), 100.0f);

            // Graphy
            AddPageLinkButton<GraphyDebugPage>("Graphy", icon: fpsIcon, onLoad: x => x.page.Setup(GraphyManager.Instance));

            // System Info
            AddPageLinkButton<SystemInfoDebugPage>("System Info", icon: settingIcon);

            // Time
            AddPageLinkButton<TimeDebugPage>("Time", icon: settingIcon);

            // Application
            AddPageLinkButton<ApplicationDebugPage>("Application", icon: settingIcon);

            // Screen
            AddPageLinkButton<ScreenDebugPage>("Screen", icon: settingIcon);

            // Quality Settings
            AddPageLinkButton<QualitySettingsDebugPage>("Quality Settings", icon: settingIcon);

            // Input
            AddPageLinkButton<InputDebugPage>("Input", icon: settingIcon);

            // Physics
            AddPageLinkButton<PhysicsDebugPage>("Physics", icon: settingIcon);

            // Physics 2D
            AddPageLinkButton<Physics2DDebugPage>("Physics 2D", icon: settingIcon);

            // Graphics
            AddPageLinkButton<GraphicsDebugPage>("Graphics", icon: settingIcon);

            Reload();

            return Task.CompletedTask;
        }
    }
}