using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Pancake
{
    public static partial class C
    {
        public static void LocationMap()
        {
            var locToKeys = new Dictionary<string, List<object>>();
            foreach (IResourceLocator locator in Addressables.ResourceLocators)
            {
                ResourceLocationMap map = locator as ResourceLocationMap;
                if (map == null) continue;
                foreach (KeyValuePair<object, IList<IResourceLocation>> keyToLocs in map.Locations)
                {
                    foreach (IResourceLocation loc in keyToLocs.Value)
                    {
                        Debug.Log(loc.PrimaryKey);
                        if (!locToKeys.ContainsKey(loc.InternalId)) locToKeys.Add(loc.InternalId, new List<object>(){ keyToLocs.Key });
                        else locToKeys[loc.InternalId].Add(keyToLocs.Key);
                    }
                }
            }
        }
    }
}