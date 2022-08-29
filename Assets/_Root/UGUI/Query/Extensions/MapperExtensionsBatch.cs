using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery
{
    public static class MapperExtensionsBatch
    {
        public static void Batch<T>(this IMapper self, Dictionary<string, Action<T>> operations) where T : Component
        {
            foreach (var operation in operations)
            {
                operation.Value(self.Get<T>(operation.Key));
            }
        }

        public static void Batch<T1, T2>(this T1 self, Dictionary<string, Action<T2>> operations) where T1 : IMappedObject where T2 : Component
        {
            self.Mapper.Batch(operations);
        }

        public static void SetText(this IMapper self, Dictionary<string, string> texts)
        {
            self.Batch(texts.ToDictionary(x => x.Key, x => (Action<Text>) (ui => ui.text = x.Value)));
        }

        public static void SetText<T1>(this T1 self, Dictionary<string, string> texts) where T1 : IMappedObject
        {
            self.Mapper.SetText(texts);
        }
    }
}
