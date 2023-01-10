#if PANCAKE_GAM
namespace Pancake.LevelBase
{
    public interface IUnit
    {
        /// <summary>
        /// Call when unit active
        /// </summary>
        public void Active();
        
        /// <summary>
        /// Call when unit deactive
        /// </summary>
        public void Deactive();
    }
}
#endif