namespace Pancake.Apex
{
    public sealed class OnObjectChangedAttribute : EditorMethodAttribute 
    {
        public OnObjectChangedAttribute()
        {
            DelayCall = false;
        }

        #region [Optional]
        /// <summary>
        /// Call callback after all inspectors update.
        /// </summary>
        public bool DelayCall { get; set; }
        #endregion
    }
}