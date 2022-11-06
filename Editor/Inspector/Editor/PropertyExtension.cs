using System;
using JetBrains.Annotations;

namespace Pancake.Editor
{
    public abstract class PropertyExtension
    {
        public bool? ApplyOnArrayElement { get; internal set; }

        [PublicAPI]
        public virtual ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition) { return ExtensionInitializationResult.Ok; }
    }

    public readonly struct ExtensionInitializationResult
    {
        public ExtensionInitializationResult(bool shouldApply, string errorMessage)
        {
            ShouldApply = shouldApply;
            ErrorMessage = errorMessage;
        }

        public bool ShouldApply { get; }
        public string ErrorMessage { get; }
        public bool IsError => ErrorMessage != null;

        [PublicAPI] public static ExtensionInitializationResult Ok => new ExtensionInitializationResult(true, null);

        [PublicAPI] public static ExtensionInitializationResult Skip => new ExtensionInitializationResult(false, null);

        [PublicAPI]
        public static ExtensionInitializationResult Error([NotNull] string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            return new ExtensionInitializationResult(false, errorMessage);
        }

        [PublicAPI]
        public static implicit operator ExtensionInitializationResult(string errorMessage) { return Error(errorMessage); }
    }
}