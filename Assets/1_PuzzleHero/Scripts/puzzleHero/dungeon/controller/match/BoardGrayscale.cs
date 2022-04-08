using UnityEngine;

public class BoardGrayscale
{
	private const string greyProperty = "_IsGrey";

	private MatchLogic matchLogic;
	private BoardBackgroundLogic boardBackgroundLogic;
	private UI2DSprite background;
	private UISprite gem;

	public BoardGrayscale(MatchLogic matchLogic, BoardBackgroundLogic boardBackgroundLogic)
	{
		this.matchLogic = matchLogic;
		this.boardBackgroundLogic = boardBackgroundLogic;

		matchLogic.battlePhaseManager.OnBattlePhaseChangedEvent += OnBattlePhaseChanged;
	}

	private void OnBattlePhaseChanged(BattlePhaseManager.BattlePhase phase)
	{
		float isGray = phase == BattlePhaseManager.BattlePhase.Monster ? 1 : 0;

		if (background == null)
		{
			background = boardBackgroundLogic.Background;
			gem = matchLogic.tiles[0].GetComponent<UISprite>();
		}

		background.drawCall.baseMaterial.SetFloat(greyProperty, isGray);
		background.drawCall.dynamicMaterial.SetFloat(greyProperty, isGray);

		gem.drawCall.baseMaterial.SetFloat(greyProperty, isGray);
		gem.drawCall.dynamicMaterial.SetFloat(greyProperty, isGray);
	}
}