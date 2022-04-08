using UnityEngine;
using System.Collections;
using System;
using GoPlaySDK;
using System.Linq;
using LitJson;

public class GoPlayPaymentService : IPaymentService {

	[Inject]
	public ShopService shopService {get;set;}
	
	[Inject] 
	public DataService dService {get; set;}
	
	[Inject]
	public ConfigManager configMgr {get;set;}

	[Inject]
	public ApplicationDispatcherService appService {get;set;}

	private PaymentModel paymentModel;
	private bool isPayment = false;

	public const string GOPLAY_PAYMENT_KEY = "goplaystore_payment";

	[PostConstruct]
	public void Init()
	{
		GoPlaySdk.Instance.OnGetUnFullFilledExchanges += HandleOnGetUnFullFilledExchange;
		GoPlaySdk.Instance.OnFullFillExchange += HandleOnFullFillExchange;
		GoPlaySdk.Instance.OnRejectExchange += HandleOnRejectExchange;
		appService.dispatcher.OnApplicationPauseHandler += OnPause;
		GoPlaySdk.Instance.UseLiveServer = true;
		paymentModel = dService.Load<PaymentModel>(GOPLAY_PAYMENT_KEY);
		if(paymentModel == null)
		{
			paymentModel = new PaymentModel();
			dService.Save(GOPLAY_PAYMENT_KEY,paymentModel);
		}
	}

	public void BuyItem(string itemId, int quantity) 
	{
		Application.OpenURL("http://goplay.la");
		isPayment = true;
	}

	public void OnBuyItemResponse(string itemId, int quantity, string transactionId, PaymentErrorCode error)
	{
		if(itemId.Equals("com.nfury.puzzlehero2.gem.1a"))
		{
			itemId = configMgr.shopCfg.gem.ElementAt(0).Value.StoreId;
		}
		string payload = GetPayLoad();
		paymentModel.AddPendingTransaction(payload,"NULL");
		shopService.OnBuyGemResponse(itemId,quantity,transactionId,error);
	}

	public void CheckPayment()
	{
		GoPlaySdk.Instance.GetUnFullFilledExchanges ();
	}

	private string GetPayLoad()
	{
		return DateTime.UtcNow.Ticks.ToString();
	}

	public void OnPause(bool pauseStatus)
	{
		if(!pauseStatus && isPayment)
		{
			isPayment = false;
			CheckPayment();
		}
	}

	private void HandleOnGetUnFullFilledExchange(IResult result)
	{
		GoPlaySDK.GetUnFullFilledExchangesResult response = result as GoPlaySDK.GetUnFullFilledExchangesResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			Debug.Log(response.Message);
		} else {
			for(int i=0;i<response.Exchanges.Count;i++){
//				Debug.Log(response.Exchanges.ElementAt(i).ExchangeOptionIdentifier);
				if(response.Exchanges.ElementAt(i).ExchangeOptionIdentifier.Equals("com.nfury.puzzlehero2.gem.1a"))
				{
					GoPlaySdk.Instance.FullFillExchange(response.Exchanges.ElementAt(i).TransactionId.ToString());
				}
				for(int j = 0; j < configMgr.shopCfg.gem.Count; j++)
				{
					if(configMgr.shopCfg.gem.ElementAt(j).Value.StoreId.Equals(response.Exchanges.ElementAt(i).ExchangeOptionIdentifier))
					{
						if(!response.Exchanges.ElementAt(i).IsFree && ((int)response.Exchanges.ElementAt(i).GoPlayTokenValue*-1) == 
						   configMgr.shopCfg.gem.ElementAt(j).Value.RM)
						{
							GoPlaySdk.Instance.FullFillExchange(response.Exchanges.ElementAt(i).TransactionId.ToString());
						} else {
							GoPlaySdk.Instance.RejectExchange(response.Exchanges.ElementAt(i).TransactionId.ToString());
						}
					}
				}
			}
		}
	}

	private void HandleOnFullFillExchange(IResult result)
	{

		GoPlaySDK.FullFillExchangeResult response = result as GoPlaySDK.FullFillExchangeResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			Debug.Log(response.Message);
		} else {
			OnBuyItemResponse(response.Exchange.ExchangeOptionIdentifier,1,
			                  response.Exchange.TransactionId.ToString(), PaymentErrorCode.OK);
		}
	}

	private void HandleOnRejectExchange(IResult result)
	{
		GoPlaySDK.RejectExchangeResult response = result as GoPlaySDK.RejectExchangeResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			Debug.Log(response.Message);
		} else {
			OnBuyItemResponse(response.Exchange.ExchangeOptionIdentifier,1,
			                  response.Exchange.TransactionId.ToString(), PaymentErrorCode.PurchaseErrorPayload);
		}
	}

	public void SetPaymentModel(PaymentModel paymentModel)
	{
		this.paymentModel = paymentModel;
	}

	public PaymentModel GetPaymentModel()
	{
		return paymentModel;
	}
}
