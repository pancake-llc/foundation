using Pancake.Editor;
using UnityEditor;

[assembly: RegisterTriAttributeValidator(typeof(SceneValidator))]

namespace Pancake.Editor
{
    public class SceneValidator : AttributeValidator<SceneAttribute>
    {
        public override ValidationResult Validate(Property property)
        {
            if (property.FieldType == typeof(string))
            {
                var value = property.Value;

                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (!property.Comparer.Equals(value, scene.path))
                    {
                        continue;
                    }

                    if (!scene.enabled)
                    {
                        return ValidationResult.Error($"{value} not in build settings");
                    }

                    return ValidationResult.Valid;
                }
            }

            return ValidationResult.Error($"{property.Value} not a valid scene");
        }
    }
}