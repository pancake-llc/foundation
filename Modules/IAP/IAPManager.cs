using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Attribute;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [AddComponentMenu("")]
    [HideMono]
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static event Action<string> OnPurchaseSucceedEvent;
        public static event Action<string> OnPurchaseFailedEvent;
        public static event Action OnPurchaseEvent;
        private static readonly Dictionary<string, Action> CompletedDict = new Dictionary<string, Action>();
        private static readonly Dictionary<string, Action> FaildDict = new Dictionary<string, Action>();

        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private static IAPManager Instance { get; set; }

        public List<IAPData> Skus { get; set; } = new List<IAPData>();
        public bool IsInitialized { get; set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public static async void Init()
        {
            OnPurchaseSucceedEvent = null;
            OnPurchaseFailedEvent = null;
            OnPurchaseEvent = null;
            var options = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(options);
            Instance.InitImpl(IAPSettings.SkusData);
        }

        private void InitImpl(List<IAPData> skuItems)
        {
            if (this != Instance) return;

            if (IsInitialized) return;
            Skus.Clear();
            Skus.AddRange(skuItems);
            CompletedDict.Clear();
            foreach (var item in skuItems)
            {
                CompletedDict.Add(item.sku.Id, null);
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            RequestProductData(builder);
            builder.Configure<IGooglePlayConfiguration>();

            UnityPurchasing.Initialize(this, builder);
            IsInitialized = true;
        }

        public static void ForceInit(List<IAPData> skuItems) { Instance.InitImpl(skuItems); }

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

            if (validPurchase)
            {
                PurchaseVerified(purchaseEvent);
            }

            return PurchaseProcessingResult.Complete;
        }

        private IAPData PurchaseProduct(IAPData product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _controller?.InitiatePurchase(product.sku.Id);
#endif
            return product;
        }

        public static IAPData Purchase(IAPData product)
        {
            OnPurchaseEvent?.Invoke();
            return Instance.PurchaseProduct(product);
        }

        public static void RegisterCompletedEvent(string key, Action action)
        {
            foreach (var e in CompletedDict.ToArray())
            {
                if (e.Key.Equals(key)) CompletedDict[key] = action;
            }
        }

        public static void RegisterFaildEvent(string key, Action action)
        {
            foreach (var e in FaildDict.ToArray())
            {
                if (e.Key.Equals(key)) FaildDict[key] = action;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailedEvent?.Invoke(failureReason.ToString());
            foreach (var e in FaildDict)
            {
                if (e.Key.Equals(product.definition.id)) e.Value?.Invoke();
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

        private void PurchaseVerified(PurchaseEventArgs e)
        {
            OnPurchaseSucceedEvent?.Invoke(e.purchasedProduct.definition.id);
            foreach (var completeEvent in CompletedDict)
            {
                if (completeEvent.Key.Equals(e.purchasedProduct.definition.id)) completeEvent.Value?.Invoke();
            }
        }

        private void RequestProductData(ConfigurationBuilder builder)
        {
            foreach (var p in Skus)
            {
                if (IAPSettings.TestMode)
                {
                    builder.AddProduct(p.sku.Id, ProductType.Consumable);
                }
                else
                {
                    builder.AddProduct(p.sku.Id, p.productType);
                }
            }
        }

        private ProductType GetIapType(string sku)
        {
            foreach (var item in Skus)
            {
                if (item.sku.Id.Equals(sku)) return item.productType;
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