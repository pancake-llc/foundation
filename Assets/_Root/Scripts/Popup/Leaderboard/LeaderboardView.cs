using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class LeaderboardView : View
    {
	    [SerializeField] private string tableId;
	    [SerializeField] private Button buttonClose;
	    
    	protected override UniTask Initialize()
    	{
		    
        	return UniTask.CompletedTask;
    	}
    }
}
