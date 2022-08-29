using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pancake.UIQuery.Editor
{
    public static class UIElementTester
    {
        public static void Test<T>(GameObject original) where T : IMappedObject, new()
        {
            var root = new GameObject("DummyRoot");
            var gameObject = Object.Instantiate(original);
            var uiCache = gameObject.GetComponent<UICache>();
            var target = new T();
            target.Initialize(uiCache);

            var wdt = 1000;
            var testTargets = new List<IMappedObject> { target };
            while (testTargets.Count > 0)
            {
                wdt--;
                if (wdt < 0)
                {
                    throw new System.Exception("wdt < 0");
                }

                var o = testTargets.First();
                testTargets.RemoveAt(0);
                Assert.IsNotNull(o);
                var type = o.GetType();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.GetCustomAttributes().Any(x => x is IgnoreTestMemberAttribute)) continue;
                    Assert.IsTrue(property.CanRead, $"{o.GetType()} : {property.Name} can not read");

                    var value = property.GetValue(o);
                    Assert.IsNotNull(value, $"{o.GetType()} : {property.Name} == null");

                    if (value is IMappedObject mappedObject)
                    {
                        testTargets.Add(mappedObject);
                        continue;
                    }

                    if (value is IMappedObjectList mappedObjectList)
                    {
                        testTargets.AddRange(mappedObjectList.MappedObjects);
                        continue;
                    }

                    if (value is IEnumerable<IMappedObject> mappedObjects)
                    {
                        testTargets.AddRange(mappedObjects);
                        continue;
                    }

                    if (value is IEnumerable<object> enumerable)
                    {
                        foreach (var (x, i) in enumerable.Select((x, i) => (x, i)))
                        {
                            Assert.IsNotNull(x, $"{o.GetType()} : {property.Name}[{i}] == null");
                        }
                    }
                }
            }

            Object.DestroyImmediate(root);
        }
    }
}
