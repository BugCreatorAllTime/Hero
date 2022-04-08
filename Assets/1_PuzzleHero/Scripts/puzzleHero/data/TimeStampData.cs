using UnityEngine;
using System.Collections;
using System;

public class TimeStampData {

	public string timeStampData;

	public void Write()
	{
		timeStampData = DateTime.UtcNow.Ticks.ToString();
	}

}
