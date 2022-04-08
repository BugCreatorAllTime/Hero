using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GoPlayStorage
{
	public UserData userData { get; set; }
	public PaymentModel paymentModel { get; set; }
	public TimeStampData timeStamp { get; set; }

	public GoPlayStorage()
	{
	}

	public GoPlayStorage(UserData uData, PaymentModel payment, TimeStampData time)
	{
		this.userData = uData;
		this.paymentModel = payment;
		this.timeStamp = time;
	}
}