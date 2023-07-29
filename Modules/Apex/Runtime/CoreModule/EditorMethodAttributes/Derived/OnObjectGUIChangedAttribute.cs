namespace Pancake.Apex
{
    public sealed class OnObjectGUIChangedAttribute : EditorMethodAttribute 
    {
        public OnObjectGUIChangedAttribute()
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