using System;
using strange.extensions.pool.api;
using UnityEngine;
using strange.examples.strangerocks;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ItemChestEffectManager{

	[Inject(PrefabWorldMap.itemFx)]
	public IPool<GameObject> itemFxPool { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public ChestService chestService { get; set;}

	[Inject]
	public ConfigManager config {get;set;}

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject(Prefabs.fx)]
	public IPool<GameObject> spinePool { get; set; }

	[Inject]
	public ItemService itemService {get; set;}

	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public GA ga { get; set; }

	private GameObject tabPanel;
	private GameObject home;
	private GameObject blackSmith;
	private GameObject fxPanel;
	private GameObject fx = null;

	private bool success = false;
	private bool upgradSuccess = false;
	private bool compleOpenChest = true;
	private bool isOpen = false;
	private ItemBaseData itemOpenData;
	ItemContentManager itemContent;

	[PostConstruct]
	public void PostConstruct()
	{

		fxPanel = GameObject.Find ("fxPanel");
		tabPanel = GameObject.Find ("tabPanel");
	}

	public void SetHomeLink()
	{
		home = GameObject.Find ("Home");
	}

	public void SetBlackSmithLink()
	{
		blackSmith = GameObject.Find ("BlackSmith");
	}

	public void OpenChest(ItemContentManager iContent)
	{
		if(compleOpenChest)
		{
			itemOpenData = chestService.OpenChest (config.UserData, iContent.indexOfInventory);
			soundMng.PlaySound (SoundName.OPEN_CHEST);
			compleOpenChest = false;
			itemContent = iContent;
			GameObject fx = CreatePoolGameObject (itemFxPool);
			fx.transform.parent = fxPanel.transform;
			fx.transform.localScale = Vector3.one;
			ItemEfContent eContent = fx.GetComponent<ItemEfContent>();
			eContent.result.gameObject.SetActive (false);
			eContent.item.gameObject.SetActive (false);
			eContent.chest.gameObject.SetActive (true);
			eContent.item.spriteName = "itemtop";
			eContent.chest.width = 96;
			eContent.chest.height = 96;
			eContent.item.width = 90;
			eContent.item.height = 90;
			eContent.theme.width = 86;
			eContent.theme.height = 86;
			eContent.icon.width = 75;
			eContent.icon.height = 75;
			eContent.chest.transform.localScale = Vector3.one;
			eContent.chest.spriteName = iContent.icon.spriteName;
			eContent.bg.gameObject.SetActive (true);
			eContent.bg.alpha = 0;
			eContent.chest.transform.position = new Vector3 (-0.41f,iContent.transform.position.y,0);
			AddEventToButton (eContent.returnScreen,"CompleteOpenChest", eContent.gameObject);
			routineRunner.StartCoroutine(MoveToCenter(eContent, iContent));
		}
	}

	private IEnumerator MoveToCenter(ItemEfContent eContent, ItemContentManager iContent)
	{
		if(!compleOpenChest)
		{
			TweenAlpha tweenAlpha;
			tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.bg.gameObject, 0.5f);
			tweenAlpha.from = 0;
			tweenAlpha.to = 1;
			if(compleOpenChest) CompleOpenChest(eContent.gameObject);
		}
		yield return new WaitForSeconds(0.5f);
		if(!compleOpenChest)
		{
			eContent.bg.gameObject.SetActive (false);
			home.SetActive (false);
			tabPanel.SetActive (false);
			if(compleOpenChest) CompleOpenChest(eContent.gameObject);
		}
		yield return new WaitForSeconds(0.5f);
		if(!compleOpenChest)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("localPosition", new Vector3 (0, 0, 0)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(CreateEffectOpen), eContent, iContent);
			HOTween.To(eContent.chest.transform, 0.5f, parms);
			
			TweenParms parms2 = new TweenParms();
			parms2.Prop ("localScale", new Vector3 (1.5f, 1.5f, 1.5f)).Ease (EaseType.EaseOutSine);
			HOTween.To(eContent.chest.transform, 0.5f, parms2);
			if(compleOpenChest) CompleOpenChest(eContent.gameObject);
		}
	}

	GameObject CreatePoolGameObject(IPool<GameObject> poolObject)
	{
		GameObject contentObject = poolObject.GetInstance();
		contentObject.SetActive(true);
		return contentObject;
	}
	
	void ReturnInstance(GameObject contentObject, IPool<GameObject> contentPool)
	{
		contentObject.SetActive (false);
		contentPool.ReturnInstance(contentObject);
	}

	void CreateEffectOpen(TweenEvent tweenEvent)
	{
		if(!compleOpenChest)
		{
			ItemEfContent eContent = ((ItemEfContent)tweenEvent.parms [0]).GetComponent<ItemEfContent>();
			ItemContentManager iContent = ((ItemContentManager)tweenEvent.parms [1]).GetComponent<ItemContentManager>();
			fx = CreateEffect ("Animation/Fx/OpenChest/Open chest fx.ske","Active",false);
			fx.transform.localPosition = new Vector2(-967, -2856);
			routineRunner.StartCoroutine(SetActiceFx(false,1f,eContent.gameObject, iContent));
			routineRunner.StartCoroutine(VibrationObject(1f,eContent.chest.gameObject,eContent.chest.transform.localPosition.x));
			routineRunner.StartCoroutine(SetActiceFx(true,1f,eContent.gameObject, iContent));
			if(compleOpenChest) CompleOpenChest(eContent.gameObject);
		}
	}

	void CreateEffectShowItem(ItemEfContent eContent, ItemContentManager iContent)
	{
		fx = CreateEffect ("Animation/Fx/GlowFx/glow item fx.ske","Idle",true);
		fx.layer = 8;
		fx.name = "fxShow";
		fx.transform.localScale = Vector3.one;
		fx.transform.parent = eContent.item.transform;
		fx.transform.localPosition = new Vector3(0, 0, 1);
		eContent.item.gameObject.SetActive (true);
		eContent.item.transform.localPosition = new Vector3 (0,0,-2);
		ItemBaseData item = itemOpenData;
		if(item!= null && !isOpen)
		{
			isOpen = true;
			routineRunner.StartCoroutine (ResetOpen());
			eContent.icon.spriteName = config.ItemCfg.GetItemByItemId(item.Id).Icon.ToString();
			eContent.item.transform.localScale = Vector3.zero;
			iContent.item = item;
			switch(config.ItemCfg.GetItemByItemId(item.Id).Color)
			{
			case 0:
				eContent.theme.spriteName = config.text.NormalItem;
				break;
			case 1:
				eContent.theme.spriteName = config.text.RareItem;
				break;
			case 2:
				eContent.theme.spriteName = config.text.LegendItem;
				break;
			}
			routineRunner.StartCoroutine(MoveUp(eContent, iContent));
		}
	}

	IEnumerator ResetOpen()
	{
		yield return  new WaitForSeconds(2f);
		isOpen = false;
	}

	private IEnumerator MoveUp(ItemEfContent eContent,  ItemContentManager iContent)
	{
		yield return new WaitForSeconds(0.5f);
		if(!compleOpenChest)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("localScale", new Vector3 (1, 1, 1)).Ease (EaseType.EaseOutSine);
			HOTween.To(eContent.item.transform, 1f, parms);
			
			TweenParms parms2 = new TweenParms();
			parms2.Prop ("localPosition", new Vector3 (0, 80, -2)).Ease (EaseType.EaseOutSine);
			HOTween.To(eContent.item.transform, 1f, parms2);
			yield return new WaitForSeconds(1f);
			SetText (eContent, iContent);
			if(compleOpenChest) CompleOpenChest(eContent.gameObject);
		}
	}

	void SetText(ItemEfContent eContent, ItemContentManager iContent)
	{
		eContent.result.gameObject.SetActive (true);
		eContent.result.transform.localPosition = new Vector3 (10,250,0);
		eContent.result.transform.localScale = new Vector3 (3,3,3);
		eContent.result.text = config.text.TextOpenChest + " "+ config.ItemCfg.GetItemByItemId (iContent.item.Id).Name;
		eContent.result.color = new Color32 (0,139,24,255);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1, 1, 1)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnHome), eContent);
		HOTween.To(eContent.result.transform, 0.25f, parms);
		
	}

	public void CompleOpenChest(GameObject go)
	{
		ItemEfContent eContent = go.GetComponent<ItemEfContent>();
		eContent.returnScreen.onClick.Clear ();
		eContent.bg.gameObject.SetActive (false);
		home.SetActive (false);
		tabPanel.SetActive (false);
		eContent.chest.transform.localPosition = Vector3.zero;
		eContent.chest.transform.localScale = Vector3.one * 1.5f;
		if (fx != null)
			fx.SetActive (false);
		CreateEffectShowItem (eContent, itemContent);
		eContent.chest.gameObject.SetActive (true);
		UISprite sprite = eContent.chest;
		sprite.spriteName = sprite.spriteName+"Open";
		sprite.width = 111;
		sprite.height = 99;
		sprite.alpha = 1;
		routineRunner.StartCoroutine(ResetAlphaChest(eContent.chest));

		eContent.item.transform.localScale = Vector3.one;
		eContent.item.transform.localPosition = new Vector3 (0, 80, -2);
//		SetText (eContent, itemContent);
	}

	IEnumerator ResetAlphaChest(UISprite image)
	{
		yield return new WaitForSeconds(0.5f);
		if(compleOpenChest) image.alpha = 1;
	}

	void ReturnHome(TweenEvent tweenEvent)
	{
		compleOpenChest = true;
		ItemEfContent eContent = (ItemEfContent)tweenEvent.parms [0];
		eContent.returnScreen.onClick.Clear ();
		AddEventToButton (eContent.returnScreen,"HideOpenChest", eContent.gameObject);
	}

	IEnumerator VibrationObject(float duration, GameObject go, float x)
	{
		float passedTime = 0;
		while (passedTime < duration)
		{
			if(!compleOpenChest)
			{
				passedTime += Time.deltaTime;
				int positionChange = Random.Range(-10,20);
				go.transform.localPosition = new Vector3(x+positionChange,go.transform.localPosition.y,go.transform.localPosition.z);
				if(compleOpenChest) CompleOpenChest(go);
			}

			yield return new WaitForEndOfFrame();
		}
	}
	
	IEnumerator SetActiceFx(bool active, float duration, GameObject go, ItemContentManager iContent)
	{
		TweenAlpha tweenAlpha;
		ItemEfContent eContent = go.GetComponent<ItemEfContent>();
		if(!compleOpenChest)
		{
			if(!active)
			{
				tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.chest.gameObject, 1);
				tweenAlpha.from = tweenAlpha.alpha;
				tweenAlpha.to = 0;
			}

		}
		yield return new WaitForSeconds(duration);
		if(!compleOpenChest)
		{
			if(active)
			{
				eContent.chest.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
				tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.chest.gameObject, 0.3f);
				tweenAlpha.from = tweenAlpha.alpha;
				tweenAlpha.to = 1;
				UISprite sprite = eContent.chest;
				sprite.spriteName = sprite.spriteName+"Open";
				sprite.width = 111;
				sprite.height = 99;
			}
			if(compleOpenChest) CompleOpenChest(go);
		}
		yield return new WaitForSeconds(0.3f);
		if(!compleOpenChest)
		{
			if(active)
			{
				ReturnInstance (fx, spinePool);
				CreateEffectShowItem (eContent, iContent);
			}
			if(compleOpenChest) CompleOpenChest(go);
		}
	}

	GameObject CreateEffect(string pathToAsset, string animationName, bool loop)
	{
		GameObject fx = spinePool.GetInstance();
		fx.transform.parent = fxPanel.transform;
		fx.layer = 0;
		fx.transform.localScale = new Vector3(480,480,480);
		fx.SetActive (true);
		SkeletonDataAsset skele = assetMgr.GetAssetSync<SkeletonDataAsset>(pathToAsset);
		SkeletonAnimation skeletonAnimation = fx.GetComponent<SkeletonAnimation>();
		skeletonAnimation.skeletonDataAsset = skele;
		skeletonAnimation.skeletonDataAsset.Reset();
		skeletonAnimation.Reset();
		skeletonAnimation.state.SetAnimation(0, animationName, loop);
		fx.transform.localPosition = new Vector2(-37166, 45391);
		return fx;
	}

	public void UpgradeItem(UpdateContent uContent, List<int> ilist, int index)
	{
		success = false;
		upgradSuccess = false;
		LoadInfoBlacksmith smith = blackSmith.GetComponent<LoadInfoBlacksmith>();
		int itemId = config.UserData.Inventory.ListItemData[index].Id;
		bool check = false;
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.END ||
		   config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.END - 1)
			check = true;
		switch (itemService.ProcessUpgradeItem(config.UserData, ilist, index, check))
		{
		case ErrorCode.OK:
			success = true;
			upgradSuccess = true;
			break;
		case ErrorCode.ITEM_UPGRADE_FAIL:
			success = true;
			upgradSuccess = false;
			break;
		case ErrorCode.NOT_ENOUGH_GOLD:
			smith.loadTab.ShowNoticePurchaseGold();
			break;
		default:
			smith.loadTab.ShowNotice(itemService.ProcessUpgradeItem(config.UserData, ilist, index).ToString());
			break;
		}
		if(success)
		{
			ga.TrackEquipmentUpgrade(itemId);
			GameObject fx = CreatePoolGameObject (itemFxPool);
			fx.transform.parent = fxPanel.transform;
			fx.transform.localScale = Vector3.one;
			ItemEfContent eContent = fx.GetComponent<ItemEfContent>();
			eContent.result.gameObject.SetActive (false);
			eContent.item.gameObject.SetActive (true);
			eContent.chest.gameObject.SetActive (false);
			eContent.item.spriteName = uContent.contentItem.spriteName;
			eContent.item.width = 110;
			eContent.item.height = 110;
			eContent.theme.width = 96;
			eContent.theme.height = 96;
			eContent.icon.width = 90;
			eContent.icon.height = 90;
			eContent.bg.gameObject.SetActive (true);
			eContent.bg.alpha = 0;
			Vector3 pos = uContent.contentItem.transform.position;
			eContent.item.transform.position = new Vector3 (pos.x,pos.y,0);
			eContent.theme.spriteName = uContent.theme.spriteName;
			eContent.icon.spriteName = uContent.icon.spriteName;
			routineRunner.StartCoroutine(MoveToCenter(eContent, ilist, index));
		}
	}

	private IEnumerator MoveToCenter(ItemEfContent eContent, List<int> ilist, int index)
	{
		TweenAlpha tweenAlpha;
		tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.bg.gameObject, 0.5f);
		tweenAlpha.from = 0;
		tweenAlpha.to = 1;
		yield return new WaitForSeconds(0.5f);
		eContent.bg.gameObject.SetActive (false);
		blackSmith.SetActive (false);
		tabPanel.SetActive (false);
		yield return new WaitForSeconds(0.5f);
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (0, 0, 0)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(CreateEffectUpgrade), eContent, ilist, index);
		HOTween.To(eContent.item.transform, 0.5f, parms);	
	}

	void CreateEffectUpgrade(TweenEvent tweenEvent)
	{
		ItemEfContent eContent = ((ItemEfContent)tweenEvent.parms [0]).GetComponent<ItemEfContent>();
		List<int> ilist = (List<int>)tweenEvent.parms [1];
		int index = (int)tweenEvent.parms [2];
		fx = CreateEffect ("Animation/Fx/UpgradeItem/upgrade fx.ske","Active",false);
		soundMng.PlaySound (SoundName.UPGRADE_ITEM);
		fx.transform.localPosition = new Vector3(0, -435,-3);
		fx.layer = 8;
		fx.transform.localScale = Vector3.one;
		routineRunner.StartCoroutine(SetActiceFx(false,2f,eContent.gameObject,ilist,index));
		routineRunner.StartCoroutine(SetActiceFx(true,2.4f,eContent.gameObject,ilist,index));
	}

	IEnumerator SetActiceFx(bool active, float duration, GameObject go, List<int> ilist, int index)
	{

		TweenAlpha tweenAlpha;
		ItemEfContent eContent = go.GetComponent<ItemEfContent>();
		if(!active)
		{
			yield return new WaitForSeconds(duration);
			tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.item.gameObject, 0.1f);
			tweenAlpha.from = tweenAlpha.alpha;
			tweenAlpha.to = 0;
		}

		if(active)
		{
			yield return new WaitForSeconds(duration);
			tweenAlpha = UITweener.Begin<TweenAlpha>(eContent.item.gameObject, 0.1f);
			tweenAlpha.from = tweenAlpha.alpha;
			tweenAlpha.to = 1;
		}
		if(active)
		{
			ReturnInstance (fx, spinePool);
			LoadInfoBlacksmith smith = blackSmith.GetComponent<LoadInfoBlacksmith>();
			if(upgradSuccess)
			{
				smith.AfterUpgrade(smith.smith.item);
				CreateEffectUpdateSuccess(eContent);
				SetText(true, eContent);
			} else {
				smith.AfterUpgrade(smith.smith.item);
				SetText(false, eContent);
			}
		}
	}

	void SetText(bool success, ItemEfContent eContent)
	{
		eContent.result.gameObject.SetActive (true);
		eContent.result.transform.localPosition = new Vector3 (10,130,0);
		eContent.result.transform.localScale = new Vector3 (3,3,3);
		if(success)
		{
			eContent.result.text = "successful";
			eContent.result.color = new Color32 (0,139,24,255);
//			soundMng.PlaySound (SoundName.UPGRADE_SUCCESS);
		} else {
			eContent.result.text = "failed";
			eContent.result.color = new Color32 (248,0,0,255);
//			soundMng.PlaySound (SoundName.UPGRADE_FAILED);
		}
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1, 1, 1)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnBlackSmith), eContent, success);
		HOTween.To(eContent.result.transform, 0.25f, parms);

	}

	void ReturnBlackSmith(TweenEvent tweenEvent)
	{
		ItemEfContent eContent = (ItemEfContent)tweenEvent.parms [0];
		bool success = (bool)tweenEvent.parms [1];
		if(success)
			routineRunner.StartCoroutine(VibrationObject(0.25f,eContent.item.gameObject,eContent.item.transform.localPosition.x));
		AddEventToButtonReturn (eContent.returnScreen,"HideUpgrade", eContent.gameObject);
	}


	void CreateEffectUpdateSuccess(ItemEfContent eContent)
	{
		fx = CreateEffect ("Animation/Fx/GlowFx/glow item fx.ske","Idle",true);
		fx.layer = 8;
		fx.name = "fxShow";
		fx.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
		fx.transform.parent = eContent.item.transform;
		fx.transform.localPosition = new Vector3(0, 0, -3);
	}


	void AddEventToButtonReturn(UIButton button, string nameEvent, GameObject item)
	{
		EventDelegate eventButton = new EventDelegate(blackSmith.GetComponent<LoadInfoBlacksmith>(), nameEvent);
		eventButton.parameters[0] = new EventDelegate.Parameter(item, "go");
		button.onClick.Add(eventButton);
		item.transform.localScale = Vector3.one;
	}
	void AddEventToButton(UIButton button, string nameEvent, GameObject item)
	{
		EventDelegate eventButton = new EventDelegate(home.GetComponent<LoadInfoCharacter>(), nameEvent);
		eventButton.parameters[0] = new EventDelegate.Parameter(item, "go");
		button.onClick.Add(eventButton);
		item.transform.localScale = Vector3.one;
	}


}
