#if PANCAKE_IAP
using System;
using UnityEngine;

namespace Pancake.IAP
{
    public static class IAPInitManager
    {
        private static bool isInitialized;

        public static void Init()
        {
            if (isInitialized) return;

            if (Application.isPlaying)
            {
                IAPManager.Init();
                isInitialized = true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitalize()
        {
            if (IAPSettings.RuntimeAutoInitialize) Init();
        }
    }
    
    public static class IAPChain
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        
        public static IAPData OnCompleted(this IAPData product, Action onCompleted)
        {
            IAPManager.RegisterCompletedEvent(product.sku.Id, onCompleted);
            return product;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static IAPData OnError(this IAPData product, Action onError)
        {
            IAPManager.RegisterFaildEvent(product.sku.Id, onError);
            return product;
        }
    }
}
#endif