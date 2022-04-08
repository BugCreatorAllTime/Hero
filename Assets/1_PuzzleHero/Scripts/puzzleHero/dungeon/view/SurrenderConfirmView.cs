using UnityEngine;

public class SurrenderConfirmView : IngameBaseView
{
	public UIButton no;
	public UIButton surrender;

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		no = GameObject.Find(GuiObjectName.surrenderConfirmPopup_NoButton).GetComponent<UIButton>();
		SetupButton(no);
		CacheObjectForEventHandling(no.gameObject);

		surrender = GameObject.Find(GuiObjectName.surrenderConfirmPopup_SurrenderButton).GetComponent<UIButton>();
		SetupButton(surrender);
		CacheObjectForEventHandling(surrender.gameObject);

		GameObject disable = GameObject.Find(GuiObjectName.surrenderDisable);
		CacheObjectForEventHandling(disable);

		gameObject.SetActive(false);
	}
}