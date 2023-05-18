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
        /// <param name="onFaild"></param>
        /// <returns></returns>
        public static IAPDataVariable OnPurchaseFailed(this IAPDataVariable product, Action onFaild)
        {
            product.purchaseFaildCallback = onFaild;
            return product;
        }

        /// <summary>
        /// wrapper 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="event">event purchase</param>
        public static void Purchase(this IAPDataVariable product, ScriptableEventIAPProduct @event) { @event.Raise(product); }

        /// <summary>
        /// wrapper
        /// </summary>
        /// <param name="product"></param>
        /// <param name="event">event check is product purchased</param>
        public static bool IsPurchased(this IAPDataVariable product, ScriptableEventIAPFuncProduct @event) { return @event.Raise(product); }
    }
}