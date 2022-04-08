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
using UnityEngine;

public class IOSNotifService : NotificationService
{
	public IOSNotifService ()
	{
	}

	public override void SendNotification (string name, string title, string body, int delay)
	{
#if UNITY_IOS
		NotificationServices.ClearLocalNotifications();
		LocalNotification notif = new LocalNotification();
		notif.alertBody = body;
		notif.fireDate = DateTime.Now.AddSeconds(delay);
		NotificationServices.ScheduleLocalNotification(notif);
#endif
	}
}

