using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;

public class CalculatorComboCommand : Command
{
	[Inject]
	public bool combo{ get; set;}

	[Inject]
	public TextFxManager textManager{ get; set;}

	[Inject]
	public SoundManager soundMg { get; set;}

	private static int countCombo = 0;

	public override void Execute()
	{
		if(!combo)
		{
			countCombo = 0;
		} else {
			countCombo++;
			if(countCombo >= 1 && countCombo <= 5)
			{
				soundMg.PlaySound(SoundName.COMBO1_5);
			} else if(countCombo >= 6 && countCombo <= 10){
				soundMg.PlaySound(SoundName.COMBO6_10);
			} else if(countCombo >= 11){
				soundMg.PlaySound(SoundName.COMBO11_UP);
			}
			textManager.ShowTextCombo(countCombo);
		}
	}
}
