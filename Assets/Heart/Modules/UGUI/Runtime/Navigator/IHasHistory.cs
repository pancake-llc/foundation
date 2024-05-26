#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public interface IHasHistory
    {
        UniTask PrevAsync(int count = 1);
    }
}
#endif