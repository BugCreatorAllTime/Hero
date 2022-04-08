using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using OnePF;

public class GooglePlayPaymentService : IPaymentService
{
	private const string PAYMENT_KEY = "ggstore_payment";

	[Inject]
	public ShopService shopService {get;set;}

	[Inject] 
	public DataService dService {get; set;}

	[Inject]
	public ConfigManager configMgr {get;set;}
	
	private PaymentModel paymentModel;

	[PostConstruct]
	public void Init()
	{
		// Listen to all events for illustration purposes
		OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
		OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
		OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		OpenIABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
		OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
		OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
		OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
		
		string publicKey = configMgr.general.GoogleStore_PublicKey;
		Options options = new Options();
		options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
		options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
		options.checkInventory = false;
		options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
		options.prefferedStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
		options.storeKeys = new Dictionary<string, string> { {OpenIAB_Android.STORE_GOOGLE, publicKey} };
		OpenIAB.init(options);

		paymentModel = dService.Load<PaymentModel>(PAYMENT_KEY);
		if(paymentModel == null)
		{
			paymentModel = new PaymentModel();
			dService.Save(PAYMENT_KEY,paymentModel);
		}
	}
	
	
	public void BuyItem(string itemId, int quantity) 
	{
		string payload = GetPayLoad();
		paymentModel.AddPendingTransaction(payload,"NULL");
		OpenIAB.purchaseProduct(itemId,payload);
	}

	public void OnBuyItemResponse(string itemId, int quantity, string transactionId, PaymentErrorCode error)
	{
		shopService.OnBuyGemResponse(itemId,quantity,transactionId,error);
	}

	private void billingSupportedEvent()
	{
		Debug.Log("billingSupportedEvent");
	}

	private void billingNotSupportedEvent(string error)
	{
		Debug.Log("billingNotSupportedEvent: " + error);
	}

	private void queryInventorySucceededEvent(Inventory inventory)
	{
		Debug.Log("queryInventorySucceededEvent: " + inventory);
	}

	private void queryInventoryFailedEvent(string error)
	{
		Debug.Log("queryInventoryFailedEvent: " + error);
	}

	private void purchaseSucceededEvent(Purchase purchase)
	{
		OpenIAB.consumeProduct(purchase);
	}

	private void purchaseFailedEvent(int errorCode, string errorMessage)
	{
		Debug.Log("purchaseFailedEvent: " + errorMessage);
		OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseFailedEvent);
	}

	private void consumePurchaseSucceededEvent(Purchase purchase)
	{
		if(ValidatePayLoad(purchase.DeveloperPayload))
		{
			PaymentErrorCode err = paymentModel.CompleteTransaction(purchase.DeveloperPayload)?PaymentErrorCode.OK:PaymentErrorCode.PurchaseDuplicateTransactionId;
			OnBuyItemResponse(purchase.Sku,1,purchase.OrderId,err);
		}else
		{
			Debug.Log("Invalid payload");
			paymentModel.RemovePendingTransaction(purchase.DeveloperPayload);
			OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseErrorPayload);
		}
	}

	private void consumePurchaseFailedEvent(string error)
	{
		Debug.Log("consumePurchaseFailedEvent: " + error);
		OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseFailedEvent);
	}

	private string GetPayLoad()
	{
		return DateTime.UtcNow.Ticks.ToString();
	}

	private bool ValidatePayLoad(string payload)
	{
		return paymentModel.pendingTransactionIds.ContainsKey(payload);
	}

	public void CheckPayment()
	{
	}

}

