
using System;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using LitJson;
using strange.extensions.context.api;
using PlayerPrefs = PreviewLabs.PlayerPrefs;
using System.Collections.Generic;

public class DataService
{
	public static readonly String USER_KEY = "user_key";
	public static readonly String TIME_STAMP_KEY = "time_stamp_key";

	public delegate void PreSaving(bool pauseStatus);
	public delegate void PreQuiting();

	public event PreSaving PreSavingEvent;
	public event PreQuiting PreQuitingEvent;

	private TimeStampData timeStampData;

	[Inject]
	public ApplicationDispatcherService appService {get;set;}

	[Inject]
	public ConfigManager configMgr {get; set;}

	[Inject]
	public IPaymentService paymentService { get; set;}

	private Dictionary<string,object> storage = new Dictionary<string, object>();
	public DataService ()
	{
	}

	[PostConstruct]
	public void Init()
	{
		appService.dispatcher.OnApplicationPauseHandler += OnPause;
		appService.dispatcher.OnApplicationQuitHandler += OnQuit;
		appService.dispatcher.OnAwakeHandler += Awake;
		PlayerPrefs.EnableEncryption(true);

	}

	public void SaveTime()
	{
//		timeStampData = Load<TimeStampData>(TIME_STAMP_KEY);
//		if(timeStampData == null)
//		{
			timeStampData = new TimeStampData();
//		}
		timeStampData.Write();
		Save(TIME_STAMP_KEY, timeStampData);
	}

	public void OnQuit()
	{
		if (PreQuitingEvent != null)
		{
			PreQuitingEvent();
		}
		OnPause(true);
	}

	public void OnPause(bool pauseStatus)
	{
		if (PreSavingEvent != null)
		{
			PreSavingEvent(pauseStatus);
		}
		Flush();
	}

	private void Awake()
	{

	}

	public void Save (string prefKey, object instance)
	{
		if(instance != null)
		{
			storage[prefKey] = instance;
		}
	}
	
	public T Load<T>(string preKey)
	{
		if(storage.ContainsKey(preKey))
		{
			return (T)storage[preKey];
		}else if (PlayerPrefs.HasKey(preKey))
		{	
			string tmp = PlayerPrefs.GetString(preKey);
			T deserializedObject = JsonMapper.ToObject<T>(tmp);
			Save(preKey,deserializedObject);
			return deserializedObject;
		}else{
			T ret = default(T);
			Save(preKey,ret);
			return ret;
		}
	}

	public void Flush()
	{
		if(storage.Count == 0)
			return ;
		foreach(KeyValuePair<string,object> pair in storage)
		{
			string tmp = JsonMapper.ToJson(pair.Value);
			PlayerPrefs.SetString (pair.Key, tmp);
		}
		PlayerPrefs.Flush();//Make sure data is saved
	}

	public string GetDataGoPlay()
	{
		PaymentModel paymentModel = ((GoPlayPaymentService) paymentService).GetPaymentModel();
		GoPlayStorage goPlayStorage = new GoPlayStorage(configMgr.UserData, paymentModel, timeStampData);
		string data = JsonMapper.ToJson(goPlayStorage);
		Debug.Log(data);
		return data;
	}
}


