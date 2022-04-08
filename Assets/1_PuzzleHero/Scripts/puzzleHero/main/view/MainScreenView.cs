using UnityEngine;

public class MainScreenView : IngameBaseView
{
	public UIButton playButton;
	public UIButton settingsButton;

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		playButton = GameObject.Find(MainScreenObjectName.playButton).GetComponent<UIButton>();
		SetupButton(playButton);
		CacheObjectForEventHandling(playButton.gameObject);

		AutoAlignTitle();
	}

	private void AutoAlignTitle()
	{
		UI2DSprite title = GameObject.Find(MainScreenObjectName.title).GetComponent<UI2DSprite>();
		GameObject uiRoot = GameObject.Find("UI Root");
		title.SetAnchor(uiRoot, -311, -110, -13, -10);
		title.leftAnchor.target = uiRoot.transform;
		title.rightAnchor.target = uiRoot.transform;
		title.topAnchor.target = uiRoot.transform;
		title.bottomAnchor.target = uiRoot.transform;
		title.leftAnchor.relative = 1;
		title.bottomAnchor.relative = 1;
		title.ResetAnchors();
		title.UpdateAnchors();
	}
}