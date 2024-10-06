#if UNITY_UGUI
using TMPro;

namespace Sisus.Init.Internal.TMPro
{
    /// <summary>
    /// Sets the <see cref="TMP_InputField.text"/> property of a <see cref="TMP_InputField"/>
    /// component to an Inspector-assigned value when the object is being loaded.
    /// </summary>
    internal sealed class InputFieldTextInitializer : CustomInitializer<TMP_InputField, string>
    {
#if UNITY_EDITOR
        /// <summary>
        /// This section can be used to customize how the Init arguments will be drawn in the Inspector.
        /// <para>
        /// The Init argument names shown in the Inspector will match the names of members defined inside this section.
        /// </para>
        /// <para>
        /// Any PropertyAttributes attached to these members will also affect the Init arguments in the Inspector.
        /// </para>
        /// </summary>
        private sealed class Init
        {
            public string text = default;
        }
#endif

        protected override void InitTarget(TMP_InputField target, string text) => target.text = text;
    }
}
#endif