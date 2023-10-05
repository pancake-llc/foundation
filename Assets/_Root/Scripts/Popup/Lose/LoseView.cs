using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class LoseView : View
    {
	    [SerializeField] private Button buttonHome;
	    [SerializeField] private Button buttonReplay;
	    [SerializeField] private Button buttonShop;
	    [SerializeField] private Button buttonSkip;
	    [Header("EVENT")] [SerializeField] private ScriptableEventString changeSceneEvent;
	    [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
	    [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
	    [SerializeField] private ScriptableEventNoParam showUiGameplayEvent;
	    [SerializeField] private IntVariable currentLevelIndex;
	    [SerializeField] private RewardVariable rewardVariable;
	    
	    
    	protected override UniTask Initialize()
    	{
        	return UniTask.CompletedTask;
    	}

        public override void Refresh() {  }
    }
}
