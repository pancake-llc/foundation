using System;
using Pancake.Component;
using Pancake.DebugView;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class DebugViewInitialization : Initialize
    {
        [SerializeField] private GameObject debugViewPrefab;
        [SerializeField] private GameObject graphyPrefab;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [Header("level")] [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventNoParam hideUiGameplayEvent;

        public override void Init()
        {
            if (!HeartSettings.DebugView) return;

            const string iconContent =
                "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABHNCSVQICAgIfAhkiAAABgJJREFUeF7tm19oFEccx2c9aPrgXaA16YPJiwbKVWhzFmzuQfTBJg+iYFFoIbSK0IAPJRJBX0qvT63Q/Km2oRbEFoSWRioYLI34YLBgWoqmhSQv6ospVK7SeulDro23/r4rcx2X3Z0/O5vcpi6Inrs78/t+5je/+c3MjsN81+DgiS7HcQ8w5uTp1gv051n/Myn7fY/snXVdNkt/fzEw8M6UaL/Df5RKZ57OZivvO44zQP+XSZlIVXMfEIiP1q5terevr+9fvOQBGBkZebFWc76hn8+rlpTm5wjCL5lM7c3+/v5fPQDk9h86DjuaZlG6thOE49QdjjnDw58Ua7UHP5Drr9EtJM3Pu65by2TcgkOt/xm1fl+axZjaTl5wyhka+piiovOKaSEpf28SHvAXeUBzyoUYmu/eJQ844Rq+vSpeewIgjgc0NTV5XlCtVhPxhra29Y+VW6kssEqlYrUuYw+A+H379rBcLsfGx79jd+7MWzGso2MDy+fzrL19PeOAxYLL5TI7e/ZrK3V5maCJB3DxLS0tdUOmpn5i1679aGwYytq+fSvzt7q/wHL5DwLwlXE9/he1AQSJ54XOz//GLly4qN0lNm3Ks+7uHVJRED829q12+VEFawGIEs8rQTzQ6RIrKV6rC6iIF0mrdIn29ja2d++eFWl5XqmyBwBAb+8bFPSyUoNVu8TBg295QTTqSsLtxfqUAeAlBKre3teVAfAhMqhLbN7cybZt27qi4rW6ALdUxfAgVZOTV9n169P1W7LWT7rltbuAKGr37p1s48YNWp6AEQIRHBfcHgCirtnZOTYxcVm5Dh6j8IJOnqDVBbg1pvGAA1DxIhGYjII/QI+Ofq48VBoBMIkHoqCuri2sWJTPwIeHT8q0e9kiMlIxKTt37rxyZmoMAJaptKQ4InAPUAVw+vSXkbl/2NC8bAAgTjUeiB7wyPDXqNXWRbZwlJCovGRZAejEg0uXLrOZmTlPtAoEQMP8wj8LlCVlMs8RqcfqArwg1YwOz+tCEI0FkPv3K6y1dd1jfd7vRiqxI9YwGOS3iAdBQ2PQ7C4OBFlU1B0+rXhAlFFhk52kIOj0f9idOABUslwQdHIH611A5pqqEIrFLWxx8b8lttbWFqWss1r9x8s0sWKkcy2LB3CDVCAEGY+oXyi8RHlHgUaPpwL1iV2qoQAgQ8NCJl84NYUAUSgLeYd/Sm4qPvEYAIORpgLA2Nh5KQQsomSzWdbcnPOSJMwI0a8xi+QAxfwB+QHEx1mQTawLcPF8ZRd9UwVCkPv6l9lQJjxJnF7ruL31RMhfuV88vx8Hgj+JMhXsf8+6B4SJD4OgOjHi78fp70HQrAKQiecG3Lgxza5cuer9xF5AodCp1aA2IVgDoCoeSjHBQcDDhVmhbDMkiI4tCFYA6Ii3BcBWTIgNQFe8TQA2IMQCYCIeRosbnDqrSmGBIk53MAZgKh4ikLePjp7y9OgsqERFSlMIRgDiivdPWnQWVGxD0AZgWzwXpLpJKhsvx8cvsps3b8seq9/XAgB3xYZG0IcLshpVpqs2ICBtxpqg6lcrWgB27drJ8AWH7qUi3qYn6KwKKQNAqx869HZd+61bt+vZXE/PjtBkRke8LQiJAPAHKnH7KWyvz0R8XAi6n9Aoe4AugDjiTSGY1KkMwN8FsMGBLW9cWKUR83kTQ8LiimpgNK1TGQAXKtsWNzUkKrDKIMSpUwvAo2Fwf+jCZBxDZCNLGIS4dWoBgJEIeEFR3/QTOZlw8T6G4O7uV+sNgICHjyh0l8LFMrUB8JcBAouXuLBfZ/sT1jAw8ELsFeCKsxjKyzcGoNNyjfzsEwB0YuR32iJ8rpFbKTnb6MAEnRj5nk6M9CRXSeOWTGeGJgDgf3dkjjcJjs5RDPi0w3WXZujYXPCuY+M2YFzL/l5aYp3ewcmhoZPHGHM/iFtimt6n1j9MBydHPAClUmlNNvvMzxQLCmkSYW6rO12p/Pky6a75Dk8vlAjCESp4FR+edgcXFnLvlUoHFgGwDoDTxPF5+vd+AoGj86vm+Dx18TnXdc74j88/BDgUy/kNG7vjAAAAAElFTkSuQmCC";

            var toolTex = new Texture2D(64, 64);
            toolTex.LoadImage(Convert.FromBase64String(iconContent));
            var toolIcon = Sprite.Create(toolTex, new Rect(0.0f, 0.0f, toolTex.width, toolTex.height), new Vector2(0.5f, 0.5f), 100.0f);

            Instantiate(debugViewPrefab);
            if (graphyPrefab != null) Instantiate(graphyPrefab);
            var initialPage = DebugSheet.Instance.GetOrCreateInitialPage();
            initialPage.AddPageLinkButton<DebugToolsPage>("Debug Tools", icon: toolIcon);
            initialPage.AddPageLinkButton<AdsToolsPage>("Ads Tools");
            initialPage.AddPageLinkButton<LevelToolsPage>("Level Tools",
                onLoad: tuple =>
                {
                    tuple.page.Setup(fxCoinSpawnEvent,
                        currentLevelIndex,
                        loadLevelEvent,
                        reCreateLevelLoadedEvent,
                        hideUiGameplayEvent);
                });
            initialPage.Reload();
        }
    }
}