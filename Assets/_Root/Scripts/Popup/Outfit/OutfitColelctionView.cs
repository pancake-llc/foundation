using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitColelctionView : View
    {
	    [SerializeField] private OutfitType outfitType;
	    
	    
    	protected override UniTask Initialize()
    	{
        	return UniTask.CompletedTask;
    	}

	    public override void Refresh()
	    {
		    
	    }
    }
}
