using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// ScriptableAssetSingleton, the created asset can be added to 'Preloaded Assets' list in player settings automatically.
    /// </summary>
    public class ScriptableAssetSingleton<T> : ScriptableAsset where T : ScriptableAssetSingleton<T>
    {
        private static T instance;


        /// <summary>
        /// The asset Instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!instance)
                {
#if UNITY_EDITOR
                    instance = EditorAssetUtilities.FindAsset<T>();
                    if (!instance)
                    {
                        instance = CreateInstance<T>();
                        Debug.LogWarning(string.Format("No asset of ease {0} loaded, a temporary Instance was created. Use {0}.CreateOrSelectAsset to create an asset.",
                            typeof(T).Name));
                    }
#else
                    _instance = CreateInstance<T>();
                    Debug.LogWarning(string.Format("No asset of ease {0} loaded, a temporary Instance was created. Do you forget to add the asset to \"Preloaded Assets\" list?", typeof(T).Name));
#endif
                }

                return instance;
            }
        }


        protected ScriptableAssetSingleton() { instance = this as T; }


#if UNITY_EDITOR

        /// <summary>
        /// Create asset if it does not exist, or just select it if it exist.
        /// The created asset can be added to 'Preloaded Assets' list in player settings automatically.
        /// </summary>
        public static void CreateOrSelectAsset(bool addToPreloadedAssets = true)
        {
            bool needCreate = false;

            if (!instance)
            {
                instance = EditorAssetUtilities.FindAsset<T>();
                if (!instance)
                {
                    instance = CreateInstance<T>();
                    needCreate = true;
                }
            }
            else needCreate = !AssetDatabase.IsNativeAsset(instance);

            if (needCreate) EditorAssetUtilities.CreateAsset(instance);

            if (addToPreloadedAssets) EditorAssetUtilities.AddPreloadedAsset(instance);

            Selection.activeObject = Instance;
        }

#endif
    } // class ScriptableAssetSingleton
} // namespace Pancake