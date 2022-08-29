using UnityEngine;

namespace Pancake.UIQuery
{
    public static class MapperExtensionsDuplicate
    {
        public static IMapper Duplicate(this IMapper self)
        {
            var rootObject = self.Get();

            var uiCache = rootObject.GetComponent<UICache>();
            if (uiCache == null)
            {
                uiCache = rootObject.AddComponent<UICache>();
                uiCache.Copy(self);
            }

            var clone = Object.Instantiate(rootObject, rootObject.transform.parent);
            return clone.GetComponent<UICache>();
        }

        public static T Duplicate<T>(this T self) where T : IMappedObject, new()
        {
            var newMapper = self.Mapper.Duplicate();
            var newObject = new T();
            newObject.Initialize(newMapper);
            return newObject;
        }
    }
}
