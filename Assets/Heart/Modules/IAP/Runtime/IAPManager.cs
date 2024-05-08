#if PANCAKE_IAP
using System;
using System.Collections.Generic;
using Pancake.Common;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using Pancake.Monetization;
#endif
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Pancake.IAP
{
    public class IAPManager : GameComponent, IDetailedStoreListener
    {
        [SerializeField] private IAPSettings iapSettings;
#if UNITY_IOS
        [SerializeField] private ScriptableEventIAPNoParam restoreEvent;
#endif

        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private static event Action<IAPDataVariable> PurchaseProductEvent;
        private static event Func<IAPDataVariable, bool> CheckOwnProductEvent;
        private static event Action RestoreProductEvent;

        public bool IsInitialized { get; set; }
        public static bool IsServiceInitialized { get; set; }

        protected void OnEnable()
        {
            PurchaseProductEvent += PurchaseProduct;
            CheckOwnProductEvent += IsPurchasedProduct;
#if UNITY_IOS
            RestoreProductEvent += RestorePurchase;
#endif
        }

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

        protected void OnDisable()
        {
            PurchaseProductEvent -= PurchaseProduct;
            CheckOwnProductEvent -= IsPurchasedProduct;
#if UNITY_IOS
            RestoreProductEvent -= RestorePurchase;
#endif
        }

        private void Start() { Init(); }

        private async void Init()
        {
            if (IsInitialized) return;

#if PANCAKE_UNITASK
            await UniTask.WaitUntil(() => IsServiceInitialized);
#endif

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            RequestProductData(builder);
            builder.Configure<IGooglePlayConfiguration>();

            UnityPurchasing.Initialize(this, builder);
            IsInitialized = true;
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

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            bool validPurchase = true;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
            var validator =
 new UnityEngine.Purchasing.Security.CrossPlatformValidator(UnityEngine.Purchasing.Security.GooglePlayTangle.Data(), UnityEngine.Purchasing.Security.AppleTangle.Data(), Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);
                Debug.Log("Receipt is valid");
            }
            catch (UnityEngine.Purchasing.Security.IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (validPurchase) PurchaseVerified(purchaseEvent);

            return PurchaseProcessingResult.Complete;
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

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { InternalPurchaseFailed(product.definition.id); }

        private void InternalPurchaseFailed(string id)
        {
            Advertising.ChangePreventDisplayAppOpen(false);
            foreach (var p in iapSettings.Products)
            {
                if (!p.id.Equals(id)) continue;
                p.OnPurchaseFailed.Raise();
                C.CallActionClean(ref p.purchaseFailedCallback);
            }
        }

        private void PurchaseVerified(PurchaseEventArgs e)
        {
            Advertising.ChangePreventDisplayAppOpen(false);
            InternalPurchaseDone(e.purchasedProduct.definition.id);
        }

        private void InternalPurchaseDone(string id)
        {
            foreach (var product in iapSettings.Products)
            {
                if (!product.id.Equals(id)) continue;
                product.OnPurchaseSuccess.Raise();
                C.CallActionClean(ref product.purchaseSuccessCallback);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            // Overall Purchasing system, configured with products for this application.
            _controller = controller;
            _extensions = extensions;
#if UNITY_ANDROID && !UNITY_EDITOR
            foreach (var product in _controller.products.all)
            {
                if (product != null && !string.IsNullOrEmpty(product.transactionID)) _controller.ConfirmPendingPurchase(product);
            }
#endif
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) { InternalPurchaseFailed(product.definition.id); }

        private void RequestProductData(ConfigurationBuilder builder)
        {
            foreach (var p in iapSettings.Products)
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
    }
}
#endif