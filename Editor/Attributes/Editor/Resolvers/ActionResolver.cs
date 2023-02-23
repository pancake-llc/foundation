using JetBrains.Annotations;

namespace Pancake.AttributeDrawer
{
    public abstract class ActionResolver
    {
        public static ActionResolver Resolve(PropertyDefinition propertyDefinition, string method)
        {
            if (InstanceActionResolver.TryResolve(propertyDefinition, method, out var iar))
            {
                return iar;
            }

            return new ErrorActionResolver(propertyDefinition, method);
        }

        [PublicAPI]
        public abstract bool TryGetErrorString(out string error);

        [PublicAPI]
        public abstract void InvokeForTarget(Property property, int targetIndex);

        [PublicAPI]
        public void InvokeForAllTargets(Property property)
        {
            for (var targetIndex = 0; targetIndex < property.PropertyTree.TargetsCount; targetIndex++)
            {
                InvokeForTarget(property, targetIndex);
            }
        }
    }
}