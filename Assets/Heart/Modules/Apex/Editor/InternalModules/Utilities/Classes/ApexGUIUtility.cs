namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for visual entities.
    /// </summary>
    public static class ApexGUIUtility
    {
        /// <summary>
        /// Smoothly animate updating height of visual entities.
        /// </summary>
        public static bool Animate { get; set; } = true;
        
        /// <summary>
        /// Horizontal spacing between visual entities.
        /// </summary>
        public static float HorizontalSpacing { get; internal set; } = 2;

        /// <summary>
        /// Top and bottom bounds of group style containers.
        /// </summary>
        public static float BoxBounds { get; internal set; } = 5;
    }
}