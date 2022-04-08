using UnityEngine;
using System.Collections;
using GoPlaySDK;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using LitJson;
using System;

public class GoPlayFlushData : Observer {

	[Inject]
	public DataService dataService { get; set; }

	[Inject]
	public PlaySignal playSignal { get; set; }

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public IPaymentService paymentService {get;set;}

	public bool isInGame = false;

	private List<string> listData = new List<string>();
	private bool isSave = false;

	[PostConstruct]
	public void PostConstruct()
	{
		dataService.PreSavingEvent += OnPreSaving;
		dataService.PreQuitingEvent += OnQuit;
		GoPlaySdk.Instance.OnSaveProgress += HandleOnSaveProgress;
		GoPlaySdk.Instance.OnGetProgress += HandleOnGetProgress;
	}

	public void AddSubjectOs()
	{
		config.UserData.AddObserver (this);
	}

	private void OnPreSaving(bool status)
	{
		if(status)
		{
			Save();
		}
	}

	private void OnQuit()
	{
		Save ();
	}

	public void Load()
	{
		GoPlaySdk.Instance.GetProgress (true);
	}

	public void Save()
	{
		if(isInGame)
		{
			dataService.SaveTime();
			string data = dataService.GetDataGoPlay();
			listData.Add(data);
			SaveData();
		}
	}

	private void SaveData()
	{
		if(listData.Count > 0 && !isSave)
		{
			isSave = true;
			GoPlaySdk.Instance.SaveProgress(dataService.GetDataGoPlay());
		}
	}

	private void HandleOnSaveProgress(IResult result)
	{
		isSave = false;
		if(listData.Count > 0)
			listData.RemoveAt (0);
		SaveData ();
	}

	private void HandleOnGetProgress(IResult result)
	{
		GoPlaySDK.GetProgressResult response = result as GoPlaySDK.GetProgressResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			Debug.Log(response.Message);
		}
		try{
			string data = Regex.Unescape(response.Data);
			Debug.Log(data);
			GoPlayStorage goPlayStorage = JsonMapper.ToObject<GoPlayStorage>(data);
			if(goPlayStorage.timeStamp == null || goPlayStorage.timeStamp.timeStampData == null)
			{
				GoPlaySdk.Instance.SaveProgress(dataService.GetDataGoPlay());
			} else {
				string curData = dataService.GetDataGoPlay();
				GoPlayStorage curStorage = JsonMapper.ToObject<GoPlayStorage>(curData);
				long curTime = GetTime(curStorage.timeStamp);
				TimeStampData remoteStorageTimeData = goPlayStorage.timeStamp;
				long remoteTime = GetTime(remoteStorageTimeData);
				if(remoteTime < curTime && curStorage.userData != null && curStorage.userData.Name != goPlayStorage.userData.Name)
				{
					Debug.Log ("local");
					GoPlaySdk.Instance.SaveProgress(dataService.GetDataGoPlay());
				} else {
					Debug.Log("remote");
					config.UserData = goPlayStorage.userData;
					config.UserData.SetConfig(config);
					dataService.Save(DataService.USER_KEY,config.UserData);
				}
			}
		} catch(Exception e){

		}

		playSignal.Dispatch ();
	}

	private long GetTime(TimeStampData localStorageTimeData)
	{
		if(localStorageTimeData != null)
			try{
				return long.Parse(localStorageTimeData.timeStampData);
			} catch(Exception e){
				return 0;
			}
		return 0;
	}

	public void ResetDataClient()
	{
		Save ();
		PaymentModel paymentModel = new PaymentModel ();
		dataService.Save(GoPlayPaymentService.GOPLAY_PAYMENT_KEY, paymentModel);
		UserData user = new UserData ();
		user.SetConfig(config);
		user.GetFlag();
		user.Init();
		config.UserData = user;
		((GoPlayPaymentService)paymentService).SetPaymentModel (paymentModel);
		Debug.Log ("reset data");
		dataService.Save(DataService.USER_KEY, user);
		dataService.Save(DataService.TIME_STAMP_KEY, new TimeStampData());
		dataService.Flush ();
		Debug.Log (config.UserData.currentStepTownTutorial);
	}

	public void Update()
	{
		Save ();
	}
}
