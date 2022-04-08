
using System;
using System.Collections;

public interface IPaymentService
{
	void BuyItem(string itemId, int quantity);
	void OnBuyItemResponse(string itemId, int quantity, string transactionId, PaymentErrorCode error);
	void CheckPayment();
}


