// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;

public class PaymentResponse
{
	public int status = -1;
	public Receipt receipt;
}


public class Receipt
{
	public string original_purchase_date_pst;
	public string purchase_date_ms;
	public string unique_identifier;
	public string original_transaction_id;
	public string transaction_id;
	public string original_purchase_date_ms;
	public string product_id;
}
