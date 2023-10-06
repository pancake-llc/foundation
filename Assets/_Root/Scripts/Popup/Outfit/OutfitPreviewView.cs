using Pancake.Threading.Tasks;

namespace Pancake.UI
{
    public sealed class OutfitPreviewView : View
    {
    	protected override UniTask Initialize()
    	{
        	return UniTask.CompletedTask;
    	}

        public override void Refresh() {  }
    }
}
