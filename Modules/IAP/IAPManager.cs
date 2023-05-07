using System;
using System.Collections.Generic;
using Pancake.Apex;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [HideMonoScript]
    public class IAPManager : GameComponent, IStoreListener
    {
        [SerializeField, Array] private List<IAPDataVariable> products;
        [SerializeField] private ScriptableEventIAPData iapPurchaseEvent;

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        public bool IsInitialized { get; set; }

        protected override void OnEnabled()
        {
            base.OnEnabled();
            iapPurchaseEvent.OnRaised += PurchaseProduct;
        }

        private void PurchaseProduct(IAPDataVariable product)
        {
            // call when IAPDataVariable raise event
            PurchaseProductInternal(product);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            iapPurchaseEvent.OnRaised -= PurchaseProduct;
        }

        private void Start() { Init(); }

        private async void Init()
        {
            var options = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(options);
            InitImpl();
        }

        private void InitImpl()
        {
            if (IsInitialized) return;

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
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);
                Debug.Log("Receipt is valid");
            }
            catch (IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
#endif

            if (validPurchase) PurchaseVerified(purchaseEvent);

            return PurchaseProcessingResult.Complete;
        }

        private IAPDataVariable PurchaseProductInternal(IAPDataVariable product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _controller?.InitiatePurchase(product.Value.id);
#endif
            return product;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }

        private void PurchaseVerified(PurchaseEventArgs e)
        {
            string id = e.purchasedProduct.definition.id;
            foreach (var product in products)
            {
                if (product.id.Equals(id)) product.onPurchaseSuccess.Raise();
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            // Overall Purchasing system, configured with products for this application.
            _controller = controller;
            _extensions = extensions;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            foreach (var product in _controller.products.all)
            {
                if (product != null && string.IsNullOrEmpty(product.transactionID)) _controller.ConfirmPendingPurchase(product);
            }
#endif
        }

        public bool IsPurchased(string sku)
        {
            if (_controller == null) return false;
            var type = GetIapType(sku);
            return type == ProductType.NonConsumable && _controller.products.WithID(sku).hasReceipt;
        }

        private void RequestProductData(ConfigurationBuilder builder)
        {
            foreach (var p in products)
            {
                builder.AddProduct(p.id, p.isTest ? ProductType.Consumable : p.productType);
            }
        }

        private ProductType GetIapType(string sku)
        {
            foreach (var item in products)
            {
                if (item.id.Equals(sku)) return item.productType;
            }

            return ProductType.Consumable;
        }

#if UNITY_IOS
        public void RestorePurchase()
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
    }
}