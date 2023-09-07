#if PANCAKE_IAP
using System;
using Pancake.Scriptable;

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
        public static IAPDataVariable OnPurchaseFailed(this IAPDataVariable product, Action onFailed)
        {
            product.purchaseFailedCallback = onFailed;
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

        /// <summary>
        /// restore purchase wrapper
        /// </summary>
        /// <param name="event"></param>
        public static void RestorePurchase(this ScriptableEventIAPNoParam @event) { @event.Raise(); }
    }
}
#endif