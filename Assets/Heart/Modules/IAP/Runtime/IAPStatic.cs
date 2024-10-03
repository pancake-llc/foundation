#if PANCAKE_IAP
using System;

namespace Pancake.IAP
{
    public static class IAPStatic
    {
        /// <summary>
        /// wrapper
        /// </summary>
        /// <param name="product"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public static IAPDataVariable OnPurchaseCompleted(this IAPDataVariable product, Action onComplete)
        {
            product.purchaseSuccessCallback = onComplete;
            return product;
        }

        /// <summary>
        /// wrapper
        /// </summary>
        /// <param name="product"></param>
        /// <param name="onFailed"></param>
        /// <returns></returns>
        public static IAPDataVariable OnPurchaseFailed(this IAPDataVariable product, Action<string> onFailed)
        {
            product.purchaseFailedCallback = onFailed;
            return product;
        }

        /// <summary>
        /// wrapper 
        /// </summary>
        /// <param name="product"></param>
        public static void Purchase(this IAPDataVariable product) { IAPManager.Purchase(product); }

        /// <summary>
        /// wrapper
        /// </summary>
        /// <param name="product"></param>
        public static bool IsPurchased(this IAPDataVariable product) { return IAPManager.IsPurchased(product); }
    }
}
#endif