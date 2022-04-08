using UnityEngine;
using System.Collections;

public class PopupPurchaseContent : MonoBehaviour {

	public UILabel[] name;
	public UILabel[] price;
	public UILabel[] description;
	public UIButton[] button;
	public UISprite[] icon;
	public UISprite[] iconGem;
	public UIButton exit;
	public UISprite disable;
	public UILabel text;
	public ShopItemCfg[] itemShop = new ShopItemCfg[5];
}
