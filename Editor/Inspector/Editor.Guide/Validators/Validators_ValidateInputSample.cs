namespace Pancake.Editor.Guide
{
    using UnityEngine;

    public class Validators_ValidateInputSample : ScriptableObject
    {
        [ValidateInput(nameof(ValidateTexture))] public Texture tex;

        private ValidationResult ValidateTexture()
        {
            if (tex == null) return ValidationResult.Error("Tex is null");
            if (!tex.isReadable) return ValidationResult.Warning("Tex must be readable");
            return ValidationResult.Valid;
        }
    }
}