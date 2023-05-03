using System;

namespace Pancake.IAP
{
    public static class IAPStatic
    {
        /// <summary>
        /// Call when purchase product success
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
        /// Call when purchase product fail
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