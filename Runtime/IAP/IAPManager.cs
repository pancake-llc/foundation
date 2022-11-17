using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [AddComponentMenu("")]
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static event Action<string> OnPurchaseSucceedEvent;
        public static event Action<string> OnPurchaseFailedEvent;
        private static readonly Dictionary<string, Action> CompletedDict = new Dictionary<string, Action>();
        private static readonly Dictionary<string, Action> FaildDict = new Dictionary<string, Action>();

        private static IAPManager instance;
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        public static IAPManager Instance
        {
            get
            {
                if (instance == null) Init();

                return instance;
            }
        }

        public List<IAPData> Skus { get; set; } = new List<IAPData>();
        public InformationPurchaseResult ReceiptInfo { get; set; }
        public bool IsInitialized { get; set; }

        public static void Init()
        {
            if (instance != null) return;

            if (Application.isPlaying)
            {
                var obj = new GameObject("IAPManager") {hideFlags = HideFlags.HideAndDontSave};
                instance = obj.AddComponent<IAPManager>();
                instance.InitImpl(IAPSettings.SkusData);
                DontDestroyOnLoad(obj);
            }
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

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
#if !UNITY_EDITOR
            ReceiptInfo = purchaseEvent.purchasedProduct.hasReceipt ? GetIapInformationPurchase(purchaseEvent.purchasedProduct.receipt) : null;
#endif

            PurchaseVerified(purchaseEvent);
            return PurchaseProcessingResult.Complete;
        }

        private IAPData PurchaseProduct(IAPData product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _controller?.InitiatePurchase(product.sku.Id);
#endif
            return product;
        }

        public static IAPData Purchase(IAPData product) { return Instance.PurchaseProduct(product); }

        public static void RegisterCompletedEvent(string key, Action action)
        {
            foreach (var e in CompletedDict)
            {
                if (e.Key.Equals(key)) CompletedDict[key] = action;
            }
        }

        public static void RegisterFaildEvent(string key, Action action)
        {
            foreach (var e in FaildDict)
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

        private InformationPurchaseResult GetIapInformationPurchase(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

#if UNITY_ANDROID
            string jsonNode = JSON.Parse(json)["Payload"].Value;
            string jsonData = JSON.Parse(jsonNode)["json"].Value;
            long purchaseTime = JSON.Parse(jsonData)["purchaseTime"].AsLong;
            const string device = "ANDROID";
            string productId = JSON.Parse(jsonData)["productId"].Value;
            var iapType = GetIapType(productId);
            string transactionId = JSON.Parse(json)["TransactionID"].Value;
            int purchaseState = JSON.Parse(jsonData)["purchaseState"].AsInt;
            string purchaseToken = JSON.Parse(jsonData)["purchaseToken"].Value;
            string signature = JSON.Parse(jsonNode)["signature"].Value;
            return new InformationPurchaseResult(device,
                iapType.ToString(),
                transactionId,
                productId,
                purchaseState,
                purchaseTime,
                purchaseToken,
                signature,
                SystemInfo.deviceUniqueIdentifier);
#elif UNITY_IOS
            string jsonNode = JSON.Parse(json)["receipt"].Value;
            long purchaseTime = JSON.Parse(jsonNode)["receipt_creation_date_ms"].AsLong;
            const string device = "IOS";
            string productId = JSON.Parse(jsonNode)["in_app"][0]["product_id"].Value;
            var iapType = GetIapType(productId);
            string transactionId = JSON.Parse(jsonNode)["in_app"][0]["transaction_id"].Value;
            int purchaseState = JSON.Parse(json)["status"].AsInt;
            const string purchaseToken = "";
            const string signature = "";
            return new InformationPurchaseResult(device,
                iapType.ToString(),
                transactionId,
                productId,
                purchaseState,
                purchaseTime,
                purchaseToken,
                signature,
                SystemInfo.deviceUniqueIdentifier);
#else
            return null;
#endif
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
#if UNITY_ANDROID
                
#endif
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