#if PANCAKE_IAP
using System;
using System.Collections;
using Pancake.Common;
using Pancake.Monetization;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Pancake.IAP
{
    [EditorIcon("icon_manager")]
    public class IAPManager : GameComponent, IDetailedStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private static event Action<IAPDataVariable> PurchaseProductEvent;
        private static event Func<IAPDataVariable, bool> CheckOwnProductEvent;
        private static event Action RestoreProductEvent;

        public bool IsInitialized { get; set; }

        #region Implement

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;

            StartCoroutine(InitializeProducts());
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    Debug.LogWarning("Billing disabled!");
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    Debug.LogWarning("No products available for purchase!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(error), error, null);
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) { OnInitializeFailed(error); }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) { InternalPurchaseFailed(product.definition.id, failureDescription.reason.ToString()); }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { InternalPurchaseFailed(product.definition.id, failureReason.ToString()); }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            bool validPurchase = IsValidReceipt(purchaseEvent.purchasedProduct.receipt, out _);
            if (validPurchase) PurchaseVerified(purchaseEvent);
            return PurchaseProcessingResult.Complete;
        }

        #endregion

        #region Unity Function

        private void Start() { Init(); }

        protected void OnEnable()
        {
            PurchaseProductEvent += PurchaseProduct;
            CheckOwnProductEvent += IsPurchasedProduct;
#if UNITY_IOS
            RestoreProductEvent += RestorePurchase;
#endif
        }

        protected void OnDisable()
        {
            PurchaseProductEvent -= PurchaseProduct;
            CheckOwnProductEvent -= IsPurchasedProduct;
#if UNITY_IOS
            RestoreProductEvent -= RestorePurchase;
#endif
        }

        #endregion

        private bool IsPurchasedProduct(IAPDataVariable product)
        {
            if (_controller == null) return false;
            return product.productType == ProductType.NonConsumable && _controller.products.WithID(product.id).hasReceipt;
        }

        private void PurchaseProduct(IAPDataVariable product)
        {
            Advertising.ChangePreventDisplayAppOpen(true);
            PurchaseProductInternal(product);
        }

        private async void Init()
        {
            if (IsInitialized) return;

#if PANCAKE_UNITASK
            await UniTask.WaitUntil(() => Static.IsUnitySeriveReady);
#endif

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            RequestProductData(builder);
            builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnPurchaseDeferred);

            UnityPurchasing.Initialize(this, builder);
            IsInitialized = true;
        }

        private IEnumerator InitializeProducts()
        {
            yield return new WaitForSeconds(1f);
            for (var i = 0; i < IAPSettings.Products.Count; i++)
            {
                var product = _controller.products.WithID(IAPSettings.Products[i].id);

                if (IAPSettings.Products[i].productType == ProductType.Subscription)
                {
                    if (product is {hasReceipt: true} && IsValidReceipt(product.receipt, out _))
                    {
                        IAPSettings.Products[i].OnPurchaseSuccess?.Raise();
                        var subscriptionManager = new SubscriptionManager(product, null);
                        IAPSettings.Products[i].receipt = product.receipt;
                        IAPSettings.Products[i].subscriptionInfo = subscriptionManager.getSubscriptionInfo();
                    }
                }

                if (IAPSettings.Products[i].productType == ProductType.NonConsumable)
                {
                    if (product is {hasReceipt: true} && IsValidReceipt(product.receipt, out _)) IAPSettings.Products[i].OnPurchaseSuccess?.Raise();
                }

                if (product != null && product.availableToPurchase)
                {
                    IAPSettings.Products[i].localizedPrice = product.metadata.localizedPriceString;
                    IAPSettings.Products[i].price = decimal.ToInt32(product.metadata.localizedPrice);
                    IAPSettings.Products[i].isoCurrencyCode = product.metadata.isoCurrencyCode;
                    IAPSettings.Products[i].localizedDescription = product.metadata.localizedDescription;
                    IAPSettings.Products[i].localizedTitle = product.metadata.localizedTitle;
                }
            }
        }

        private void OnPurchaseDeferred(Product product)
        {
            Debug.Log("Deferred product " + product.definition.id);
            // TODO Deferred purchasing successful events. Do not grant the item here. Instead, record the purchase and remind the user to complete the transaction in the Play Store.
        }

        private IAPDataVariable PurchaseProductInternal(IAPDataVariable product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _controller?.InitiatePurchase(product.id);
#elif UNITY_EDITOR
            InternalPurchaseDone(product.id);
#endif
            return product;
        }

        private void InternalPurchaseFailed(string id, string reason)
        {
            Advertising.ChangePreventDisplayAppOpen(false);
            foreach (var p in IAPSettings.Products)
            {
                if (!p.id.Equals(id)) continue;
                p.OnPurchaseFailed.Raise(reason);
                C.CallActionClean(ref p.purchaseFailedCallback, reason);
            }
        }

        private void PurchaseVerified(PurchaseEventArgs e)
        {
            Advertising.ChangePreventDisplayAppOpen(false);
            InternalPurchaseDone(e.purchasedProduct.definition.id);
        }

        private void InternalPurchaseDone(string id)
        {
            foreach (var product in IAPSettings.Products)
            {
                if (!product.id.Equals(id)) continue;
                product.OnPurchaseSuccess.Raise();
                C.CallActionClean(ref product.purchaseSuccessCallback);
            }
        }

        private void RequestProductData(ConfigurationBuilder builder)
        {
            foreach (var p in IAPSettings.Products)
            {
                builder.AddProduct(p.id, p.productType);
            }
        }

#if UNITY_IOS
        private void RestorePurchase()
        {
            if (!IsInitialized)
            {
                Debug.Log("Restore purchases fail. not initialized!");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("Restore purchase started ...");

                var storeProvider = _extensions.GetExtension<IAppleExtensions>();
                storeProvider.RestoreTransactions(_ =>
                {
                    // no purchase are avaiable to restore
                    Debug.Log("Restore purchase continuting: " + _ + ". If no further messages, no purchase available to restore.");
                });
            }
            else
            {
                Debug.Log("Restore purchase fail. not supported on this platform. current = " + Application.platform);
            }
        }
#endif

        internal static void Purchase(IAPDataVariable product) { PurchaseProductEvent?.Invoke(product); }

        internal static bool IsPurchased(IAPDataVariable product) { return CheckOwnProductEvent != null && CheckOwnProductEvent.Invoke(product); }

        public static void Restore() { RestoreProductEvent?.Invoke(); }

        private bool IsValidReceipt(string receipt, out UnityEngine.Purchasing.Security.IAPSecurityException exception)
        {
            exception = null;
            // ReSharper disable once ConvertToConstant.Local
            var validPurchase = true;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
            var validator = new UnityEngine.Purchasing.Security.CrossPlatformValidator(UnityEngine.Purchasing.Security.GooglePlayTangle.Data(),
                UnityEngine.Purchasing.Security.AppleTangle.Data(),
                Application.identifier);

            try
            {
                validator.Validate(receipt);
            }
            catch (UnityEngine.Purchasing.Security.IAPSecurityException e)
            {
                exception = e;
                validPurchase = false;
                throw;
            }

#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return validPurchase;
        }
    }
}
#endif