using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoogleAnalytics : MonoBehaviour {
	public static readonly string EVENT = "event";
	public static readonly int VERSION = 1;
	public string propertyID;
	
	public static GoogleAnalytics instance;
	
	public string appName;
	public string appVersion;
	
	private string screenRes;
	private string clientID;
	private string url = "http://www.google-analytics.com/collect";
	void Awake()
	{
		if(instance)
			DestroyImmediate(gameObject);
		else
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
	}
	
	void Start() 
	{
	
		screenRes = Screen.width + "x" + Screen.height;
		
		clientID = SystemInfo.deviceUniqueIdentifier;
			
	}
	

	public void trackEvent(string cat, string act, string lab, long value) {
		TrackItem item = new TrackItem();
		item.ec = cat;
		item.ea = act;
		item.el = lab;
		item.ev = value.ToString();
		StartCoroutine(process(item));
	}


	private IEnumerator process(TrackItem item){
		yield return new WaitForEndOfFrame();
		WWWForm form = new WWWForm();
		// event

		form.AddField("ec",item.ec);

		form.AddField("ea",item.ea);
		form.AddField("el",item.el);
		form.AddField("ev",item.ev);
		// core config
		form.AddField("v",VERSION);
		form.AddField("tid",propertyID);
		form.AddField("cid",clientID);
		form.AddField("t",EVENT);
		form.AddField("sr",screenRes);
		// app config
		form.AddField("av",appVersion);
		form.AddField("an",appName);

		WWW request = new WWW(url,form);
		StartCoroutine(WaitForRequest(request));
	}

	IEnumerator WaitForRequest(WWW request)
	{
		yield return request;
		if(request.error == null)
		{
			if (request.responseHeaders.ContainsKey("STATUS"))
			{
				if (request.responseHeaders["STATUS"] == "HTTP/1.1 200 OK")	
				{
					Debug.Log ("GA Success " + clientID);
				}else{
					Debug.LogWarning(request.responseHeaders["STATUS"]);	
				}
			}else{
				Debug.LogWarning("Event failed to send to Google: " + clientID);	
			}
		}else{
			Debug.LogWarning(request.error.ToString());	
		}
	}    


	

	
}