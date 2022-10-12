using UnityEngine;

namespace Pancake.Editor
{
    /// <summary>
    /// A method that is called before or after the header GUI of a component is drawn.
    /// </summary>
    /// <param name="component"> The component whose header is being drawn. </param>
    /// <param name="headerRect"> The position and dimensions of the header GUI. </param>
    /// <param name="HeaderIsSelected"> Is the header currently selected in the Inspector or not? </param>
    /// <param name="supportsRichText"> Does the header that is being drawn support rich text tags or not? </param>
    /// <returns> The height of the element that this method has drawn before or after the header GUI. </returns>
    public delegate float HeaderGUIHandler([JetBrains.Annotations.NotNull] Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText);

    /// <summary>
    /// A class that contains callbacks
    /// </summary>
    public static class ComponentHeader
	{
        /// <summary>
        /// Callback invoked right before component header GUI is drawn.
        /// </summary>
        public static event HeaderGUIHandler BeforeHeaderGUI;

        /// <summary>
        /// Callback invoked right after component header GUI has been drawn.
        /// </summary>
        public static event HeaderGUIHandler AfterHeaderGUI;

        internal static float InvokeBeforeHeaderGUI([JetBrains.Annotations.NotNull] Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
			=> BeforeHeaderGUI is null ? 0f : BeforeHeaderGUI.Invoke(component, headerRect, HeaderIsSelected, supportsRichText);

        internal static float InvokeAfterHeaderGUI([JetBrains.Annotations.NotNull] Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
			=> AfterHeaderGUI is null ? 0f : AfterHeaderGUI.Invoke(component, headerRect, HeaderIsSelected, supportsRichText);
	}
}