using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class DebugViewInitialization : Initialize
    {
        [SerializeField] private GameObject debugViewPrefab;
        [SerializeField] private StringConstant coinType;
        [Header("level")] [SerializeField] private StringConstant levelType;
        [SerializeField] private ScriptableEventNoParam hideUiGameplayEvent;

        public override void Init()
        {
            if (!HeartSettings.DebugView) return;

            Instantiate(debugViewPrefab);
            // var initialPage = DebugSheet.Instance.GetOrCreateInitialPage();
            // initialPage.AddPageLinkButton<DebugToolsPage>("Debug Tools", icon: toolIcon);
            // initialPage.AddPageLinkButton<AdsToolsPage>("Ads Tools");
            // initialPage.AddPageLinkButton<LevelToolsPage>("Level Tools", onLoad: tuple => { tuple.page.Setup(coinType, levelType, hideUiGameplayEvent); });
            //initialPage.Reload();
        }
    }
}