using UnityEngine;
using System.Collections;

public class HintView : BaseView{

	[Inject]
	public ConfigManager config { get; set;}

	[NGUITag]
	public UILabel Hint { get; set;}

	protected override void OnStart ()
	{
		base.OnStart ();
		int indexHint = Random.Range (1, config.hintCfg.hint.Count+1);
		string text = "Tip: "+config.hintCfg.hint [indexHint.ToString()].Text;
		text = text.Replace (";",",");
		Hint.text = text;
		Hint.transform.localPosition = new Vector3 (0, -385,0);
		Hint.width = 500;
		Hint.height = 125;
	}
}
