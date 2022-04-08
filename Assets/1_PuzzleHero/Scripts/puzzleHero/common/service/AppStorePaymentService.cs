using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using common.service;
using LitJson;
#if UNITY_IPHONE
public class AppStorePaymentService : IPaymentService
{


//	private const string PAYMENT_KEY = "appstore_payment";
//	[Inject]
//	public ShopService shopService {get;set;}
//
//	[Inject]
//	public ConfigManager configMfg {get; set;}
//
//	[Inject]
//	public ServerService ss {get; set;}
//
//	[Inject] 
//	public DataService dService {get; set;}
//
//	private PaymentModel paymentModel;
//	[PostConstruct]
//	public void Init()
//	{
//		StoreKitManager.autoConfirmTransactions = false;
//		StoreKitManager.transactionUpdatedEvent += transactionUpdatedEvent;
//		StoreKitManager.productPurchaseAwaitingConfirmationEvent += productPurchaseAwaitingConfirmationEvent;
//		StoreKitManager.purchaseSuccessfulEvent += purchaseSuccessfulEvent;
//		StoreKitManager.purchaseCancelledEvent += purchaseCancelledEvent;
//		StoreKitManager.purchaseFailedEvent += purchaseFailedEvent;
//		StoreKitManager.productListReceivedEvent += productListReceivedEvent;
//		StoreKitManager.productListRequestFailedEvent += productListRequestFailedEvent;
//		StoreKitManager.restoreTransactionsFailedEvent += restoreTransactionsFailedEvent;
//		StoreKitManager.restoreTransactionsFinishedEvent += restoreTransactionsFinishedEvent;
//		StoreKitManager.paymentQueueUpdatedDownloadsEvent += paymentQueueUpdatedDownloadsEvent;
//
//		paymentModel = dService.Load<PaymentModel>(PAYMENT_KEY);
//		if(paymentModel == null)
//		{
//			paymentModel = new PaymentModel();
//			dService.Save(PAYMENT_KEY,paymentModel);
//		}
//		// dev only
////		StoreKitBinding.forceFinishPendingTransactions();
////		StoreKitBinding.cancelDownloads();
////		StoreKitBinding.enableHighDetailLogs(true);
//
//		string[] arr = new string[configMfg.shopCfg.gem.Count];
//		int i = 0;
//		foreach (KeyValuePair<string,ShopItemCfg> pair in configMfg.shopCfg.gem)
//		{
//			arr[i] = pair.Value.StoreId;
//			++i;
//		}
//		StoreKitBinding.requestProductData(arr);
//	}
//
//
//	public void BuyItem(string itemId, int quantity) 
//	{
//		StoreKitBinding.purchaseProduct(itemId,quantity);
//		AutoCompletePendingTransactions();
//	}
//
//	public void OnBuyItemResponse(string itemId, int quantity, string transactionId, PaymentErrorCode error)
//	{
//		Debug.Log("AppStore::OnBuyItemResp");
//		shopService.OnBuyGemResponse(itemId,quantity,transactionId,error);
//	}
//
//	
//	void transactionUpdatedEvent( StoreKitTransaction transaction )
//	{
//		Debug.Log( "transactionUpdatedEvent: " + transaction );
//	}
//
//	void productListReceivedEvent( List<StoreKitProduct> productList )
//	{
//		AutoCompletePendingTransactions();
//		Debug.Log( "productListReceivedEvent. total products received: " + productList.Count );
//		
//		// print the products to the console
//		foreach( StoreKitProduct product in productList )
//			Debug.Log( product.ToString() + "\n" );
//	}
//	
//	void productListRequestFailedEvent( string error )
//	{
//		Debug.Log( "productListRequestFailedEvent: " + error );
//	}
//
//	void purchaseFailedEvent( string error )
//	{
//		Debug.Log( "purchaseFailedEvent: " + error );
//		OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseFailedEvent);
//	}
//	
//	
//	void purchaseCancelledEvent( string error )
//	{
//		Debug.Log( "purchaseCancelledEvent: " + error );
//		OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseCancelledEvent);
//	}
//	
//	
//	void productPurchaseAwaitingConfirmationEvent( StoreKitTransaction transaction )
//	{
//		Debug.Log( "productPurchaseAwaitingConfirmationEvent: " + transaction );
//		StoreKitBinding.finishPendingTransaction(transaction.transactionIdentifier);
//	}
//
//	void OnValidateTransaction(object request, string data, string error)
//	{
//		if(error == null)
//		{
//			PaymentRequest req = request as PaymentRequest;
//			PaymentResponse resp = JsonMapper.ToObject<PaymentResponse>(data);
//			PaymentErrorCode err;
//			if(resp.status == 0)
//			{
//				err = paymentModel.CompleteTransaction(resp.receipt.transaction_id)?PaymentErrorCode.OK:PaymentErrorCode.PurchaseDuplicateTransactionId;
//				OnBuyItemResponse(resp.receipt.product_id,1,resp.receipt.transaction_id,err);
//			}else
//			{
//				paymentModel.RemovePendingTransaction(req.original_tranId);
//			}
//		}else
//		{
//			Logger.Info("Network error " + error);
//			OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseNetworkError);
//		}
//
//	}
//	
//	void purchaseSuccessfulEvent( StoreKitTransaction transaction )
//	{
//		if(configMfg.general.AppStore_EnableValidate)
//		{
//			if(paymentModel.AddPendingTransaction(transaction.transactionIdentifier,transaction.base64EncodedTransactionReceipt))
//			{
//				ss.ValidateTransaction(transaction.base64EncodedTransactionReceipt, transaction.transactionIdentifier, OnValidateTransaction);
//			}else
//			{
//				OnBuyItemResponse("NULL",0,"NULL",PaymentErrorCode.PurchaseDuplicateTransactionId);
//			}
//		}else
//		{
//			OnBuyItemResponse(transaction.productIdentifier,1,transaction.transactionIdentifier,PaymentErrorCode.OK);
//		}
//
//	}
//	
//	void restoreTransactionsFailedEvent( string error )
//	{
//		Debug.Log( "restoreTransactionsFailedEvent: " + error );
//	}
//	
//	
//	void restoreTransactionsFinishedEvent()
//	{
//		Debug.Log( "restoreTransactionsFinished" );
//	}
//	
//	
//	void paymentQueueUpdatedDownloadsEvent( List<StoreKitDownload> downloads )
//	{
//		Debug.Log( "paymentQueueUpdatedDownloadsEvent: " );
//		foreach( var dl in downloads )
//			Debug.Log( dl );
//	}
//
//	private void AutoCompletePendingTransactions()
//	{
//		foreach(KeyValuePair<string,string> pair in paymentModel.pendingTransactionIds)
//		{
//			ss.ValidateTransaction(pair.Value, pair.Key, OnValidateTransaction);
//		}
//	}
//
//	public void CheckPayment()
//	{
//	}
}
#endif

